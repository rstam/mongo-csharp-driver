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
        private readonly object _processSpawnLock = new object();

        private readonly AutoEncryptionOptions _autoEncryptionOptions;
        private readonly CryptClient _cryptClient;
        private readonly IMongoClient _mongoCryptDClient;

        private EncryptionSource(MongoClientSettings mongoClientSettings)
        {
            Ensure.IsNotNull(mongoClientSettings, nameof(mongoClientSettings));
            _autoEncryptionOptions = Ensure.IsNotNull(mongoClientSettings.AutoEncryptionOptions, nameof(mongoClientSettings.AutoEncryptionOptions));
            _mongoCryptDClient = CreateMongoCryptDClient(_autoEncryptionOptions.ExtraOptions);
            _cryptClient = CreateCryptClient(CreateCryptOptions());
        }

#pragma warning disable 3003
        public CryptClient CryptClient => _cryptClient;
#pragma warning restore

        public IMongoClient MongoCryptDClient => _mongoCryptDClient;

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
            if (extraOptions == null)
            {
                extraOptions = new Dictionary<string, object>();
            }

            if (!extraOptions.TryGetValue("mongocryptdURI", out var connectionString))
            {
                connectionString = "mongodb://localhost:27020";
            }

            lock (_processSpawnLock)
            {
                SpawnMongocryptdIfNecessary(extraOptions);
            }
            return new MongoClient(connectionString.ToString());
        }

        private void SpawnMongocryptdIfNecessary(IReadOnlyDictionary<string, object> extraOptions)
        {
            if (!extraOptions.TryGetValue("mongocryptdBypassSpawn", out var mongoCryptBypassSpawn) || (!bool.Parse(mongoCryptBypassSpawn.ToString())))
            {
                if (!extraOptions.TryGetValue("mongocryptdSpawnPath", out var path))
                {
                    path = string.Empty; // look at the current directory or at a system PATH
                    path = @"C:\MongoInstances\4.2.0rc3\bin";
                }

                if (!Path.HasExtension(path.ToString()))
                {
                    string fileName = "mongocryptd.exe";
                    path = Path.Combine(path.ToString(), fileName);
                }

                try
                {
                    using (Process mongoCryptD = new Process())
                    {
                        mongoCryptD.StartInfo.UseShellExecute = true;
                        mongoCryptD.StartInfo.FileName = path.ToString();
                        mongoCryptD.StartInfo.CreateNoWindow = true;
                        if (extraOptions.TryGetValue("mongocryptdSpawnArgs", out var mongocryptdSpawnArgs))
                        {
                            string trimStartHyphens(string str) => str.TrimStart('-').TrimStart('-');
                            var args = string.Empty;
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

                            mongoCryptD.StartInfo.Arguments = args;
                        }
                        //1. todo: serverSelectionTimeoutMS=1000 - is not supported?
                        //2. If spawning is necessary, the driver MUST spawn mongocryptd whenever server selection on the MongoClient to mongocryptd fails. If the MongoClient fails to connect after spawning, the server selection error is propagated to the user.
                        if (!mongoCryptD.Start())
                        {
                            //todo: handling?
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
