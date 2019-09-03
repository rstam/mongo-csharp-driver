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
            return _libMongoCryptController.CreateDataKey(
                kmsProvider, 
                dataKeyOptions.AlternateKeyNames, 
                dataKeyOptions.MasterKey, 
                cancellationToken);
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
            return await _libMongoCryptController
                .CreateDataKeyAsync(
                    kmsProvider,
                    dataKeyOptions.AlternateKeyNames,
                    dataKeyOptions.MasterKey, 
                    cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Decrypts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The decrypted value.</returns>
        public BsonBinaryData Decrypt(BsonBinaryData value, CancellationToken cancellationToken)
        {
            return _libMongoCryptController.DecryptField(value, cancellationToken);
        }

        /// <summary>
        /// Decrypts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The decrypted value.</returns>
        public async Task<BsonBinaryData> DecryptAsync(BsonBinaryData value, CancellationToken cancellationToken)
        {
            return await _libMongoCryptController.DecryptFieldAsync(value, cancellationToken).ConfigureAwait(false);
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
            GetEncryptFieldArgs(encryptOptions, out var keyId, out var algorithm);
            return _libMongoCryptController.EncryptField(
                value,
                keyId,
                encryptOptions.AlternateKeyName,
                algorithm,
                cancellationToken);
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
            GetEncryptFieldArgs(encryptOptions, out var keyId, out var algorithm);
            return await _libMongoCryptController
                .EncryptFieldAsync(
                    value,
                    keyId,
                    encryptOptions.AlternateKeyName,
                    algorithm,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        private void GetEncryptFieldArgs(
            EncryptOptions encryptOptions,
            out Guid? keyId,
            out EncryptionAlgorithm algorithm)
        {
            keyId = encryptOptions.KeyId != null ? new Guid(encryptOptions.KeyId) : (Guid?)null;
            algorithm = (EncryptionAlgorithm)Enum.Parse(typeof(EncryptionAlgorithm), encryptOptions.Algorithm);
        }
    }
}
