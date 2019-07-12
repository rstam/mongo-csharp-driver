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

using System.Collections.Generic;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver
{
    /// <summary>
    /// Client encryption options.
    /// </summary>
    public class ClientEncryptionOptions
    {
        // private fields
        private IMongoClient _keyVaultClient;
        private CollectionNamespace _keyVaultNamespace;
        private IReadOnlyDictionary<string, IReadOnlyDictionary<string, object>> _kmsProviders;

        // constructors        
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientEncryptionOptions"/> class.
        /// </summary>
        /// <param name="keyVaultClient">The key vault client.</param>
        /// <param name="keyVaultNamespace">The key vault namespace.</param>
        /// <param name="kmsProviders">The KMS providers.</param>
        private ClientEncryptionOptions(
            IMongoClient keyVaultClient,
            CollectionNamespace keyVaultNamespace,
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, object>> kmsProviders)            
        {
            _keyVaultClient = Ensure.IsNotNull(keyVaultClient, nameof(keyVaultClient));
            _keyVaultNamespace = Ensure.IsNotNull(keyVaultNamespace, nameof(keyVaultNamespace));
            _kmsProviders = Ensure.IsNotNull(kmsProviders, nameof(kmsProviders));
        }

        // public properties        
        /// <summary>
        /// Gets the key value client.
        /// </summary>
        /// <value>
        /// The key value client.
        /// </value>
        public IMongoClient KeyValueClient => _keyVaultClient;

        /// <summary>
        /// Gets the key vault namespace.
        /// </summary>
        /// <value>
        /// The key vault namespace.
        /// </value>
        public CollectionNamespace KeyVaultNamespace => _keyVaultNamespace;

        /// <summary>
        /// Gets the KMS providers.
        /// </summary>
        /// <value>
        /// The KMS providers.
        /// </value>
        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, object>> KmsProviders => _kmsProviders;

        // nested types        
        /// <summary>
        /// A builder of ClientEncryptionOptions instances.
        /// </summary>
        public class Builder
        {
            // private fields
            private IMongoClient _keyVaultClient;
            private CollectionNamespace _keyVaultNamespace;
            private IReadOnlyDictionary<string, IReadOnlyDictionary<string, object>> _kmsProviders;

            // public properties            
            /// <summary>
            /// Gets or sets the key value client.
            /// </summary>
            /// <value>
            /// The key value client.
            /// </value>
            public IMongoClient KeyValueClient
            {
                get => _keyVaultClient;
                set => _keyVaultClient = value;
            }

            /// <summary>
            /// Gets or sets the key vault namespace.
            /// </summary>
            /// <value>
            /// The key vault namespace.
            /// </value>
            public CollectionNamespace KeyVaultNamespace
            {
                get => _keyVaultNamespace;
                set => _keyVaultNamespace = value;
            }

            /// <summary>
            /// Gets or sets the KMS providers.
            /// </summary>
            /// <value>
            /// The KMS providers.
            /// </value>
            public IReadOnlyDictionary<string, IReadOnlyDictionary<string, object>> KmsProviders
            {
                get => _kmsProviders;
                set => _kmsProviders = value;
            }

            // public methods            
            /// <summary>
            /// Builds the ClientEncryptionOptions instance.
            /// </summary>
            /// <returns>The ClientEncryptionOptions instance.</returns>
            public ClientEncryptionOptions Build()
            {
                return new ClientEncryptionOptions(
                    _keyVaultClient,
                    _keyVaultNamespace,
                    _kmsProviders);
            }
        }
    }
}
