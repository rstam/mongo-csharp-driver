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
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Misc;
using MongoDB.Libmongocrypt;

namespace MongoDB.Driver
{
    /// <summary>
    /// Explicit client encryption.
    /// </summary>
    public class ClientEncryption
    {
        // private fields
        private readonly LibMongoCryptController _libMongoCryptController;

        // constructors
        internal ClientEncryption(LibMongoCryptController libMongoCryptController)
        {
            _libMongoCryptController = Ensure.IsNotNull(libMongoCryptController, nameof(libMongoCryptController));
        }

        // public methods
        /// <summary>
        /// Creates a data key.
        /// </summary>
        /// <param name="kmsProvider">The kms provider.</param>
        /// <param name="dataKeyOptions">The data key options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A data key.</returns>
        public BsonValue CreateDataKey(string kmsProvider, DataKeyOptions dataKeyOptions, CancellationToken cancellationToken)
        {
            var kmsKeyId = GetKmsId(kmsProvider, dataKeyOptions);
            var dataKey = _libMongoCryptController.CreateDataKey(kmsKeyId, cancellationToken);
            return new BsonBinaryData(dataKey, GuidRepresentation.Standard);
        }

        /// <summary>
        /// Creates a data key.
        /// </summary>
        /// <param name="kmsProvider">The kms provider.</param>
        /// <param name="dataKeyOptions">The data key options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A data key.</returns>
        public async Task<BsonValue> CreateDataKeyAsync(string kmsProvider, DataKeyOptions dataKeyOptions, CancellationToken cancellationToken)
        {
            var kmsKeyId = GetKmsId(kmsProvider, dataKeyOptions);
            var dataKey = await _libMongoCryptController.CreateDataKeyAsync(kmsKeyId, cancellationToken).ConfigureAwait(false);
            return new BsonBinaryData(dataKey, GuidRepresentation.Standard);
        }

        /// <summary>
        /// Decrypts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The decrypted value.</returns>
        public BsonBinaryData Decrypt(BsonBinaryData value, CancellationToken cancellationToken)
        {
            var wrappedValueBytes = GetWrappedValueBytes(value);
            return _libMongoCryptController.DecryptField(wrappedValueBytes, cancellationToken);
        }

        /// <summary>
        /// Decrypts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The decrypted value.</returns>
        public async Task<BsonBinaryData> DecryptAsync(BsonBinaryData value, CancellationToken cancellationToken)
        {
            var wrappedValueBytes = GetWrappedValueBytes(value);
            return await _libMongoCryptController.DecryptFieldAsync(wrappedValueBytes, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Encrypts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="encryptOptions">The encrypt options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The encrypted value.</returns>
        public BsonBinaryData Encrypt(BsonValue value, EncryptOptions encryptOptions, CancellationToken cancellationToken)
        {
            var unencryptedWrappedValueBytes = GetWrappedValueBytes(value);
            GetEncryptFieldArgs(encryptOptions, out var keyId, out var keyAltNameBytes, out var algorithm);
            var encryptedWrappedValueBytes = _libMongoCryptController.EncryptField(
                unencryptedWrappedValueBytes,
                keyId,
                keyAltNameBytes,
                algorithm,
                cancellationToken);
            return UnwrapEncryptedValue(encryptedWrappedValueBytes);
        }

        /// <summary>
        /// Encrypts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="encryptOptions">The encrypt options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The encrypted value.</returns>
        public async Task<BsonBinaryData> EncryptAsync(BsonValue value, EncryptOptions encryptOptions, CancellationToken cancellationToken)
        {
            var unencryptedWrappedValueBytes = GetWrappedValueBytes(value);
            GetEncryptFieldArgs(encryptOptions, out var keyId, out var keyAltNameBytes, out var algorithm);
            var encryptedWrappedValueBytes = await _libMongoCryptController
                .EncryptFieldAsync(
                    unencryptedWrappedValueBytes,
                    keyId,
                    keyAltNameBytes,
                    algorithm,
                    cancellationToken)
                .ConfigureAwait(false);
            return UnwrapEncryptedValue(encryptedWrappedValueBytes);
        }

        private void GetEncryptFieldArgs(
            EncryptOptions encryptOptions,
            out Guid? keyId,
            out byte[] alternateKeyNameDocuments,
            out EncryptionAlgorithm algorithm)
        {
            keyId = encryptOptions.KeyId != null ? new Guid(encryptOptions.KeyId) : (Guid?)null;
            algorithm = (EncryptionAlgorithm)Enum.Parse(typeof(EncryptionAlgorithm), encryptOptions.Algorithm);
            alternateKeyNameDocuments =
                !string.IsNullOrWhiteSpace(encryptOptions.KeyAltName)
                    ? new BsonDocument("keyAltName", encryptOptions.KeyAltName).ToBson(serializer: BsonValueSerializer.Instance)
                    : null;
        }

        private IKmsKeyId GetKmsId(string kmsProvider, DataKeyOptions dataKeyOptions)
        {
            IEnumerable<byte[]> alternateKeyNameDocuments = null;
            if (dataKeyOptions.KeyAltNames != null)
            {
                alternateKeyNameDocuments = dataKeyOptions
                    .KeyAltNames
                    .Select(c => new BsonDocument("keyAltName", c))
                    .Select(c => c.ToBson());
            }

            switch (kmsProvider)
            {
                case "aws":
                    var masterKey = dataKeyOptions.MasterKey;
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

        private byte[] GetWrappedValueBytes(BsonValue value)
        {
            var writerSettings = BsonBinaryWriterSettings.Defaults.Clone();
            writerSettings.GuidRepresentation = GuidRepresentation.Unspecified;
            var wrappedValue = new BsonDocument("v", value);
            return wrappedValue.ToBson(serializer: BsonValueSerializer.Instance, writerSettings: writerSettings);
        }

        private BsonBinaryData UnwrapEncryptedValue(byte[] encryptedWrappedBytes)
        {
            var rawDocument = new RawBsonDocument(encryptedWrappedBytes);
            var encryptedBytes = rawDocument["v"].AsByteArray;
            return new BsonBinaryData(encryptedBytes, BsonBinarySubType.Encrypted);
        }
    }
}
