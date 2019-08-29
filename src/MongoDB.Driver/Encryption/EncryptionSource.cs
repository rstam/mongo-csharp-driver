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
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver.Core.Misc;
using MongoDB.Libmongocrypt;

namespace MongoDB.Driver
{
    internal interface IEncryptionClientsSource
    {
        IEncryptionClients EncryptionClients { get; }
    }

    internal interface IEncryptionClients
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

    internal class EncryptionClients : IEncryptionClients
    {
        private readonly AutoEncryptionOptions _autoEncryptionOptions;
        private readonly Lazy<CryptClient> _cryptClient;
        private readonly Lazy<IMongoClient> _mongoCryptDClient;

        private EncryptionClients(MongoClientSettings mongoClientSettings)
        {
            Ensure.IsNotNull(mongoClientSettings, nameof(mongoClientSettings));
            _autoEncryptionOptions = Ensure.IsNotNull(mongoClientSettings.AutoEncryptionOptions, nameof(mongoClientSettings.AutoEncryptionOptions));
            _cryptClient = new Lazy<CryptClient>(() => CreateCryptClient(CreateCryptOptions()));
            _mongoCryptDClient = new Lazy<IMongoClient>(() => CreateMongoCryptDClient(_autoEncryptionOptions.ExtraOptions));
        }

#pragma warning disable 3003
        public CryptClient CryptClient => _cryptClient.Value;
#pragma warning restore

        public IMongoClient MongoCryptDClient => _mongoCryptDClient.Value;

        public static IEncryptionClients CreateEncryptionClientsIfNecessary(MongoClientSettings mongoClientSettings)
        {
            if (mongoClientSettings.AutoEncryptionOptions != null)
            {
                return new EncryptionClients(mongoClientSettings);
            }
            else
            {
                return null;
            }
        }

        // private methods
        private string CreateMongoCryptDConnectionString(IReadOnlyDictionary<string, object> extraOptions)
        {
            if (extraOptions == null)
            {
                extraOptions = new Dictionary<string, object>();
            }

            if (!extraOptions.TryGetValue("mongocryptdURI", out var connectionString))
            {
                connectionString = "mongodb://localhost:27020";
            }

            return connectionString.ToString();
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

        private IMongoClient CreateMongoCryptDClient(IReadOnlyDictionary<string, object> extraOptions)
        {
            var connectionString = CreateMongoCryptDConnectionString(extraOptions);

            if (ShouldMongocryptdBeSpawned(out var path, out var args, extraOptions))
            {
                StartProcess(path, args);
            }

            return new MongoClient(connectionString);
        }

        private bool ShouldMongocryptdBeSpawned(out string path, out string args, IReadOnlyDictionary<string, object> extraOptions)
        {
            path = null;
            args = null;
            if (!extraOptions.TryGetValue("mongocryptdBypassSpawn", out var mongoCryptBypassSpawn) || (!bool.Parse(mongoCryptBypassSpawn.ToString())))
            {
                if (!extraOptions.TryGetValue("mongocryptdSpawnPath", out var objPath))
                {
                    path = Environment.GetEnvironmentVariable("MONGODB_BINARIES") ?? string.Empty;
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
                if (extraOptions.TryGetValue("mongocryptdSpawnArgs", out var mongocryptdSpawnArgs))
                {
                    string trimStartHyphens(string str) => str.TrimStart('-').TrimStart('-');
                    switch (mongocryptdSpawnArgs)
                    {
                        case string str:
                            var options = str.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                            if (!options.Any(o => o.Contains("idleShutdownTimeoutSecs")))
                            {
                                args += "--idleShutdownTimeoutSecs 60;";
                            }
                            args += str;
                            break;
                        case IDictionary dictionary:
                            foreach (var key in dictionary.Keys)
                            {
                                args += $"--{trimStartHyphens(key.ToString())} {dictionary[key]}".TrimEnd(';') + ";";
                            }
                            break;
                        case IEnumerable enumerable:
                            foreach (var item in enumerable)
                            {
                                args += $"--{trimStartHyphens(item.ToString())}".TrimEnd(';') + ";";
                            }
                            break;
                        default:
                            args = mongocryptdSpawnArgs.ToString();
                            break;
                    }
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
                    mongoCryptD.StartInfo.UseShellExecute = true;
                    mongoCryptD.StartInfo.FileName = path;
                    mongoCryptD.StartInfo.CreateNoWindow = true;
                    // todo: should it be?
                    mongoCryptD.StartInfo.UseShellExecute = false;
                    mongoCryptD.StartInfo.Arguments = args;

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
