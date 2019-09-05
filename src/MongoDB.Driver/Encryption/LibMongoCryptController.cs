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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Serializers;
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
        private readonly AutoEncryptionOptions _autoEncryptionOptions;
        private readonly MongoClient _client;
        private readonly CryptClient _cryptClient;
        private bool _isDisposed = false;
        private bool _isInitialized = false;
        private IMongoCollection<BsonDocument> _keyVaultCollection;
        private readonly MongoClient _mongocryptdClient;

        // constructors
        public LibMongoCryptController(
            MongoClient client,
            ClientEncryptionOptions clientEncryptionOptions) 
            : this(
                client,
                AutoEncryptionOptions.FromClientEncryptionOptions(clientEncryptionOptions),
                client.LibMongoCryptController?.CryptClient) // can be null
        {
        }

        public LibMongoCryptController(
            MongoClient client,
            AutoEncryptionOptions autoEncryptionOptions) 
            : this(
                client,
                autoEncryptionOptions,
                null) // will be created in the base constructor
        {
            // needs only for auto encryption
            _mongocryptdClient = MongoCryptDHelper.CreateClient(_autoEncryptionOptions);
        }

        private LibMongoCryptController(
            MongoClient client,
            AutoEncryptionOptions autoEncryptionOptions,
            CryptClient cryptClient)
        {
            _autoEncryptionOptions = Ensure.IsNotNull(autoEncryptionOptions, nameof(autoEncryptionOptions));
            _client = Ensure.IsNotNull(client, nameof(client));
            _cryptClient = cryptClient ?? CryptClientHelper.CreateCryptClient(autoEncryptionOptions);
        }

        public CryptClient CryptClient => _cryptClient;

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
            if (!_isDisposed)
            {
                _cryptClient.Dispose();
                _isDisposed = true;
            }
        }

        public BsonBinaryData EncryptField(BsonValue value, Guid? keyId, string alternateKeyName, EncryptionAlgorithm encryptionAlgorithm, CancellationToken cancellationToken)
        {
            ThrowIfNotInitializedOrDisposed();

            var wrappedValueBytes = GetWrappedValueBytes(value);
            var alternateKeyNameBytes = GetWrappedAlternateKeyName(alternateKeyName);

            try
            {
                CryptContext context;
                if (keyId.HasValue)
                {
                    context = _cryptClient.StartExplicitEncryptionContext(keyId.Value, encryptionAlgorithm, wrappedValueBytes);
                }
                else if (alternateKeyNameBytes != null)
                {
                    context = _cryptClient.StartExplicitEncryptionContext(alternateKeyNameBytes, encryptionAlgorithm, wrappedValueBytes);
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
            var alternateKeyNameBytes = GetWrappedAlternateKeyName(alternateKeyName);

            try
            {
                CryptContext context;
                if (keyId.HasValue)
                {
                    context = _cryptClient.StartExplicitEncryptionContext(keyId.Value, encryptionAlgorithm, wrappedValueBytes);
                }
                else if (alternateKeyNameBytes != null)
                {
                    context = _cryptClient.StartExplicitEncryptionContext(alternateKeyNameBytes, encryptionAlgorithm, wrappedValueBytes);
                }
                else
                {
                    throw new Exception("Key Id and Alt name cannot both be set.");
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
            if (!_isInitialized)
            {
                _keyVaultCollection = GetKeyVaultCollection(_autoEncryptionOptions, _client);
                _isInitialized = true;
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
            try
            {
                var writerSettings = new BsonBinaryWriterSettings { GuidRepresentation = GuidRepresentation.Unspecified };
                foreach (var document in documents)
                {
                    var documentBytes = document.ToBson(writerSettings: writerSettings);
                    context.Feed(documentBytes);
                }
                context.MarkDone();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private IMongoCollection<BsonDocument> GetKeyVaultCollection(AutoEncryptionOptions autoEncryptionOptions, IMongoClient client)
        {
            var keyVaultClient = autoEncryptionOptions.KeyVaultClient ?? client;
            var keyVaultNamespace = autoEncryptionOptions.KeyVaultNamespace;
            var keyVaultDatabase = keyVaultClient.GetDatabase(keyVaultNamespace.DatabaseNamespace.DatabaseName);
            return keyVaultDatabase.GetCollection<BsonDocument>(keyVaultNamespace.CollectionName);
        }

        private IKmsKeyId GetKmsId(string kmsProvider, IReadOnlyList<string> alternateKeyNames, BsonDocument masterKey)
        {
            IEnumerable<byte[]> alternateKeyNameDocuments = null;
            if (alternateKeyNames != null)
            {
                alternateKeyNameDocuments = alternateKeyNames
                    .Select(c => GetWrappedAlternateKeyName(c));
            }

            switch (kmsProvider)
            {
                case "aws":
                    var customerMasterKey = masterKey["key"].ToString();
                    var region = masterKey["region"].ToString();
                    return alternateKeyNameDocuments != null
                        ? new AwsKeyId(customerMasterKey, region, alternateKeyNameDocuments)
                        : new AwsKeyId(customerMasterKey, region);
                case "local":
                    return alternateKeyNameDocuments != null ? new LocalKeyId(alternateKeyNameDocuments) : new LocalKeyId();
                default:
                    throw new ArgumentException($"Unexpected kmsProvider {kmsProvider}.");
            }
        }

        private byte[] GetWrappedAlternateKeyName(string value)
        {
            return
               !string.IsNullOrWhiteSpace(value)
                   ? new BsonDocument("keyAltName", value).ToBson(serializer: BsonValueSerializer.Instance)
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

        private void ThrowIfNotInitializedOrDisposed()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException($"{nameof(LibMongoCryptController)} must be initialized.");
            }

            if (_isDisposed)
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
        private class CryptClientHelper
        {
            #region static
            public static CryptClient CreateCryptClient(AutoEncryptionOptions autoEncryptionOptions)
            {
                var helper = new CryptClientHelper(autoEncryptionOptions);
                var cryptOptions = helper.CreateCryptOptions();
                return helper.CreateCryptClient(cryptOptions);
            }
            #endregion

            private readonly AutoEncryptionOptions _autoEncryptionOptions;

            private CryptClientHelper(AutoEncryptionOptions autoEncryptionOptions)
            {
                _autoEncryptionOptions = Ensure.IsNotNull(autoEncryptionOptions, nameof(autoEncryptionOptions));
            }

            private CryptClient CreateCryptClient(CryptOptions options)
            {
                return CryptClientFactory.Create(options);
            }

            private CryptOptions CreateCryptOptions()
            {
                var kmsProviders = _autoEncryptionOptions.KmsProviders;
                Dictionary<KmsType, IKmsCredentials> kmsProvidersMap = null;
                if (kmsProviders != null)
                {
                    kmsProvidersMap = new Dictionary<KmsType, IKmsCredentials>();
                    if (kmsProviders.TryGetValue("aws", out var awsProvider))
                    {
                        if (awsProvider.TryGetValue("accessKeyId", out var accessKeyId) &&
                            awsProvider.TryGetValue("secretAccessKey", out var secretAccessKey))
                        {
                            kmsProvidersMap.Add(KmsType.Aws, new AwsKmsCredentials((string)secretAccessKey, (string)accessKeyId));
                        }
                    }
                    if (kmsProviders.TryGetValue("local", out var localProvider))
                    {
                        if (localProvider.TryGetValue("key", out var keyObject) && keyObject is byte[] key)
                        {
                            kmsProvidersMap.Add(KmsType.Local, new LocalKmsCredentials(key));
                        }
                    }
                }

                byte[] schemaBytes = null;
                var schemaMap = _autoEncryptionOptions.SchemaMap;
                if (schemaMap != null)
                {
                    var schemaMapElements = schemaMap.Select(c => new BsonElement(c.Key, c.Value));
                    var schemaDocument = new BsonDocument(schemaMapElements);
                    var writeSettings = new BsonBinaryWriterSettings { GuidRepresentation = GuidRepresentation.Unspecified };
                    schemaBytes = schemaDocument.ToBson(writerSettings: writeSettings);
                }

                return new CryptOptions(kmsProvidersMap, schemaBytes);
            }
        }

        private class MongoCryptDHelper
        {
            #region static
            public static MongoClient CreateClient(AutoEncryptionOptions autoEncryptionOptions)
            {
                var helper = new MongoCryptDHelper(autoEncryptionOptions);
                var client = helper.CreateMongoCryptDClient();
                return client;
            }
            #endregion

            private readonly IReadOnlyDictionary<string, object> _extraOptions;

            private MongoCryptDHelper(AutoEncryptionOptions autoEncryptionOptions)
            {
                Ensure.IsNotNull(autoEncryptionOptions, nameof(autoEncryptionOptions));
                _extraOptions = autoEncryptionOptions.ExtraOptions ?? new Dictionary<string, object>();
            }

            // private methods
            private string CreateMongoCryptDConnectionString()
            {
                if (!_extraOptions.TryGetValue("mongocryptdURI", out var connectionString))
                {
                    connectionString = "mongodb://localhost:27020";
                }

                return connectionString.ToString();
            }

            private MongoClient CreateMongoCryptDClient()
            {
                var connectionString = CreateMongoCryptDConnectionString();

                if (ShouldMongocryptdBeSpawned(out var path, out var args))
                {
                    StartProcess(path, args);
                }

                return new MongoClient(connectionString);
            }

            private bool ShouldMongocryptdBeSpawned(out string path, out string args)
            {
                path = null;
                args = null;
                if (!_extraOptions.TryGetValue("mongocryptdBypassSpawn", out var mongoCryptBypassSpawn) || (!bool.Parse(mongoCryptBypassSpawn.ToString())))
                {
                    if (!_extraOptions.TryGetValue("mongocryptdSpawnPath", out var objPath))
                    {
                        path = string.Empty; // look at the PATH env variable
                    }
                    else
                    {
                        path = objPath.ToString();
                    }

                    if (!Path.HasExtension(path))
                    {
                        string fileName = "mongocryptd.exe";
                        path = Path.Combine(path, fileName);
                    }

                    args = string.Empty;
                    if (_extraOptions.TryGetValue("mongocryptdSpawnArgs", out var mongocryptdSpawnArgs))
                    {
                        string trimStartHyphens(string str) => str.TrimStart('-').TrimStart('-');
                        switch (mongocryptdSpawnArgs)
                        {
                            case string str:
                                var options = str.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                                if (!options.Any(o => o.Contains("idleShutdownTimeoutSecs")))
                                {
                                    args += "--idleShutdownTimeoutSecs 60 ";
                                }
                                args += str;
                                break;
                            case IDictionary dictionary:
                                foreach (var key in dictionary.Keys)
                                {
                                    args += $"--{trimStartHyphens(key.ToString())} {dictionary[key]}".TrimEnd(';') + " ";
                                }
                                break;
                            case IEnumerable enumerable:
                                foreach (var item in enumerable)
                                {
                                    args += $"--{trimStartHyphens(item.ToString())}".TrimEnd(';') + " ";
                                }
                                break;
                            default:
                                args = mongocryptdSpawnArgs.ToString();
                                break;
                        }
                    }
                    else
                    {
                        args += "--idleShutdownTimeoutSecs 60";
                    }

                    return true;
                }

                return false;
            }

            private void StartProcess(string path, string args)
            {
                try
                {
                    using (Process mongoCryptD = new Process())
                    {
                        mongoCryptD.StartInfo.Arguments = args;
                        mongoCryptD.StartInfo.FileName = path;
                        mongoCryptD.StartInfo.CreateNoWindow = true;
                        mongoCryptD.StartInfo.UseShellExecute = false;

                        if (!mongoCryptD.Start())
                        {
                            // skip it. This case can happen if no new process resource is started
                            // (for example, if an existing process is reused)
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new MongoClientException("Exception starting mongocryptd process. Is mongocryptd on the system path?", ex);
                }
            }
        }
    }
}
