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
    internal abstract class LibMongoCryptControllerBase
    {
        // protected fields
        protected readonly CryptClient _cryptClient;
        protected readonly IMongoClient _keyVaultClient;
        protected Lazy<IMongoCollection<BsonDocument>> _keyVaultCollection;
        protected readonly CollectionNamespace _keyVaultNamespace;

        // constructors
        protected LibMongoCryptControllerBase(
             CryptClient cryptClient,
             IMongoClient keyVaultClient,
             CollectionNamespace keyVaultNamespace)
        {
            _cryptClient = cryptClient;
            _keyVaultClient = keyVaultClient; // _keyVaultClient might not be fully constructed at this point, don't call any instance methods on it yet
            _keyVaultNamespace = keyVaultNamespace;
            _keyVaultCollection = new Lazy<IMongoCollection<BsonDocument>>(GetKeyVaultCollection); // delay use _keyVaultClient
        }

        // protected methods
        protected void FeedResult(CryptContext context, BsonDocument document)
        {
            var writerSettings = new BsonBinaryWriterSettings { GuidRepresentation = GuidRepresentation.Unspecified };
            var documentBytes = document.ToBson(writerSettings: writerSettings);
            context.Feed(documentBytes);
            context.MarkDone();
        }

        protected void FeedResults(CryptContext context, IEnumerable<BsonDocument> documents)
        {
            var writerSettings = new BsonBinaryWriterSettings { GuidRepresentation = GuidRepresentation.Unspecified };
            foreach (var document in documents)
            {
                var documentBytes = document.ToBson(writerSettings: writerSettings);
                context.Feed(documentBytes);
            }
            context.MarkDone();
        }

        protected virtual void ProcessNeedCollectionInfoState(CryptContext context, string databaseName, CancellationToken cancellationToken)
        {
            throw new NotSupportedException($"{GetType().Name} does not support {nameof(ProcessNeedCollectionInfoState)}.");
        }

        protected virtual Task ProcessNeedCollectionInfoStateAsync(CryptContext context, string databaseName, CancellationToken cancellationToken)
        {
            throw new NotSupportedException($"{GetType().Name} does not support {nameof(ProcessNeedCollectionInfoStateAsync)}.");
        }

        protected virtual void ProcessNeedMongoMarkingsState(CryptContext context, string databaseName, CancellationToken cancellationToken)
        {
            throw new NotSupportedException($"{GetType().Name} does not support {nameof(ProcessNeedMongoMarkingsState)}.");
        }

        protected virtual Task ProcessNeedMongoMarkingsStateAsync(CryptContext context, string databaseName, CancellationToken cancellationToken)
        {
            throw new NotSupportedException($"{GetType().Name} does not support {nameof(ProcessNeedMongoMarkingsStateAsync)}.");
        }

        protected byte[] ProcessStates(CryptContext context, string databaseName, CancellationToken cancellationToken)
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

        protected async Task<byte[]> ProcessStatesAsync(CryptContext context, string databaseName, CancellationToken cancellationToken)
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

        // private methods
        private IMongoCollection<BsonDocument> GetKeyVaultCollection()
        {
            var keyVaultDatabase = _keyVaultClient.GetDatabase(_keyVaultNamespace.DatabaseNamespace.DatabaseName);
            return keyVaultDatabase.GetCollection<BsonDocument>(_keyVaultNamespace.CollectionName);
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
            var cursor = _keyVaultCollection.Value.FindSync(filter, cancellationToken: cancellationToken);
            var results = cursor.ToList(cancellationToken);
            FeedResults(context, results);
        }

        private async Task ProcessNeedMongoKeysStateAsync(CryptContext context, CancellationToken cancellationToken)
        {
            var filterBytes = context.GetOperation().ToArray();
            var filterDocument = new RawBsonDocument(filterBytes);
            var filter = new BsonDocumentFilterDefinition<BsonDocument>(filterDocument);
            var cursor = await _keyVaultCollection.Value.FindAsync(filter, cancellationToken: cancellationToken).ConfigureAwait(false);
            var results = await cursor.ToListAsync(cancellationToken).ConfigureAwait(false);
            FeedResults(context, results);
        }

        private byte[] ProcessReadyState(CryptContext context)
        {
            return context.FinalizeForEncryption().ToArray();
        }

        private void SendKmsRequest(KmsRequest request, CancellationToken cancellation)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(request.Endpoint, 443);

            using (var networkStream = new NetworkStream(socket, ownsSocket: true))
            using (var sslStream = new SslStream(networkStream, leaveInnerStreamOpen: false))
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

            using (var networkStream = new NetworkStream(socket, ownsSocket: true))
            using (var sslStream = new SslStream(networkStream, leaveInnerStreamOpen: false))
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
    }

    internal sealed class AutoEncryptionLibMongoCryptController : LibMongoCryptControllerBase, IBinaryDocumentFieldDecryptor, IBinaryCommandFieldEncryptor
    {
        // private fields
        private readonly IMongoClient _client;
        private IMongoClient _mongocryptdClient;
        private MongocryptdFactory _mongocryptdFactory;

        // constructors
        public AutoEncryptionLibMongoCryptController(
            IMongoClient client,
            CryptClient cryptClient,
            AutoEncryptionOptions autoEncryptionOptions)
            : base(
                  Ensure.IsNotNull(cryptClient, nameof(cryptClient)),
                  Ensure.IsNotNull(autoEncryptionOptions, nameof(autoEncryptionOptions)).KeyVaultClient ?? client,
                  Ensure.IsNotNull(Ensure.IsNotNull(autoEncryptionOptions, nameof(autoEncryptionOptions)).KeyVaultNamespace, nameof(autoEncryptionOptions.KeyVaultNamespace)))
        {
            _client = Ensure.IsNotNull(client, nameof(client)); // _client might not be fully constructed at this point, don't call any instance methods on it yet
            _mongocryptdFactory = new MongocryptdFactory(autoEncryptionOptions.ExtraOptions);
            _mongocryptdClient = _mongocryptdFactory.CreateMongocryptdClient();
        }

        // public methods
        public byte[] DecryptFields(byte[] encryptedDocumentBytes, CancellationToken cancellationToken)
        {
            try
            {
                using (var context = _cryptClient.StartDecryptionContext(encryptedDocumentBytes))
                {
                    return ProcessStates(context, databaseName: null, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                throw new MongoEncryptionException($"Exception in encryption library: {ex.Message}.", ex);
            }
        }

        public async Task<byte[]> DecryptFieldsAsync(byte[] encryptedDocumentBytes, CancellationToken cancellationToken)
        {
            try
            {
                using (var context = _cryptClient.StartDecryptionContext(encryptedDocumentBytes))
                {
                    return await ProcessStatesAsync(context, databaseName: null, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                throw new MongoEncryptionException($"Exception in encryption library: {ex.Message}.", ex);
            }
        }

        public byte[] EncryptFields(string databaseName, byte[] unencryptedCommandBytes, CancellationToken cancellationToken)
        {
            try
            {
                using (var context = _cryptClient.StartEncryptionContext(databaseName, unencryptedCommandBytes))
                {
                    return ProcessStates(context, databaseName, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                throw new MongoEncryptionException($"Exception in encryption library: {ex.Message}.", ex);
            }
        }

        public async Task<byte[]> EncryptFieldsAsync(string databaseName, byte[] unencryptedCommandBytes, CancellationToken cancellationToken)
        {
            try
            {
                using (var context = _cryptClient.StartEncryptionContext(databaseName, unencryptedCommandBytes))
                {
                    return await ProcessStatesAsync(context, databaseName, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                throw new MongoEncryptionException($"Exception in encryption library: {ex.Message}.", ex);
            }
        }

        // protected methods
        protected override void ProcessNeedCollectionInfoState(CryptContext context, string databaseName, CancellationToken cancellationToken)
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

        protected override async Task ProcessNeedCollectionInfoStateAsync(CryptContext context, string databaseName, CancellationToken cancellationToken)
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

        protected override void ProcessNeedMongoMarkingsState(CryptContext context, string databaseName, CancellationToken cancellationToken)
        {
            var database = _mongocryptdClient.GetDatabase(databaseName);
            var commandBytes = context.GetOperation().ToArray();
            var commandDocument = new RawBsonDocument(commandBytes);
            var command = new BsonDocumentCommand<BsonDocument>(commandDocument);

            BsonDocument response = null;
            for (var attempt = 1; response == null; attempt++)
            {
                try
                {
                    response = database.RunCommand(command, cancellationToken: cancellationToken);
                }
                catch (TimeoutException) when (attempt == 1)
                {
                    _mongocryptdFactory.SpawnMongocryptdProcessIfRequired();
                }
            }

            RestoreDbNodeInResponse(commandDocument, response);
            FeedResult(context, response);
        }

        protected override async Task ProcessNeedMongoMarkingsStateAsync(CryptContext context, string databaseName, CancellationToken cancellationToken)
        {
            var database = _mongocryptdClient.GetDatabase(databaseName);
            var commandBytes = context.GetOperation().ToArray();
            var commandDocument = new RawBsonDocument(commandBytes);
            var command = new BsonDocumentCommand<BsonDocument>(commandDocument);

            BsonDocument response = null;
            for (var attempt = 1; response == null; attempt++)
            {
                try
                {
                    response = await database.RunCommandAsync(command, cancellationToken: cancellationToken).ConfigureAwait(false);
                }
                catch (TimeoutException) when (attempt == 1)
                {
                    _mongocryptdFactory.SpawnMongocryptdProcessIfRequired();
                }
            }

            RestoreDbNodeInResponse(commandDocument, response);
            FeedResult(context, response);
        }

        // private methods
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
    }

    internal sealed class ExplicitEncryptionLibMongoCryptController : LibMongoCryptControllerBase
    {
        // constructors
        public ExplicitEncryptionLibMongoCryptController(
            CryptClient cryptClient,
            ClientEncryptionOptions clientEncryptionOptions)
            : base(
                  Ensure.IsNotNull(cryptClient, nameof(cryptClient)),
                  Ensure.IsNotNull(Ensure.IsNotNull(clientEncryptionOptions, nameof(clientEncryptionOptions)).KeyVaultClient, nameof(clientEncryptionOptions.KeyVaultClient)),
                  Ensure.IsNotNull(Ensure.IsNotNull(clientEncryptionOptions, nameof(clientEncryptionOptions)).KeyVaultNamespace, nameof(clientEncryptionOptions.KeyVaultNamespace)))
        {
        }

        // public methods
        public Guid CreateDataKey(
            string kmsProvider,
            IReadOnlyList<string> alternateKeyNames,
            BsonDocument masterKey,
            CancellationToken cancellationToken)
        {
            var kmsKeyId = GetKmsKeyId(kmsProvider, alternateKeyNames, masterKey);

            byte[] wrappedKeyBytes;
            try
            {
                using (var context = _cryptClient.StartCreateDataKeyContext(kmsKeyId))
                {
                    wrappedKeyBytes = ProcessStates(context, _keyVaultNamespace.DatabaseNamespace.DatabaseName, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                throw new MongoEncryptionException($"Exception in encryption library: {ex.Message}.", ex);
            }

            var wrappedKeyDocument = new RawBsonDocument(wrappedKeyBytes);
            _keyVaultCollection.Value.InsertOne(wrappedKeyDocument, cancellationToken: cancellationToken);
            var keyBytes = wrappedKeyDocument["_id"].AsBsonBinaryData.Bytes;
            return GuidConverter.FromBytes(keyBytes, GuidRepresentation.Standard);
        }

        public async Task<Guid> CreateDataKeyAsync(
            string kmsProvider,
            IReadOnlyList<string> alternateKeyNames,
            BsonDocument masterKey,
            CancellationToken cancellationToken)
        {
            var kmsKeyId = GetKmsKeyId(kmsProvider, alternateKeyNames, masterKey);

            byte[] wrappedKeyBytes;
            try
            {
                using (var context = _cryptClient.StartCreateDataKeyContext(kmsKeyId))
                {
                    wrappedKeyBytes = await ProcessStatesAsync(context, _keyVaultNamespace.DatabaseNamespace.DatabaseName, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                throw new MongoEncryptionException($"Exception in encryption library: {ex.Message}.", ex);
            }

            var wrappedKeyDocument = new RawBsonDocument(wrappedKeyBytes);
            await _keyVaultCollection.Value.InsertOneAsync(wrappedKeyDocument, cancellationToken: cancellationToken).ConfigureAwait(false);
            var keyBytes = wrappedKeyDocument["_id"].AsBsonBinaryData.Bytes;
            return GuidConverter.FromBytes(keyBytes, GuidRepresentation.Standard);
        }

        public BsonValue DecryptField(BsonBinaryData encryptedValue, CancellationToken cancellationToken)
        {
            var wrappedValueBytes = GetWrappedValueBytes(encryptedValue);

            try
            {
                using (var context = _cryptClient.StartExplicitDecryptionContext(wrappedValueBytes))
                {
                    var wrappedBytes = ProcessStates(context, databaseName: null, cancellationToken);
                    return UnwrapDecryptedValue(wrappedBytes);
                }
            }
            catch (Exception ex)
            {
                throw new MongoEncryptionException($"Exception in encryption library: {ex.Message}.", ex);
            }
        }

        public async Task<BsonValue> DecryptFieldAsync(BsonBinaryData wrappedBinaryValue, CancellationToken cancellationToken)
        {
            var wrappedValueBytes = GetWrappedValueBytes(wrappedBinaryValue);

            try
            {
                using (var context = _cryptClient.StartExplicitDecryptionContext(wrappedValueBytes))
                {
                    var wrappedBytes = await ProcessStatesAsync(context, databaseName: null, cancellationToken).ConfigureAwait(false);
                    return UnwrapDecryptedValue(wrappedBytes);
                }
            }
            catch (Exception ex)
            {
                throw new MongoEncryptionException($"Exception in encryption library: {ex.Message}.", ex);
            }
        }

        public BsonBinaryData EncryptField(
            BsonValue value,
            Guid? keyId,
            string alternateKeyName,
            EncryptionAlgorithm encryptionAlgorithm,
            CancellationToken cancellationToken)
        {
            try
            {
                var wrappedValueBytes = GetWrappedValueBytes(value);

                CryptContext context;
                if (keyId.HasValue && alternateKeyName != null)
                {
                    throw new ArgumentException("keyId and alternateKeyName cannot both be provided.");
                }
                else if (keyId.HasValue)
                {
                    var keyBytes = GuidConverter.ToBytes(keyId.Value, GuidRepresentation.Standard);
                    context = _cryptClient.StartExplicitEncryptionContextWithKeyId(keyBytes, encryptionAlgorithm, wrappedValueBytes);
                }
                else if (alternateKeyName != null)
                {
                    var wrappedAlternateKeyNameBytes = GetWrappedAlternateKeyNameBytes(alternateKeyName);
                    context = _cryptClient.StartExplicitEncryptionContextWithKeyAltName(wrappedAlternateKeyNameBytes, encryptionAlgorithm, wrappedValueBytes);
                }
                else
                {
                    throw new ArgumentException("Either keyId or alternateKeyName must be provided.");
                }

                using (context)
                {
                    var wrappedBytes = ProcessStates(context, databaseName: null, cancellationToken);
                    return UnwrapEncryptedValue(wrappedBytes);
                }
            }
            catch (Exception ex)
            {
                throw new MongoEncryptionException($"Exception in encryption library: {ex.Message}.", ex);
            }
        }

        public async Task<BsonBinaryData> EncryptFieldAsync(
            BsonValue value,
            Guid? keyId,
            string alternateKeyName,
            EncryptionAlgorithm encryptionAlgorithm,
            CancellationToken cancellationToken)
        {
            try
            {
                var wrappedValueBytes = GetWrappedValueBytes(value);

                CryptContext context;
                if (keyId.HasValue && alternateKeyName != null)
                {
                    throw new ArgumentException("keyId and alternateKeyName cannot both be provided.");
                }
                else if (keyId.HasValue)
                {
                    var bytes = GuidConverter.ToBytes(keyId.Value, GuidRepresentation.Standard);
                    context = _cryptClient.StartExplicitEncryptionContextWithKeyId(bytes, encryptionAlgorithm, wrappedValueBytes);
                }
                else if (alternateKeyName != null)
                {
                    var wrappedAlternateKeyNameBytes = GetWrappedAlternateKeyNameBytes(alternateKeyName);
                    context = _cryptClient.StartExplicitEncryptionContextWithKeyAltName(wrappedAlternateKeyNameBytes, encryptionAlgorithm, wrappedValueBytes);
                }
                else
                {
                    throw new ArgumentException("Either keyId or alternateKeyName must be provided.");
                }

                using (context)
                {
                    var wrappedBytes = await ProcessStatesAsync(context, databaseName: null, cancellationToken).ConfigureAwait(false);
                    return UnwrapEncryptedValue(wrappedBytes);
                }
            }
            catch (Exception ex)
            {
                throw new MongoEncryptionException($"Exception in encryption library: {ex.Message}.", ex);
            }
        }

        // private methods
        private IKmsKeyId GetKmsKeyId(string kmsProvider, IReadOnlyList<string> alternateKeyNames, BsonDocument masterKey)
        {
            IEnumerable<byte[]> wrappedAlternateKeyNamesBytes = null;
            if (alternateKeyNames != null)
            {
                wrappedAlternateKeyNamesBytes = alternateKeyNames.Select(GetWrappedAlternateKeyNameBytes);
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
                    throw new ArgumentException($"Invalid kmsProvider {kmsProvider}.");
            }
        }

        private byte[] GetWrappedAlternateKeyNameBytes(string value)
        {
            return
               !string.IsNullOrWhiteSpace(value)
                   ? new BsonDocument("keyAltName", value).ToBson()
                   : null;
        }

        private byte[] GetWrappedValueBytes(BsonValue value)
        {
            var wrappedValue = new BsonDocument("v", value);
            var writerSettings = BsonBinaryWriterSettings.Defaults.Clone();
            writerSettings.GuidRepresentation = GuidRepresentation.Unspecified;
            return wrappedValue.ToBson(serializer: BsonValueSerializer.Instance, writerSettings: writerSettings);
        }

        private BsonValue UnwrapDecryptedValue(byte[] wrappedBytes)
        {
            var wrappedDocument = new RawBsonDocument(wrappedBytes);
            return wrappedDocument["v"];
        }

        private BsonBinaryData UnwrapEncryptedValue(byte[] encryptedWrappedBytes)
        {
            var wrappedDocument = new RawBsonDocument(encryptedWrappedBytes);
            //var encryptedBytes = wrappedDocument["v"].AsByteArray;
            //return new BsonBinaryData(encryptedBytes, BsonBinarySubType.Encrypted);
            return wrappedDocument["v"].AsBsonBinaryData;
        }
    }
}
