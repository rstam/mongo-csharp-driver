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
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Crypt;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.LibMongoCrypt
{
    /// <summary>
    /// Represents a encryption source.
    /// </summary>
    public interface IEncryptionSource //: IDisposable
    {
#pragma warning disable 3003
        /// <summary>
        /// Gets the singleton instance of <see cref="CryptClient"/>.
        /// </summary>
        /// <value>
        /// The singleton instance of <see cref="CryptClient"/>.
        /// </value>
        CryptClient CryptClient { get; }
#pragma warning restore
        
        /// <summary>
        /// Gets the mongocryptd client.
        /// </summary>
        /// <value>
        /// The mongo client for mongocryptd.
        /// </value>
        IMongoClient MongoCryptDClient { get; }
    }

    internal class EncryptionSource : IEncryptionSource
    {
        private readonly AutoEncryptionOptions _autoEncryptionOptions;
        private readonly Lazy<CryptClient> _cryptClient;
        private readonly IMongoClient _mongoCryptDClient;

        private EncryptionSource(MongoClientSettings mongoClientSettings)
        {
            Ensure.IsNotNull(mongoClientSettings, nameof(mongoClientSettings));
            _autoEncryptionOptions = Ensure.IsNotNull(mongoClientSettings.AutoEncryptionOptions, nameof(mongoClientSettings.AutoEncryptionOptions));
            _mongoCryptDClient = CreateMongoCryptDClient(_autoEncryptionOptions.ExtraOptions);
            _cryptClient = new Lazy<CryptClient>(() => CreateCryptClient(CreateCryptOptions()));
        }

        public IMongoClient MongoCryptDClient => _mongoCryptDClient;

#pragma warning disable 3003
        public CryptClient CryptClient => _cryptClient.Value;
#pragma warning restore

        public static IEncryptionSource CreateEncryptionSourceIfNecessary(MongoClientSettings mongoClientSettings)
        {
            if (mongoClientSettings.AutoEncryptionOptions != null)
            {
                return new EncryptionSource(mongoClientSettings);
            }
            else
            {
                return null;
            }
        }

        // private methods
        private CryptClient CreateCryptClient(CryptOptions options)
        {
            return CryptClientFactory.Create(options);
        }

        private CryptOptions CreateCryptOptions()
        {
            IKmsCredentials kmsCredentials = null;
            var kmsProviders = _autoEncryptionOptions.KmsProviders;
            if (kmsProviders != null)
            {
                if (kmsProviders.TryGetValue("aws", out var awsProvider))
                {
                    if (awsProvider.TryGetValue("accessKeyId", out var accessKeyId) &&
                        awsProvider.TryGetValue("secretAccessKey", out var secretAccessKey))
                    {
                        kmsCredentials = new AwsKmsCredentials((string)secretAccessKey, (string)accessKeyId);
                    }
                }
                if (kmsProviders.TryGetValue("local", out var localProvider))
                {
                    if (localProvider.TryGetValue("key", out var keyObject) && keyObject is byte[] key)
                    {
                        kmsCredentials = new LocalKmsCredentials(key);
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

            return new CryptOptions(kmsCredentials, schemaBytes);
        }

        private IMongoClient CreateMongoCryptDClient(IReadOnlyDictionary<string, object> extraOptions)
        {
            if (extraOptions == null)
            {
                extraOptions = new Dictionary<string, object>();
            }

            if (!extraOptions.TryGetValue("mongocryptdURI", out var connectionString))
            {
                connectionString = "mongodb://localhost:27020";
            }
            // todo:
            // mongocryptdBypassSpawn
            // mongocryptdSpawnPath
            // mongocryptdSpawnArgs
            return new MongoClient(connectionString.ToString());
        }
    }
}
