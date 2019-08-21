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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.TestHelpers.JsonDrivenTests;
using MongoDB.Bson.TestHelpers.XunitExtensions;
using MongoDB.Crypt;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Operations;
using MongoDB.Driver.Core.TestHelpers.XunitExtensions;
using MongoDB.Driver.TestHelpers;
using Xunit;

namespace MongoDB.Driver.Tests.Specifications.client_encryption_prose_tests
{
    public class ClientEncryptionProseTests
    {
        #region static
        private static readonly CollectionNamespace __collCollectionNamespace = CollectionNamespace.FromFullName("db.coll");
        private static readonly CollectionNamespace __keyVaultCollectionNamespace = CollectionNamespace.FromFullName("admin.datakeys");
        #endregion

        private const string SchemaMap = @"{
            ""db.coll"": {
            ""bsonType"": ""object"",
            ""properties"": {
                ""encrypted_placeholder"": {
                    ""encrypt"": {
                        ""keyId"": ""/placeholder"",
                        ""bsonType"": ""string"",
                        ""algorithm"": ""AEAD_AES_256_CBC_HMAC_SHA_512-Random""
                        }
                    }
                }
            }
        }";

        private readonly ICluster _cluster;
        private readonly ICoreSessionHandle _session;

        public ClientEncryptionProseTests()
        {
            BsonDefaults.GuidRepresentation = GuidRepresentation.Standard;
            _cluster = CoreTestConfiguration.Cluster;
            _session = CoreTestConfiguration.StartSession(_cluster);
        }

        [SkippableTheory(Skip = "Not finished.")]
        [ParameterAttributeData]
        // todo: this test doesn't pass since the logic https://github.com/mongodb/specifications/blob/master/source/client-side-encryption/client-side-encryption.rst#size-limits-and-wire-protocol-considerations
        // has not fully implemented yet.Currently `Type1CommandMessageSection` doesn't consider rules from here:
        // https://github.com/mongodb/specifications/blob/master/source/client-side-encryption/client-side-encryption.rst#size-limits-and-wire-protocol-considerations
        public void BsonSizeLimitAndBatchSizeSplittingTest(
            [Values(false, true)] bool async)
        {
            RequireServer.Check().Supports(Feature.ClientSideEncryption);

            using (var client = ConfigureClient())
            using (var clientEncrypted = ConfigureClientEncrypted(out _, kmsProvider: "local"))
            {
                var collLimitSchema = JsonTestDataFactory.Instance.Documents["limits.limits-schema.json"];
                CreateCollection(client, __collCollectionNamespace, new BsonDocument("$jsonSchema", collLimitSchema));
                var datakeysLimitsKey = JsonTestDataFactory.Instance.Documents["limits.limits-key.json"];
                var keyVaultCollection = GetCollection(client, __keyVaultCollectionNamespace);
                Insert(keyVaultCollection, async, datakeysLimitsKey);

                var coll = GetCollection(clientEncrypted, __collCollectionNamespace);

                var exception = Record.Exception(
                    () => Insert(
                        coll,
                        async,
                        new BsonDocument
                        {
                            { "_id", "no_encryption_under_2mib" },
                            { "unencrypted", new string('a', 2097152 - 1000) }
                        }));
                exception.Should().BeNull();

                exception = Record.Exception(
                    () => Insert(
                        coll,
                        async,
                        new BsonDocument
                        {
                            { "_id", "no_encryption_over_2mib" },
                            { "unencrypted", new string('a', 2097152) }
                        }));
                exception.Should().NotBeNull();

                var limitsDoc = JsonTestDataFactory.Instance.Documents["limits.limits-doc.json"];
                limitsDoc.AddRange(
                    new BsonDocument
                    {
                        {"_id", "encryption_exceeds_2mib"},
                        {"unencrypted", new string('a', 2097152 - 2000)}
                    });
                exception = Record.Exception(
                    () => Insert(
                        coll,
                        async,
                        limitsDoc));
                exception.Should().BeNull();

                exception = Record.Exception(
                    () => Insert(
                        coll,
                        async,
                        new BsonDocument
                        {
                            { "_id", "no_encryption_under_2mib_1" },
                            { "unencrypted", new string('a', 2097152 - 1000) }
                        },
                        new BsonDocument
                        {
                            { "_id", "no_encryption_under_2mib_2" },
                            { "unencrypted", new string('a', 2097152 - 1000) }
                        }));
                exception.Should().BeNull();
                //todo: command monitoring.?

                var limitsDoc1 = JsonTestDataFactory.Instance.Documents["limits.limits-doc.json"];
                limitsDoc1.AddRange(
                    new BsonDocument
                    {
                        { "_id", "encryption_exceeds_2mib_1" },
                        { "unencrypted", new string('a', 2097152 - 2000) }
                    });
                var limitsDoc2 = JsonTestDataFactory.Instance.Documents["limits.limits-doc.json"];
                limitsDoc1.AddRange(
                    new BsonDocument
                    {
                        { "_id", "encryption_exceeds_2mib_2" },
                        { "unencrypted", new string('a', 2097152 - 2000) }
                    });
                exception = Record.Exception(
                    () => Insert(
                        coll,
                        async,
                        limitsDoc1,
                        limitsDoc2));
                exception.Should().BeNull();
                //todo: command monitoring.?
            }
        }

