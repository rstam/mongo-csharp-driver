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

using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver
{
    /// <summary>
    /// Encryption options for explicit encryption.
    /// </summary>
    public class EncryptOptions
    {
        // private fields
        private readonly string _algorithm;
        private readonly string _keyAltName;
        private readonly byte[] _keyId;

        // constructors
        private EncryptOptions(
            string algorithm,
            string keyAltName,
            byte[] keyId)
        {
            _algorithm = Ensure.IsNotNull(algorithm, nameof(algorithm));
            _keyAltName = keyAltName;
            _keyId = keyId;
        }

        // public properties                    
        /// <summary>
        /// Gets the algorithm.
        /// </summary>
        /// <value>
        /// The algorithm.
        /// </value>
        public string Algorithm=> _algorithm;

        /// <summary>
        /// Gets the alt key name.
        /// </summary>
        /// <value>
        /// The alt key name.
        /// </value>
        public string KeyAltName => _keyAltName;

        /// <summary>
        /// Gets the key identifier.
        /// </summary>
        /// <value>
        /// The key identifier.
        /// </value>
        public byte[] KeyId => _keyId;

        // nested types        
        /// <summary>
        /// A builder of EncryptOptions instances.
        /// </summary>
        public class Builder
        {
            // private fields
            private string _algorithm;
            private string _keyAltName;
            private byte[] _keyId;

            // public properties            
            /// <summary>
            /// Gets or sets the algorithm.
            /// </summary>
            /// <value>
            /// The algorithm.
            /// </value>
            public string Algorithm
            {
                get => _algorithm;
                set => _algorithm = value;
            }

            /// <summary>
            /// Gets or sets the alt key name.
            /// </summary>
            /// <value>
            /// The alt key name.
            /// </value>
            public string KeyAltName
            {
                get => _keyAltName;
                set => _keyAltName = value;
            }

            /// <summary>
            /// Gets or sets the key identifier.
            /// </summary>
            /// <value>
            /// The key identifier.
            /// </value>
            public byte[] KeyId
            {
                get => _keyId;
                set => _keyId = value;
            }

            // public methods            
            /// <summary>
            /// Builds the instance.
            /// </summary>
            /// <returns>An EncryptOptions instance.</returns>
            public EncryptOptions Build()
            {
                return new EncryptOptions(_algorithm, _keyAltName, _keyId);
            }
        }
    }
}
