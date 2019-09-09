/* Copyright 2019-present MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol;
using MongoDB.Libmongocrypt;

namespace MongoDB.Driver.Encryption
{
    internal sealed class LibMongoCryptController : IBinaryDocumentFieldDecryptor, IBinaryCommandFieldEncryptor, IDisposable
    {
        #region static
        private static bool AcceptAnyCertificate(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
        #endregion

        // private fields
        private readonly MongoClient _client;
        private CryptClient _cryptClient;
        private bool _disposed;
        private readonly EncryptionMode _encryptionMode;
        private readonly IReadOnlyDictionary<string, object> _extraOptions;
        private bool _initialized = false;
        private readonly IMongoClient _keyVaultClient;
        private IMongoCollection<BsonDocument> _keyVaultCollection;
        private readonly CollectionNamespace _keyVaultNamespace;
        private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, object>> _kmsProviders;
        private MongoClient _mongocryptdClient;
        private readonly IReadOnlyDictionary<string, BsonDocument> _schemaMap;

        // constructors
        public LibMongoCryptController(
            MongoClient client,
            ClientEncryptionOptions clientEncryptionOptions)
        {
            _client = Ensure.IsNotNull(client, nameof(client));
            _encryptionMode = EncryptionMode.ClientEncryption;
            _keyVaultClient = Ensure.IsNotNull(clientEncryptionOptions.KeyVaultClient, "keyVaultClient");
            _keyVaultNamespace = Ensure.IsNotNull(clientEncryptionOptions.KeyVaultNamespace, "keyVaultNamespace");
            _kmsProviders = Ensure.IsNotNull(clientEncryptionOptions.KmsProviders, "kmsProviders");
        }

        public LibMongoCryptController(
            MongoClient client,
            AutoEncryptionOptions autoEncryptionOptions)
        {
            _client = Ensure.IsNotNull(client, nameof(client));
            Ensure.IsNotNull(client.Cluster, "cluster");
            _cryptClient = Ensure.IsNotNull(client.Cluster.CryptClient, "cryptClient");
            _encryptionMode = EncryptionMode.Auto;
            _extraOptions = autoEncryptionOptions.ExtraOptions;
            _keyVaultClient = autoEncryptionOptions.KeyVaultClient;
            _keyVaultNamespace = Ensure.IsNotNull(autoEncryptionOptions.KeyVaultNamespace, "keyVaultNamespace");
            _kmsProviders = Ensure.IsNotNull(autoEncryptionOptions.KmsProviders, "kmsProviders");
            _schemaMap = autoEncryptionOptions.SchemaMap;
        }

        // public methods
        public BsonBinaryData CreateDataKey(
            string kmsProvider,
            IReadOnlyList<string> alternateKeyNames,
            BsonDocument masterKey,
            CancellationToken cancellationToken)
        {
            ThrowIfNotInitializedOrDisposed();

            var kmsKeyId = GetKmsId(kmsProvider, alternateKeyNames, masterKey);

            byte[] keyDocumentBytes;
            try
            {
                using (var context = _cryptClient.StartCreateDataKeyContext(kmsKeyId))
                {
                    keyDocumentBytes = ProcessStates(context, _keyVaultCollection.Database.DatabaseNamespace.DatabaseName, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                throw new MongoClientException(ex.Message, ex);
            }

            var keyDocument = new RawBsonDocument(keyDocumentBytes);
            _keyVaultCollection.InsertOne(keyDocument, cancellationToken: cancellationToken);
            return keyDocument["_id"].AsBsonBinaryData;
        }

        public async Task<BsonBinaryData> CreateDataKeyAsync(
            string kmsProvider,
            IReadOnlyList<string> alternateKeyNames,
            BsonDocument masterKey,
            CancellationToken cancellationToken)
        {
            ThrowIfNotInitializedOrDisposed();

            var kmsKeyId = GetKmsId(kmsProvider, alternateKeyNames, masterKey);

            byte[] keyDocumentBytes;
            try
            {
                using (var context = _cryptClient.StartCreateDataKeyContext(kmsKeyId))
                {
                    keyDocumentBytes = await ProcessStatesAsync(context, _keyVaultCollection.Database.DatabaseNamespace.DatabaseName, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                throw new MongoClientException(ex.Message, ex);
            }

            var keyDocument = new RawBsonDocument(keyDocumentBytes);
            await _keyVaultCollection.InsertOneAsync(keyDocument, cancellationToken: cancellationToken).ConfigureAwait(false);
            return keyDocument["_id"].AsBsonBinaryData;
        }

        public BsonBinaryData DecryptField(BsonBinaryData wrappedBinaryValue, CancellationToken cancellationToken)
        {
            ThrowIfNotInitializedOrDisposed();

            var wrappedValueBytes = GetWrappedValueBytes(wrappedBinaryValue);

            try
            {
                using (var context = _cryptClient.StartExplicitDecryptionContext(wrappedValueBytes))
                {
                    return ProcessStates(context, databaseName: null, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                throw new MongoClientException(ex.Message, ex);
            }
        }

        public async Task<BsonBinaryData> DecryptFieldAsync(BsonBinaryData wrappedBinaryValue, CancellationToken cancellationToken)
        {
            ThrowIfNotInitializedOrDisposed();

            var wrappedValueBytes = GetWrappedValueBytes(wrappedBinaryValue);

            try
            {
                using (var context = _cryptClient.StartExplicitDecryptionContext(wrappedValueBytes))
                {
                    return await ProcessStatesAsync(context, databaseName: null, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                throw new MongoClientException(ex.Message, ex);
            }
        }

        public byte[] DecryptFields(byte[] encryptedDocumentBytes, CancellationToken cancellationToken)
        {
            ThrowIfNotInitializedOrDisposed();

            try
            {
                using (var context = _cryptClient.StartDecryptionContext(encryptedDocumentBytes))
                {
                    return ProcessStates(context, databaseName: null, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                throw new MongoClientException(ex.Message, ex);
            }
        }

        public async Task<byte[]> DecryptFieldsAsync(byte[] encryptedDocumentBytes, CancellationToken cancellationToken)
        {
            ThrowIfNotInitializedOrDisposed();

            try
            {
                using (var context = _cryptClient.StartDecryptionContext(encryptedDocumentBytes))
                {
                    return await ProcessStatesAsync(context, databaseName: null, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                throw new MongoClientException(ex.Message, ex);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _cryptClient.Dispose();
                _disposed = true;
            }
        }

        public BsonBinaryData EncryptField(BsonValue value, Guid? keyId, string alternateKeyName, EncryptionAlgorithm encryptionAlgorithm, CancellationToken cancellationToken)
        {
            ThrowIfNotInitializedOrDisposed();

            var wrappedValueBytes = GetWrappedValueBytes(value);
            var wrappedAlternateKeyNameBytes = GetWrappedAlternateKeyName(alternateKeyName);

            try
            {
                CryptContext context;
                if (keyId.HasValue)
                {
                    context = _cryptClient.StartExplicitEncryptionContext(keyId.Value, encryptionAlgorithm, wrappedValueBytes);
                }
                else if (wrappedAlternateKeyNameBytes != null)
                {
                    context = _cryptClient.StartExplicitEncryptionContext(wrappedAlternateKeyNameBytes, encryptionAlgorithm, wrappedValueBytes);
                }
                else
                {
                    throw new Exception("Key Id and Alternate key name cannot both be set.");
                }

                using (context)
                {
                    var wrappedBytes = ProcessStates(context, databaseName: null, cancellationToken);
                    return UnwrapEncryptedValue(wrappedBytes);
                }
            }
            catch (Exception ex)
            {
                throw new MongoClientException(ex.Message, ex);
            }
        }

        public async Task<BsonBinaryData> EncryptFieldAsync(BsonValue value, Guid? keyId, string alternateKeyName, EncryptionAlgorithm encryptionAlgorithm, CancellationToken cancellationToken)
        {
            ThrowIfNotInitializedOrDisposed();

            var wrappedValueBytes = GetWrappedValueBytes(value);
            var wrappedAlternateKeyNameBytes = GetWrappedAlternateKeyName(alternateKeyName);

            try
            {
                CryptContext context;
                if (keyId.HasValue)
                {
                    context = _cryptClient.StartExplicitEncryptionContext(keyId.Value, encryptionAlgorithm, wrappedValueBytes);
                }
                else if (wrappedAlternateKeyNameBytes != null)
                {
                    context = _cryptClient.StartExplicitEncryptionContext(wrappedAlternateKeyNameBytes, encryptionAlgorithm, wrappedValueBytes);
                }
                else
                {
                    throw new Exception("Key Id and Alternate key name cannot both be set.");
                }

                using (context)
                {
                    var wrappedBytes = await ProcessStatesAsync(context, databaseName: null, cancellationToken).ConfigureAwait(false);
                    return UnwrapEncryptedValue(wrappedBytes);
                }
            }
            catch (Exception ex)
            {
                throw new MongoClientException(ex.Message, ex);
            }
        }

        public byte[] EncryptFields(string databaseName, byte[] unencryptedCommandBytes, CancellationToken cancellationToken)
        {
            ThrowIfNotInitializedOrDisposed();

            try
            {
                using (var context = _cryptClient.StartEncryptionContext(databaseName, unencryptedCommandBytes))
                {
                    return ProcessStates(context, databaseName, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                throw new MongoClientException(ex.Message, ex);
            }
        }

        public async Task<byte[]> EncryptFieldsAsync(string databaseName, byte[] unencryptedCommandBytes, CancellationToken cancellationToken)
        {
            ThrowIfNotInitializedOrDisposed();

            try
            {
                using (var context = _cryptClient.StartEncryptionContext(databaseName, unencryptedCommandBytes))
                {
                    return await ProcessStatesAsync(context, databaseName, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                throw new MongoClientException(ex.Message, ex);
            }
        }

        public void Initialize()
        {
            if (!_initialized)
            {
                _keyVaultCollection = GetKeyVaultCollection();
                _initialized = true;

                if (_encryptionMode == EncryptionMode.Auto)
                {
                    // cryptClient has been provided via constructor from the cluster
                    _mongocryptdClient = MongocryptdHelper.CreateClientIfRequired(_extraOptions);
                }
                else
                {
                    _cryptClient = CryptClientHelper.CreateCryptClientIfRequired(_kmsProviders, _schemaMap);
                    // mongocryptd is not required for ClientEncryption
                }
            }
        }

        // private methods
        private void FeedResult(CryptContext context, BsonDocument document)
        {
            var writerSettings = new BsonBinaryWriterSettings { GuidRepresentation = GuidRepresentation.Unspecified };
            var documentBytes = document.ToBson(writerSettings: writerSettings);
            context.Feed(documentBytes);
            context.MarkDone();
        }

        private void FeedResults(CryptContext context, IEnumerable<BsonDocument> documents)
        {
            var writerSettings = new BsonBinaryWriterSettings { GuidRepresentation = GuidRepresentation.Unspecified };
            foreach (var document in documents)
            {
                var documentBytes = document.ToBson(writerSettings: writerSettings);
                context.Feed(documentBytes);
            }
            context.MarkDone();
        }

        private IMongoCollection<BsonDocument> GetKeyVaultCollection()
        {
            var keyVaultClient = _keyVaultClient ?? _client;
            var keyVaultDatabase = keyVaultClient.GetDatabase(_keyVaultNamespace.DatabaseNamespace.DatabaseName);
            return keyVaultDatabase.GetCollection<BsonDocument>(_keyVaultNamespace.CollectionName);
        }

        private IKmsKeyId GetKmsId(string kmsProvider, IReadOnlyList<string> alternateKeyNames, BsonDocument masterKey)
        {
            IEnumerable<byte[]> wrappedAlternateKeyNamesBytes = null;
            if (alternateKeyNames != null)
            {
                wrappedAlternateKeyNamesBytes = alternateKeyNames.Select(GetWrappedAlternateKeyName);
            }

            switch (kmsProvider)
            {
                case "aws":
                    var customerMasterKey = masterKey["key"].ToString();
                    var region = masterKey["region"].ToString();
                    return wrappedAlternateKeyNamesBytes != null
                        ? new AwsKeyId(customerMasterKey, region, wrappedAlternateKeyNamesBytes)
                        : new AwsKeyId(customerMasterKey, region);
                case "local":
                    return wrappedAlternateKeyNamesBytes != null ? new LocalKeyId(wrappedAlternateKeyNamesBytes) : new LocalKeyId();
                default:
                    throw new ArgumentException($"Unexpected kmsProvider {kmsProvider}.");
            }
        }

        private byte[] GetWrappedAlternateKeyName(string value)
        {
            return
               !string.IsNullOrWhiteSpace(value)
                   ? new BsonDocument("keyAltName", value).ToBson()
                   : null;
        }

        private byte[] GetWrappedValueBytes(BsonValue value)
        {
            var writerSettings = BsonBinaryWriterSettings.Defaults.Clone();
            writerSettings.GuidRepresentation = GuidRepresentation.Unspecified;
            var wrappedValue = new BsonDocument("v", value);
            return wrappedValue.ToBson(serializer: BsonValueSerializer.Instance, writerSettings: writerSettings);
        }

        private void ProcessNeedCollectionInfoState(CryptContext context, string databaseName, CancellationToken cancellationToken)
        {
            var database = _client.GetDatabase(databaseName);
            var filterBytes = context.GetOperation().ToArray();
            var filterDocument = new RawBsonDocument(filterBytes);
            var filter = new BsonDocumentFilterDefinition<BsonDocument>(filterDocument);
            var options = new ListCollectionsOptions { Filter = filter };
            var cursor = database.ListCollections(options, cancellationToken);
            var results = cursor.ToList(cancellationToken);
            FeedResults(context, results);
        }

        private async Task ProcessNeedCollectionInfoStateAsync(CryptContext context, string databaseName, CancellationToken cancellationToken)
        {
            var database = _client.GetDatabase(databaseName);
            var filterBytes = context.GetOperation().ToArray();
            var filterDocument = new RawBsonDocument(filterBytes);
            var filter = new BsonDocumentFilterDefinition<BsonDocument>(filterDocument);
            var options = new ListCollectionsOptions { Filter = filter };
            var cursor = await database.ListCollectionsAsync(options, cancellationToken).ConfigureAwait(false);
            var results = await cursor.ToListAsync(cancellationToken).ConfigureAwait(false);
            FeedResults(context, results);
        }

        private void ProcessNeedKmsState(CryptContext context, CancellationToken cancellationToken)
        {
            var requests = context.GetKmsMessageRequests();
            foreach (var request in requests)
            {
                SendKmsRequest(request, cancellationToken);
            }
            requests.MarkDone();
        }

        private async Task ProcessNeedKmsStateAsync(CryptContext context, CancellationToken cancellationToken)
        {
            var requests = context.GetKmsMessageRequests();
            foreach (var request in requests)
            {
                await SendKmsRequestAsync(request, cancellationToken).ConfigureAwait(false);
            }
            requests.MarkDone();
        }

        private void ProcessNeedMongoKeysState(CryptContext context, CancellationToken cancellationToken)
        {
            var filterBytes = context.GetOperation().ToArray();
            var filterDocument = new RawBsonDocument(filterBytes);
            var filter = new BsonDocumentFilterDefinition<BsonDocument>(filterDocument);
            var cursor = _keyVaultCollection.FindSync(filter, cancellationToken: cancellationToken);
            var results = cursor.ToList(cancellationToken);
            FeedResults(context, results);
        }

        private async Task ProcessNeedMongoKeysStateAsync(CryptContext context, CancellationToken cancellationToken)
        {
            var filterBytes = context.GetOperation().ToArray();
            var filterDocument = new RawBsonDocument(filterBytes);
            var filter = new BsonDocumentFilterDefinition<BsonDocument>(filterDocument);
            var cursor = await _keyVaultCollection.FindAsync(filter, cancellationToken: cancellationToken).ConfigureAwait(false);
            var results = await cursor.ToListAsync(cancellationToken).ConfigureAwait(false);
            FeedResults(context, results);
        }

        private void ProcessNeedMongoMarkingsState(CryptContext context, string databaseName, CancellationToken cancellationToken)
        {
            var database = _mongocryptdClient.GetDatabase(databaseName);
            var commandBytes = context.GetOperation().ToArray();
            var commandDocument = new RawBsonDocument(commandBytes);
            var command = new BsonDocumentCommand<BsonDocument>(commandDocument);
            var response = database.RunCommand(command, cancellationToken: cancellationToken);
            RestoreDbNodeInResponse(commandDocument, response);
            FeedResult(context, response);
        }

        private async Task ProcessNeedMongoMarkingsStateAsync(CryptContext context, string databaseName, CancellationToken cancellationToken)
        {
            var database = _mongocryptdClient.GetDatabase(databaseName);
            var commandBytes = context.GetOperation().ToArray();
            var commandDocument = new RawBsonDocument(commandBytes);
            var command = new BsonDocumentCommand<BsonDocument>(commandDocument);
            var response = await database.RunCommandAsync(command, cancellationToken: cancellationToken).ConfigureAwait(false);
            RestoreDbNodeInResponse(commandDocument, response);
            FeedResult(context, response);
        }

        private byte[] ProcessReadyState(CryptContext context)
        {
            return context.FinalizeForEncryption().ToArray();
        }

        private byte[] ProcessStates(CryptContext context, string databaseName, CancellationToken cancellationToken)
        {
            byte[] result = null;
            while (true)
            {
                switch (context.State)
                {
                    case CryptContext.StateCode.MONGOCRYPT_CTX_DONE:
                        return result;
                    case CryptContext.StateCode.MONGOCRYPT_CTX_NEED_KMS:
                        ProcessNeedKmsState(context, cancellationToken);
                        break;
                    case CryptContext.StateCode.MONGOCRYPT_CTX_NEED_MONGO_COLLINFO:
                        ProcessNeedCollectionInfoState(context, databaseName, cancellationToken);
                        break;
                    case CryptContext.StateCode.MONGOCRYPT_CTX_NEED_MONGO_KEYS:
                        ProcessNeedMongoKeysState(context, cancellationToken);
                        break;
                    case CryptContext.StateCode.MONGOCRYPT_CTX_NEED_MONGO_MARKINGS:
                        ThrowIfNotAutoEncryption(context.State);
                        ProcessNeedMongoMarkingsState(context, databaseName, cancellationToken);
                        break;
                    case CryptContext.StateCode.MONGOCRYPT_CTX_READY:
                        result = ProcessReadyState(context);
                        break;
                    default:
                        throw new InvalidOperationException($"Unexpected context state: {context.State}.");
                }
            }
        }

        private async Task<byte[]> ProcessStatesAsync(CryptContext context, string databaseName, CancellationToken cancellationToken)
        {
            byte[] result = null;
            while (true)
            {
                switch (context.State)
                {
                    case CryptContext.StateCode.MONGOCRYPT_CTX_DONE:
                        return result;
                    case CryptContext.StateCode.MONGOCRYPT_CTX_NEED_KMS:
                        await ProcessNeedKmsStateAsync(context, cancellationToken).ConfigureAwait(false);
                        break;
                    case CryptContext.StateCode.MONGOCRYPT_CTX_NEED_MONGO_COLLINFO:
                        await ProcessNeedCollectionInfoStateAsync(context, databaseName, cancellationToken).ConfigureAwait(false);
                        break;
                    case CryptContext.StateCode.MONGOCRYPT_CTX_NEED_MONGO_KEYS:
                        await ProcessNeedMongoKeysStateAsync(context, cancellationToken).ConfigureAwait(false);
                        break;
                    case CryptContext.StateCode.MONGOCRYPT_CTX_NEED_MONGO_MARKINGS:
                        ThrowIfNotAutoEncryption(context.State);
                        await ProcessNeedMongoMarkingsStateAsync(context, databaseName, cancellationToken).ConfigureAwait(false);
                        break;
                    case CryptContext.StateCode.MONGOCRYPT_CTX_READY:
                        result = ProcessReadyState(context); // no async version needed
                        break;
                    default:
                        throw new InvalidOperationException($"Unexpected context state: {context.State}.");
                }
            }
        }

        private void SendKmsRequest(KmsRequest request, CancellationToken cancellation)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(request.Endpoint, 443);

            var validationCallback = new RemoteCertificateValidationCallback(AcceptAnyCertificate); // TODO: what validation needs to be done?

            using (var networkStream = new NetworkStream(socket, ownsSocket: true))
            using (var sslStream = new SslStream(networkStream, leaveInnerStreamOpen: false, validationCallback))
            {
#if NETSTANDARD1_5
                sslStream.AuthenticateAsClientAsync(request.Endpoint).ConfigureAwait(false).GetAwaiter().GetResult();
#else
                sslStream.AuthenticateAsClient(request.Endpoint);
#endif

                var requestBytes = request.Message.ToArray();
                sslStream.Write(requestBytes);

                var buffer = new byte[4096];
                while (request.BytesNeeded > 0)
                {
                    var count = sslStream.Read(buffer, 0, buffer.Length);
                    var responseBytes = new byte[count];
                    Buffer.BlockCopy(buffer, 0, responseBytes, 0, count);
                    request.Feed(responseBytes);
                }
            }
        }

        private async Task SendKmsRequestAsync(KmsRequest request, CancellationToken cancellation)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
#if NETSTANDARD1_5
            await socket.ConnectAsync(request.Endpoint, 443).ConfigureAwait(false);
#else
            await Task.Factory.FromAsync(socket.BeginConnect(request.Endpoint, 443, null, null), socket.EndConnect).ConfigureAwait(false);
#endif

            var validationCallback = new RemoteCertificateValidationCallback(AcceptAnyCertificate); // TODO: what validation needs to be done?

            using (var networkStream = new NetworkStream(socket, ownsSocket: true))
            using (var sslStream = new SslStream(networkStream, leaveInnerStreamOpen: false, validationCallback))
            {
                await sslStream.AuthenticateAsClientAsync(request.Endpoint).ConfigureAwait(false);

                var requestBytes = request.Message.ToArray();
                await sslStream.WriteAsync(requestBytes, 0, requestBytes.Length).ConfigureAwait(false);

                var buffer = new byte[4096];
                while (request.BytesNeeded > 0)
                {
                    var count = await sslStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                    var responseBytes = new byte[count];
                    Buffer.BlockCopy(buffer, 0, responseBytes, 0, count);
                    request.Feed(responseBytes);
                }
            }
        }

        private void RestoreDbNodeInResponse(BsonDocument request, BsonDocument response)
        {
            if (request.TryGetElement("$db", out var db))
            {
                var result = response["result"].AsBsonDocument;
                if (!result.Contains("$db"))
                {
                    result.Add(db);
                }
            }
        }

        private void ThrowIfNotAutoEncryption(CryptContext.StateCode state)
        {
            if (_encryptionMode != EncryptionMode.Auto)
            {
                throw new InvalidOperationException($"{state} is available only for auto encryption.");
            }
        }

        private void ThrowIfNotInitializedOrDisposed()
        {
            if (!_initialized)
            {
                throw new InvalidOperationException($"{nameof(LibMongoCryptController)} must be initialized.");
            }

            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        private BsonBinaryData UnwrapEncryptedValue(byte[] encryptedWrappedBytes)
        {
            var rawDocument = new RawBsonDocument(encryptedWrappedBytes);
            var encryptedBytes = rawDocument["v"].AsByteArray;
            return new BsonBinaryData(encryptedBytes, BsonBinarySubType.Encrypted);
        }

        // nested types
        private enum EncryptionMode
        {
            Auto,
            ClientEncryption
        }
    }
}