        [SkippableTheory()]
        [ParameterAttributeData]
        public void CorpusTest(
            [Values(false, true)] bool useLocalSchema,
            [Values(false)] bool async)
        {
            RequireServer.Check().Supports(Feature.ClientSideEncryption);

            EncryptionAlgorithm ParseAlgorithm(string algorithm)
            {
                switch (algorithm)
                {
                    case "rand":
                        return EncryptionAlgorithm.AEAD_AES_256_CBC_HMAC_SHA_512_Random;
                    case "det":
                        return EncryptionAlgorithm.AEAD_AES_256_CBC_HMAC_SHA_512_Deterministic;
                    default:
                        throw new ArgumentException($"Unsupported algorithm {algorithm}.");
                }
            }

            EncryptOptions CreateEncryptionOptions(string algorithm, string identifier, string kms)
            {
                byte[] keyId = null;
                string altName = null;
                if (identifier == "id")
                {
                    switch (kms)
                    {
                        case "local":
                            keyId = Convert.FromBase64String("LOCALAAAAAAAAAAAAAAAAA==");
                            break;
                        case "aws":
                            keyId = Convert.FromBase64String("AWSAAAAAAAAAAAAAAAAAAA==");
                            break;
                        default:
                            throw new ArgumentException($"Unsupported kms type {kms}.");
                    }
                }
                else if (identifier == "altname")
                {
                    altName = kms;
                }
                else
                {
                    throw new ArgumentException($"Unsupported identifier {identifier}.", nameof(identifier));
                }

                return new EncryptOptions(ParseAlgorithm(algorithm).ToString(), altName, keyId);
            }

            var corpusSchema = JsonTestDataFactory.Instance.Documents["corpus.corpus-schema.json"];
            var schemaMap = useLocalSchema ? new BsonDocument("db.coll", corpusSchema) : null;
            using (var client = ConfigureClient())
            using (var clientEncrypted = ConfigureClientEncrypted(out var clientEncryption, schemaMap))
            {
                CreateCollection(client, __collCollectionNamespace, new BsonDocument("$jsonSchema", corpusSchema));

                var corpusKeyLocal = JsonTestDataFactory.Instance.Documents["corpus.corpus-key-local.json"];
                var corpusKeyAws = JsonTestDataFactory.Instance.Documents["corpus.corpus-key-aws.json"];
                var keyVaultCollection = GetCollection(client, __keyVaultCollectionNamespace);
                Insert(keyVaultCollection, async, corpusKeyLocal, corpusKeyAws);

                var corpus = JsonTestDataFactory.Instance.Documents["corpus.corpus.json"];
                var corpusCopied = new BsonDocument
                {
                    corpus.GetElement("_id"),
                    corpus.GetElement("altname_aws"),
                    corpus.GetElement("altname_local")
                };
                foreach (var corpusElement in corpus.Elements.Where(c => c.Value.IsBsonDocument))
                {
                    var corpusValue = corpusElement.Value.DeepClone();
                    var kms = corpusValue["kms"].AsString;
                    var abbreviatedAlgorithmName = corpusValue["algo"].AsString;
                    var identifier = corpusValue["identifier"].AsString;
                    var allowed = corpusValue["allowed"].ToBoolean();
                    var value = corpusValue["value"];
                    var method = corpusValue["method"].AsString;
                    switch (method)
                    {
                        case "auto":
                            corpusCopied.Add(corpusElement);
                            continue;
                        case "explicit":
                            {
                                var encryptionOptions = CreateEncryptionOptions(abbreviatedAlgorithmName, identifier, kms);
                                BsonBinaryData encrypted = null;
                                var exception = Record.Exception(() =>
                                {
                                    encrypted = ExplicitEncrypt(
                                        clientEncryption,
                                        encryptionOptions,
                                        value, async);
                                });
                                if (allowed)
                                {
                                    exception.Should().BeNull();
                                    encrypted.Should().NotBeNull();
                                    corpusValue["value"] = encrypted;
                                }
                                else
                                {
                                    exception.Should().NotBeNull();
                                }
                                corpusCopied.Add(new BsonElement(corpusElement.Name, corpusValue));
                            }
                            break;
                        default:
                            throw new ArgumentException($"Unsupported method name {method}.", nameof(method));
                    }
                }

                var coll = GetCollection(clientEncrypted, __collCollectionNamespace);
                Insert(coll, async, corpusCopied);

                var corpusDecrypted = Find(coll, new BsonDocument(), async).Single();
                corpusDecrypted.Should().Be(corpus);

                var corpusEncryptedExpected = JsonTestDataFactory.Instance.Documents["corpus.corpus-encrypted.json"];
                coll = GetCollection(client, __collCollectionNamespace);
                var corpusEncryptedActual = Find(coll, new BsonDocument(), async).Single();
                foreach (var expectedElement in corpusEncryptedExpected.Elements.Where(c => c.Value.IsBsonDocument))
                {
                    var expectedElementValue = expectedElement.Value;
                    var expectedAlgorithm = ParseAlgorithm(expectedElementValue["algo"].AsString);
                    var expectedAllowed = expectedElementValue["allowed"].ToBoolean();
                    var expectedValue = expectedElementValue["value"];
                    var actualValue = corpusEncryptedActual.GetValue(expectedElement.Name)["value"];

                    switch (expectedAlgorithm)
                    {
                        case EncryptionAlgorithm.AEAD_AES_256_CBC_HMAC_SHA_512_Deterministic:
                            actualValue.Should().Be(expectedValue);
                            break;
                        case EncryptionAlgorithm.AEAD_AES_256_CBC_HMAC_SHA_512_Random:
                            if (expectedAllowed)
                            {
                                actualValue.Should().NotBe(expectedValue);
                            }
                            break;
                        default:
                            throw new ArgumentException($"Unsupported expected algorithm {expectedAllowed}.", nameof(expectedAlgorithm));
                    }

                    if (expectedAllowed)
                    {
                        var actualDecryptedValue = ExplicitDecrypt(clientEncryption, actualValue.AsBsonBinaryData, async);
                        var expectedDecryptedValue = ExplicitDecrypt(clientEncryption, expectedValue.AsBsonBinaryData, async);
                        actualDecryptedValue.Should().Be(expectedDecryptedValue);
                    }
                    else
                    {
                        actualValue.Should().Be(expectedValue);
                    }
                }
            }
        }

