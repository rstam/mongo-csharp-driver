using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Libmongocrypt;

namespace MongoDB.Driver.Core.Clusters
{
    internal class CryptClientHelper
    {
        #region static
        public static CryptClient CreateCryptClientIfRequired(
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, object>> kmsProviders,
            IReadOnlyDictionary<string, BsonDocument> schemaMap)
        {
            if (kmsProviders == null && schemaMap == null)
            {
                return null;
            }

            var helper = new CryptClientHelper(kmsProviders, schemaMap);
            var cryptOptions = helper.CreateCryptOptions();
            return helper.CreateCryptClient(cryptOptions);
        }
        #endregion

        private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, object>> _kmsProviders;
        private readonly IReadOnlyDictionary<string, BsonDocument> _schemaMap;

        private CryptClientHelper(
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, object>> kmsProviders,
            IReadOnlyDictionary<string, BsonDocument> schemaMap)
        {
            _kmsProviders = kmsProviders;
            _schemaMap = schemaMap;
        }

        private CryptClient CreateCryptClient(CryptOptions options)
        {
            return CryptClientFactory.Create(options);
        }

        private CryptOptions CreateCryptOptions()
        {
            Dictionary<KmsType, IKmsCredentials> kmsProvidersMap = null;
            if (_kmsProviders != null && _kmsProviders.Any())
            {
                kmsProvidersMap = new Dictionary<KmsType, IKmsCredentials>();
                if (_kmsProviders.TryGetValue("aws", out var awsProvider))
                {
                    if (awsProvider.TryGetValue("accessKeyId", out var accessKeyId) &&
                        awsProvider.TryGetValue("secretAccessKey", out var secretAccessKey))
                    {
                        kmsProvidersMap.Add(KmsType.Aws, new AwsKmsCredentials((string)secretAccessKey, (string)accessKeyId));
                    }
                }
                if (_kmsProviders.TryGetValue("local", out var localProvider))
                {
                    if (localProvider.TryGetValue("key", out var keyObject) && keyObject is byte[] key)
                    {
                        kmsProvidersMap.Add(KmsType.Local, new LocalKmsCredentials(key));
                    }
                }
            }
            else
            {
                throw new ArgumentException("At least one kms provider must be specified");
            }

            byte[] schemaBytes = null;
            var schemaMap = _schemaMap;
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
}
