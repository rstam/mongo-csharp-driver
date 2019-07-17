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
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Crypt;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.LibMongoCrypt;

namespace MongoDB.Driver
{
    /// <summary>
    /// Explicit client encryption.
    /// </summary>
    public class ClientEncryption
    {
        // private fields
        private readonly LibMongoCryptController _libMongoCryptController;
        private readonly ClientEncryptionOptions _options;

        // constructors
        internal ClientEncryption(
            LibMongoCryptController libMongoCryptController,
            ClientEncryptionOptions options)
        {
            _libMongoCryptController = Ensure.IsNotNull(libMongoCryptController, nameof(libMongoCryptController));
            _options = Ensure.IsNotNull(options, nameof(options));
        }

        // public methods
        /// <summary>
        /// Creates a data key.
        /// </summary>
        /// <param name="dataKeyOptions">The data key options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A data key.</returns>
        public BsonValue CreateDataKey(DataKeyOptions dataKeyOptions, CancellationToken cancellationToken)
        {
            var key = ParseKmsKeyId(dataKeyOptions.MasterKey);
            return _libMongoCryptController.GenerateKey(key, cancellationToken);
        }

        /// <summary>
        /// Creates a data key.
        /// </summary>
        /// <param name="dataKeyOptions">The data key options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A data key.</returns>
        public async Task<BsonValue> CreateDataKeyAsync(DataKeyOptions dataKeyOptions, CancellationToken cancellationToken)
        {
            var key = ParseKmsKeyId(dataKeyOptions.MasterKey);
            return await _libMongoCryptController.GenerateKeyAsync(key, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Decrypts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The decrypted value.</returns>
        public BsonValue Decrypt(BsonBinaryData value, CancellationToken cancellationToken)
        {
            return _libMongoCryptController.DecryptField(value.Bytes, cancellationToken);
        }

        /// <summary>
        /// Decrypts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The decrypted value.</returns>
        public async Task<BsonValue> DecryptAsync(BsonBinaryData value, CancellationToken cancellationToken)
        {
            return await _libMongoCryptController.DecryptFieldAsync(value.Bytes, cancellationToken).ConfigureAwait(false);
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
            var bytes = value.ToBson(); //todo:
            return _libMongoCryptController.EncryptField(bytes, encryptOptions, cancellationToken);
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
            var bytes = value.ToBson(); //todo:
            return await _libMongoCryptController.EncryptFieldAsync(bytes, encryptOptions, cancellationToken).ConfigureAwait(false);
        }

        private IKmsKeyId ParseKmsKeyId(BsonDocument masterKey)
        {
            if (!masterKey.TryGetValue("key", out var customerMasterKey))
            {
                throw new ArgumentException("TODO");
            }

            if (!masterKey.TryGetValue("region", out var region))
            {
                throw new ArgumentException("TODO");
            }

            return new AwsKeyId(customerMasterKey.ToString(), region.ToString());
        }
    }
}
