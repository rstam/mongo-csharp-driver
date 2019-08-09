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
using MongoDB.Bson.TestHelpers.XunitExtensions;
using MongoDB.Crypt;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.TestHelpers.XunitExtensions;
using MongoDB.Driver.TestHelpers;
using Xunit;

namespace MongoDB.Driver.Tests
{
    public class ClientEncryptionTests
    {
        private CollectionNamespace __keyVaultCollectionNamespace = CollectionNamespace.FromFullName("admin.datakeys");
        private CollectionNamespace __collCollectionNamespace = CollectionNamespace.FromFullName("db.coll");

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

        // todo: add async part
        [SkippableTheory]
        [ParameterAttributeData]
        public void CreateDataKeyAndDoubleEncryptionForLocalTest([Values(false)] bool async)
        {
            RequireServer.Check().Supports(Feature.ClientSideEncryption);

            ConfigureClients(out var client, out var clientEncrypted, out var clientEncryption);
            using (client)
            using (clientEncrypted)
            {
                var datakeyOptions = new DataKeyOptions(
                    keyAltNames: new Optional<IReadOnlyList<string>>(new[] {"local_altname"}));

                var localDataKey = clientEncryption.CreateDataKey("local", datakeyOptions, CancellationToken.None);
                localDataKey.Should().NotBeNull();
                localDataKey.AsBsonBinaryData.GuidRepresentation.Should().Be(GuidRepresentation.Standard);

                var keyVaultCollection = client
                    .GetDatabase(__keyVaultCollectionNamespace.DatabaseNamespace.DatabaseName)
                    .GetCollection<BsonDocument>(__keyVaultCollectionNamespace.CollectionName);
                var keyVaultDocument = keyVaultCollection
                    .FindSync(document => document["_id"] == localDataKey)
                    .Single();
                keyVaultDocument["masterKey"]["provider"].Should().Be(BsonValue.Create("local"));

                var encryptOptions = new EncryptOptions(
                    Alogrithm.AEAD_AES_256_CBC_HMAC_SHA_512_Deterministic.ToString(),
                    keyId: localDataKey.AsBsonBinaryData.Bytes);
                var localEncrypted = clientEncryption.Encrypt(
                    "hello local",
                    encryptOptions,
                    CancellationToken.None);

                var coll = clientEncrypted
                    .GetDatabase(__collCollectionNamespace.DatabaseNamespace.DatabaseName)
                    .GetCollection<BsonDocument>(__collCollectionNamespace.CollectionName);
                coll.InsertOne(
                    new BsonDocument
                    {
                        {"_id", "local"},
                        {"value", localEncrypted}
                    });

                //todo: doesn't work yet
                //var res = coll.FindSync(document => document["_id"] == "local" && document["value"] == "hello local").Single();
            }
        }

        // todo: combine these tests?
        // todo: async part
        [SkippableTheory]
        [ParameterAttributeData]
        public void CreateDataKeyAndDoubleEncryptionForAwsTest([Values(false)] bool async)
        {
            RequireServer.Check().Supports(Feature.ClientSideEncryption);

            ConfigureClients(out var client, out var clientEncrypted, out var clientEncryption);
            using (client)
            using (clientEncrypted)
            {
                var datakeyOptions = new DataKeyOptions(
                    masterKey: 
                    new Optional<BsonDocument>(
                        new BsonDocument
                        {
                            { "region", "us-east-1" },
                            { "key", "arn:aws:kms:us-east-1:579766882180:key/89fcc2c4-08b0-4bd9-9f25-e30687b580d0" }
                        }));

                var awsDataKey = clientEncryption.CreateDataKey("aws", datakeyOptions, CancellationToken.None);
                awsDataKey.Should().NotBeNull();
                awsDataKey.AsBsonBinaryData.GuidRepresentation.Should().Be(GuidRepresentation.Standard);

                var keyVaultCollection = client
                    .GetDatabase(__keyVaultCollectionNamespace.DatabaseNamespace.DatabaseName)
                    .GetCollection<BsonDocument>(__keyVaultCollectionNamespace.CollectionName);
                var keyVaultDocument = keyVaultCollection
                    .FindSync(document => document["_id"] == awsDataKey)
                    .Single();
                keyVaultDocument["masterKey"]["provider"].Should().Be(BsonValue.Create("aws"));

                var encryptOptions = new EncryptOptions(
                    Alogrithm.AEAD_AES_256_CBC_HMAC_SHA_512_Deterministic.ToString(),
                    keyId: awsDataKey.AsBsonBinaryData.Bytes);
                var awsEncrypted = clientEncryption.Encrypt(
                    "hello aws",
                    encryptOptions,
                    CancellationToken.None);

                var coll = clientEncrypted
                    .GetDatabase(__collCollectionNamespace.DatabaseNamespace.DatabaseName)
                    .GetCollection<BsonDocument>(__collCollectionNamespace.CollectionName);
                coll.InsertOne(
                    new BsonDocument
                    {
                        {"_id", "aws"},
                        {"value", awsEncrypted}
                    });

                //todo: doesn't work yet
                //var res = coll.FindSync(document => document["_id"] == "local" && document["value"] == "hello local").Single();
            }
        }

