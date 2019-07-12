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
using MongoDB.Bson;
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
        //private readonly LibMongoCryptController _libMongoCryptController;
        private readonly ClientEncryptionOptions _options;

        // constructors        
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientEncryption"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public ClientEncryption(ClientEncryptionOptions options)
        {
            _options = Ensure.IsNotNull(options, nameof(options));
        }

        // public methods        
        /// <summary>
        /// Creates a data key.
        /// </summary>
        /// <param name="kmsProvider">The KMS provider.</param>
        /// <param name="dataKeyOptions">The data key options.</param>
        /// <returns>A data key.</returns>
        public BsonValue CreateDataKey(string kmsProvider, DataKeyOptions dataKeyOptions)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Decrypts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The decrypted value.</returns>
        public BsonValue Decrypt(BsonBinaryData value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Encrypts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="encryptOptions">The encrypt options.</param>
        /// <returns>The encrypted value.</returns>
        public BsonBinaryData Encrypt(BsonValue value, EncryptOptions encryptOptions)
        {
            throw new NotImplementedException();
        }
    }
}