        [SkippableTheory]
        [ParameterAttributeData]
        public void CreateDataKeyAndDoubleEncryptionTest(
            [Values("local", "aws")] string kmsProvider,
            [Values(false, true)] bool async)
        {
            RequireServer.Check().Supports(Feature.ClientSideEncryption);

            using (var client = ConfigureClient())
            using (var clientEncrypted = ConfigureClientEncrypted(out var clientEncryption, BsonDocument.Parse(SchemaMap)))
            {
                var dataKeyOptions = CreateDataKeyOptions(kmsProvider);

                BsonValue dataKey;
                if (async)
                {
                    dataKey = clientEncryption
                        .CreateDataKeyAsync(kmsProvider, dataKeyOptions, CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                }
                else
                {
                    dataKey = clientEncryption.CreateDataKey(kmsProvider, dataKeyOptions, CancellationToken.None);
                }
                dataKey.AsBsonBinaryData.SubType.Should().Be(BsonBinarySubType.UuidStandard);

                var keyVaultCollection = GetCollection(client, __keyVaultCollectionNamespace);
                var keyVaultDocument =
                    Find(
                        keyVaultCollection,
                        new BsonDocument("_id", dataKey),
                        async)
                    .Single();
                keyVaultDocument["masterKey"]["provider"].Should().Be(BsonValue.Create(kmsProvider));

                var encryptOptions = new EncryptOptions(
                    EncryptionAlgorithm.AEAD_AES_256_CBC_HMAC_SHA_512_Deterministic.ToString(),
                    keyId: dataKey.AsBsonBinaryData.Bytes);

                var encryptedValue = ExplicitEncrypt(
                    clientEncryption,
                    encryptOptions,
                    $"hello {kmsProvider}",
                    async);
                encryptedValue.SubType.Should().Be(BsonBinarySubType.Encryption);

                var coll = GetCollection(clientEncrypted, __collCollectionNamespace);
                Insert(
                    coll,
                    async,
                    new BsonDocument
                    {
                        {"_id", kmsProvider},
                        {"value", encryptedValue}
                    });

                var findResult = Find(coll, new BsonDocument("_id", kmsProvider), async).Single();
                findResult["value"].ToString().Should().Be($"hello {kmsProvider}");

                encryptOptions = new EncryptOptions(
                    EncryptionAlgorithm.AEAD_AES_256_CBC_HMAC_SHA_512_Deterministic.ToString(),
                    keyAltName: $"{kmsProvider}_altname");
                var encryptedValueWithKeyAltName = ExplicitEncrypt(
                    clientEncryption,
                    encryptOptions,
                    $"hello {kmsProvider}",
                    async);
                encryptedValueWithKeyAltName.SubType.Should().Be(BsonBinarySubType.Encryption);
                encryptedValueWithKeyAltName.Should().Be(encryptedValue);

                if (kmsProvider == "local") // the test description expects this assert only once for a local kms provider
                {
                    coll = GetCollection(clientEncrypted, __collCollectionNamespace);
                    var exception = Record.Exception(() => coll.InsertOne(new BsonDocument("encrypted_placeholder", encryptedValue)));
                    exception.Should().BeOfType<MongoClientException>();
                }
            }
        }

        [SkippableTheory]
        [ParameterAttributeData]
        public void ExternalKeyVaultTest(
            [Values(false, true)] bool withExternalKeyVault,
            [Values(false, true)] bool async)
        {
            RequireServer.Check().Supports(Feature.ClientSideEncryption);

            var clientEncryptedSchema = new BsonDocument("db.coll", JsonTestDataFactory.Instance.Documents["external.external-schema.json"]);
            using (var client = ConfigureClient())
            using (var clientEncrypted = ConfigureClientEncrypted(out var clientEncryption, clientEncryptedSchema, withExternalKeyVault))
            {
                var datakeys = GetCollection(client, __keyVaultCollectionNamespace);
                var externalKey = JsonTestDataFactory.Instance.Documents["external.external-key.json"];
                Insert(datakeys, async, externalKey);

                var coll = GetCollection(clientEncrypted, __collCollectionNamespace);
                var exception = Record.Exception(() => Insert(coll, async, new BsonDocument("encrypted", "test")));
                if (withExternalKeyVault)
                {
                    exception.InnerException.Should().BeOfType<MongoAuthenticationException>();
                }
                else
                {
                    exception.Should().BeNull();
                }

                var encryptionOptions = new EncryptOptions(
                    algorithm: EncryptionAlgorithm.AEAD_AES_256_CBC_HMAC_SHA_512_Deterministic.ToString(),
                    keyId: Convert.FromBase64String("LOCALAAAAAAAAAAAAAAAAA=="));
                exception = Record.Exception(() => ExplicitEncrypt(clientEncryption, encryptionOptions, "test", async));
                if (withExternalKeyVault)
                {
                    exception.InnerException.Should().BeOfType<MongoAuthenticationException>();
                }
                else
                {
                    exception.Should().BeNull();
                }
            }
        }

        [SkippableTheory]
        [ParameterAttributeData]
        public void ViewAreProhibitedTest([Values(false, true)] bool async)
        {
            RequireServer.Check().Supports(Feature.ClientSideEncryption);
            CollectionNamespace viewName = CollectionNamespace.FromFullName("db.view");

            using (var client = ConfigureClient(false))
            using (var clientEncrypted = ConfigureClientEncrypted(out _, kmsProvider: "local"))
            {
                DropView(viewName);
                client
                    .GetDatabase(viewName.DatabaseNamespace.DatabaseName)
                    .CreateView(
                        viewName.CollectionName,
                        __collCollectionNamespace.CollectionName,
                        new EmptyPipelineDefinition<BsonDocument>());

                var view = GetCollection(clientEncrypted, viewName);
                var exception = Record.Exception(() => Insert(
                    view,
                    async,
                    documents: new BsonDocument("test", 1)));
                exception.Message.Should().Be("cannot auto encrypt a view");
            }
        }

        // private methods
        private DisposableMongoClient ConfigureClient(bool clearCollections = true)
        {
            var client = new DisposableMongoClient(GetMongoClient());
            if (clearCollections)
            {
                var clientAdminDatabase =
                    client.GetDatabase(__keyVaultCollectionNamespace.DatabaseNamespace.DatabaseName);
                clientAdminDatabase.DropCollection(__keyVaultCollectionNamespace.CollectionName);
                var clientDbDatabase = client.GetDatabase(__collCollectionNamespace.DatabaseNamespace.DatabaseName);
                clientDbDatabase.DropCollection(__collCollectionNamespace.CollectionName);
            }
            return client;
        }

        private DisposableMongoClient ConfigureClientEncrypted(out ClientEncryption clientEncryption, BsonDocument schemaMap = null, bool withExternalKeyVault = false, string kmsProvider = null)
        {
            var kmsProviders = GetKmsProviders();
            var clientEncrypted = new DisposableMongoClient(
                GetMongoClient(
                    __keyVaultCollectionNamespace,
                    schemaMap != null ? schemaMap : null,
                    kmsProvider == null
                        ? kmsProviders
                        : kmsProviders
                            .Where(c => c.Key == kmsProvider)
                            .ToDictionary(key => key.Key, value => value.Value),
                    withExternalKeyVault: withExternalKeyVault));

            var clientEncryptionOptions = new ClientEncryptionOptions(
                keyVaultClient: clientEncrypted.Wrapped.Settings.AutoEncryptionOptions.KeyVaultClient ?? clientEncrypted,
                __keyVaultCollectionNamespace,
                kmsProviders);
            clientEncryption = clientEncrypted.GetClientEncryption(clientEncryptionOptions);
            return clientEncrypted;
        }

        private void CreateCollection(IMongoClient client, CollectionNamespace collectionNamespace, BsonDocument validatorSchema)
        {
            client
                .GetDatabase(collectionNamespace.DatabaseNamespace.DatabaseName)
                .CreateCollection(
                    collectionNamespace.CollectionName,
                    new CreateCollectionOptions<BsonDocument>()
                    {
                        Validator = new BsonDocumentFilterDefinition<BsonDocument>(validatorSchema)
                    });
        }

        private DataKeyOptions CreateDataKeyOptions(string kmsProvider)
        {
            switch (kmsProvider)
            {
                case "local":
                    return new DataKeyOptions(keyAltNames: new Optional<IReadOnlyList<string>>(new[] { $"{kmsProvider}_altname" }));
                case "aws":
                    var masterKey = new BsonDocument
                    {
                        { "region", "us-east-1" },
                        { "key", "arn:aws:kms:us-east-1:579766882180:key/89fcc2c4-08b0-4bd9-9f25-e30687b580d0" }
                    };
                    return new DataKeyOptions(
                        keyAltNames: new Optional<IReadOnlyList<string>>(new[] { $"{kmsProvider}_altname" }),
                        masterKey: new Optional<BsonDocument>(masterKey));
                default:
                    throw new ArgumentException($"Incorrect kms provider {kmsProvider}", nameof(kmsProvider));
            }
        }

        private void DropView(CollectionNamespace viewNamespace)
        {
            var operation = new DropCollectionOperation(viewNamespace, CoreTestConfiguration.MessageEncoderSettings);
            using (var binding = new WritableServerBinding(_cluster, _session.Fork()))
            using (var bindingHandle = new ReadWriteBindingHandle(binding))
            {
                operation.Execute(bindingHandle, CancellationToken.None);
            }
        }

        private BsonBinaryData ExplicitDecrypt(
            ClientEncryption clientEncryption,
            BsonBinaryData value,
            bool async)
        {
            BsonBinaryData encryptedValue;
            if (async)
            {
                encryptedValue = clientEncryption
                    .DecryptAsync(
                        value,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
            }
            else
            {
                encryptedValue = clientEncryption.Decrypt(
                    value,
                    CancellationToken.None);
            }

            return encryptedValue;
        }

        private BsonBinaryData ExplicitEncrypt(
            ClientEncryption clientEncryption,
            EncryptOptions encryptOptions,
            BsonValue value,
            bool async)
        {
            BsonBinaryData encryptedValue;
            if (async)
            {
                encryptedValue = clientEncryption
                    .EncryptAsync(
                        value,
                        encryptOptions,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
            }
            else
            {
                encryptedValue = clientEncryption.Encrypt(
                    value,
                    encryptOptions,
                    CancellationToken.None);
            }

            return encryptedValue;
        }

        private IAsyncCursor<BsonDocument> Find(
            IMongoCollection<BsonDocument> collection,
            BsonDocument filter,
            bool async)
        {
            if (async)
            {
                return collection
                    .FindAsync(new BsonDocumentFilterDefinition<BsonDocument>(filter))
                    .GetAwaiter()
                    .GetResult();
            }
            else
            {
                return collection
                    .FindSync(new BsonDocumentFilterDefinition<BsonDocument>(filter));
            }
        }

        private IMongoCollection<BsonDocument> GetCollection(IMongoClient client, CollectionNamespace collectionNamespace)
        {
            return client
                .GetDatabase(collectionNamespace.DatabaseNamespace.DatabaseName)
                .GetCollection<BsonDocument>(collectionNamespace.CollectionName);
        }

        private IReadOnlyDictionary<string, IReadOnlyDictionary<string, object>> GetKmsProviders()
        {
            var kmsProviders = new Dictionary<string, IReadOnlyDictionary<string, object>>();

            var kmsOptions = new Dictionary<string, object>();
            var awsRegion = Environment.GetEnvironmentVariable("FLE_AWS_REGION") ?? "us-east-1";
            var awsAccessKey = Environment.GetEnvironmentVariable("FLE_AWS_ACCESS_KEY_ID") ?? throw new Exception("The AWS_ACCESS_KEY_ID system variable should be configured on the machine.");
            var awsSecretAccessKey = Environment.GetEnvironmentVariable("FLE_AWS_SECRET_ACCESS_KEY") ?? throw new Exception("The AWS_SECRET_ACCESS_KEY system variable should be configured on the machine.");
            kmsOptions.Add("region", awsRegion);
            kmsOptions.Add("accessKeyId", awsAccessKey);
            kmsOptions.Add("secretAccessKey", awsSecretAccessKey);
            kmsProviders.Add("aws", kmsOptions);

            var localOptions = new Dictionary<string, object>();
            var localMasterKey = Environment.GetEnvironmentVariable("FLE_LOCAL_MASTERKEY");
            localOptions.Add("key", new BsonBinaryData(Convert.FromBase64String(localMasterKey), BsonBinarySubType.Binary).Bytes);
            kmsProviders.Add("local", localOptions);

            return new ReadOnlyDictionary<string, IReadOnlyDictionary<string, object>>(kmsProviders);
        }

        private IMongoClient GetMongoClient(
            CollectionNamespace keyVaultNamespace = null,
            BsonDocument schemaMapDocument = null,
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, object>> kmsProviders = null,
            bool withExternalKeyVault = false)
        {
            if (keyVaultNamespace != null || schemaMapDocument != null)
            {
                Dictionary<string, BsonDocument> schemaMap = null;
                if (schemaMapDocument != null)
                {
                    var element = schemaMapDocument.Single();
                    schemaMap = new Dictionary<string, BsonDocument>
                    {
                        { element.Name, element.Value.AsBsonDocument }
                    };
                }

                if (kmsProviders == null)
                {
                    kmsProviders = new ReadOnlyDictionary<string, IReadOnlyDictionary<string, object>>(new Dictionary<string, IReadOnlyDictionary<string, object>>());
                }

                var autoEncryptionOptions = new AutoEncryptionOptions(
                    keyVaultNamespace,
                    kmsProviders,
                    schemaMap: new Optional<IReadOnlyDictionary<string, BsonDocument>>(schemaMap));
                if (withExternalKeyVault)
                {
                    autoEncryptionOptions = autoEncryptionOptions.With(
                        keyVaultClient: new Optional<IMongoClient>(
                            new MongoClient(
                                new MongoClientSettings()
                                {
                                    Credential = MongoCredential.FromComponents(null, null, "fake-user", "fake-pwd")
                                })));
                }

                return new MongoClient(
                    new MongoClientSettings
                    {
                        AutoEncryptionOptions = autoEncryptionOptions,
                        GuidRepresentation = GuidRepresentation.Standard
                    });
            }
            else
            {
                return new MongoClient(
                    new MongoClientSettings
                    {
                        GuidRepresentation = GuidRepresentation.Standard
                    });
            }
        }

        private void Insert(
            IMongoCollection<BsonDocument> collection,
            bool async,
            params BsonDocument[] documents)
        {
            if (async)
            {
                collection
                    .InsertManyAsync(documents)
                    .GetAwaiter()
                    .GetResult();
            }
            else
            {
                collection.InsertMany(documents);
            }
        }

        public class JsonTestDataFactory : JsonDrivenTestCaseFactory
        {
            #region static
            private static JsonTestDataFactory __instance;
            public static JsonTestDataFactory Instance => __instance ?? (__instance = new JsonTestDataFactory());
            #endregion

            public JsonTestDataFactory()
            {
                Documents = new ReadOnlyDictionary<string, BsonDocument>(ReadDocuments());
            }

            protected override string[] PathPrefixes => new[]
            {
                "MongoDB.Driver.Tests.Specifications.client_side_encryption.testsdata.corpus.",
                "MongoDB.Driver.Tests.Specifications.client_side_encryption.testsdata.external.",
                "MongoDB.Driver.Tests.Specifications.client_side_encryption.testsdata.limits."
            };

            public IReadOnlyDictionary<string, BsonDocument> Documents { get; }

            private IDictionary<string, BsonDocument> ReadDocuments()
            {
                var documents = ReadJsonDocuments();
                return new Dictionary<string, BsonDocument>(
                    documents.ToDictionary(
                        key =>
                        {
                            var path = key["_path"].ToString();
                            var testTitle = "MongoDB.Driver.Tests.Specifications.client_side_encryption.testsdata";
                            var startIndex = path.IndexOf(testTitle, StringComparison.Ordinal);
                            if (startIndex != -1)
                            {
                                return path.Substring(startIndex + testTitle.Length + 1);
                            }
                            else
                            {
                                throw new ArgumentException($"Unexpected test file: {path}.");
                            }
                        },
                        value =>
                        {
                            value.Remove("_path");
                            return value;
                        }));
            }
        }
    }
}