        // private methods
        private void ConfigureClients(out DisposableMongoClient client, out DisposableMongoClient clientEncrypted, out ClientEncryption clientEncryption)
        {
            client = new DisposableMongoClient(GetMongoClient());
            var clientAdminDatabase = client.GetDatabase(__keyVaultCollectionNamespace.DatabaseNamespace.DatabaseName); //todo: WriteConcern.WMajority?
            clientAdminDatabase.DropCollection(__keyVaultCollectionNamespace.CollectionName);
            var clientDbDatabase = client.GetDatabase(__collCollectionNamespace.DatabaseNamespace.DatabaseName);
            clientDbDatabase.DropCollection(__collCollectionNamespace.CollectionName);

            var kmsProviders = GetKmsProviders();
            clientEncrypted = new DisposableMongoClient(
                GetMongoClient(
                    __keyVaultCollectionNamespace,
                    BsonDocument.Parse(SchemaMap),
                    kmsProviders));

            var clientEncryptionOptions = new ClientEncryptionOptions(
                keyVaultClient: client,
                __keyVaultCollectionNamespace,
                kmsProviders);
            clientEncryption = clientEncrypted.GetClientEncryption(clientEncryptionOptions);
        }

        private IMongoClient GetMongoClient(
            CollectionNamespace keyVaultNamespace = null,
            BsonDocument schemaMapDocument = null,
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, object>> kmsProviders = null)
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
                return new MongoClient(
                    new MongoClientSettings
                    {
                        AutoEncryptionOptions = autoEncryptionOptions
                    });
            }
            else
            {
                return new MongoClient();
            }
        }

        private IReadOnlyDictionary<string, IReadOnlyDictionary<string, object>> GetKmsProviders()
        {
            var kmsProviders = new Dictionary<string, IReadOnlyDictionary<string, object>>();

            var kmsOptions = new Dictionary<string, object>();
            // todo: replace on right way of using environment variables
            // todo: add `FLE` prefixes
            var awsRegion = Environment.GetEnvironmentVariable("AWS_REGION", EnvironmentVariableTarget.Machine) ?? "us-east-1";
            var awsAccessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID", EnvironmentVariableTarget.Machine) ?? throw new Exception("The AWS_ACCESS_KEY_ID system variable should be configured on the machine.");
            var awsSecretAccessKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", EnvironmentVariableTarget.Machine) ?? throw new Exception("The AWS_SECRET_ACCESS_KEY system variable should be configured on the machine.");
            kmsOptions.Add("region", awsRegion);
            kmsOptions.Add("accessKeyId", awsAccessKey);
            kmsOptions.Add("secretAccessKey", awsSecretAccessKey);
            kmsProviders.Add("aws", kmsOptions);

            var localOptions = new Dictionary<string, object>();
            var localMasterKey = Environment.GetEnvironmentVariable("LOCAL_MASTERKEY", EnvironmentVariableTarget.Machine);
            localOptions.Add("key", new BsonBinaryData(Convert.FromBase64String(localMasterKey), BsonBinarySubType.Binary).Bytes);
            kmsProviders.Add("local", localOptions);

            return new ReadOnlyDictionary<string, IReadOnlyDictionary<string, object>>(kmsProviders);
        }
    }
}
