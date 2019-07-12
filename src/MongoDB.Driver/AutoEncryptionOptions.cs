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
using MongoDB.Bson;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver
{
    /// <summary>
    /// Auto encryption options.
    /// </summary>
    public class AutoEncryptionOptions
    {
        // private fields
        private readonly bool _bypassAutoEncryption;
        private readonly IReadOnlyDictionary<string, object> _extraOptions;
        private readonly IMongoClient _keyVaultClient;
        private readonly CollectionNamespace _keyVaultNamespace;
        private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, object>> _kmsProviders;
        private readonly IReadOnlyDictionary<string, BsonDocument> _schemaMap;

        // constructors
        private AutoEncryptionOptions(
            bool bypassAutoEncryption,
            IReadOnlyDictionary<string, object> extraOptions,
            IMongoClient keyVaultClient,
            CollectionNamespace keyVaultNamespace,
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, object>> kmsProviders,
            IReadOnlyDictionary<string, BsonDocument> schemaMap)
        {
            _bypassAutoEncryption = bypassAutoEncryption;
            _extraOptions = extraOptions;
            _keyVaultClient = keyVaultClient;
            _keyVaultNamespace = Ensure.IsNotNull(keyVaultNamespace, nameof(keyVaultNamespace));
            _kmsProviders = Ensure.IsNotNull(kmsProviders, nameof(kmsProviders));
            _schemaMap = schemaMap;
        }

        // public properties
        /// <summary>
        /// Gets a value indicating whether to bypass automatic encryption.
        /// </summary>
        /// <value>
        ///   <c>true</c> if automatic encryption should be bypasssed; otherwise, <c>false</c>.
        /// </value>
        public bool BypassAutoEncryption => _bypassAutoEncryption;

        /// <summary>
        /// Gets the extra options.
        /// </summary>
        /// <value>
        /// The extra options.
        /// </value>
        public IReadOnlyDictionary<string, object> ExtraOptions => _extraOptions;

        /// <summary>
        /// Gets the key vault client.
        /// </summary>
        /// <value>
        /// The key vault client.
        /// </value>
        public IMongoClient KeyVaultClient => _keyVaultClient;

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

        /// <summary>
        /// Gets the schema map.
        /// </summary>
        /// <value>
        /// The schema map.
        /// </value>
        public IReadOnlyDictionary<string, BsonDocument> SchemaMap => _schemaMap;

        // nested types        
        /// <summary>
        /// A builder of AutoEncryptionOptions instances.
        /// </summary>
        public class Builder
        {
            // private fields
            private bool _bypassAutoEncryption;
            private IReadOnlyDictionary<string, object> _extraOptions;
            private IMongoClient _keyVaultClient;
            private CollectionNamespace _keyVaultNamespace;
            private IReadOnlyDictionary<string, IReadOnlyDictionary<string, object>> _kmsProviders;
            private IReadOnlyDictionary<string, BsonDocument> _schemaMap;

            // public properties            
            /// <summary>
            /// Gets or sets a value indicating whether to bypass automatic encryption.
            /// </summary>
            /// <value>
            ///   <c>true</c> if automatic encryption should be bypasssed; otherwise, <c>false</c>.
            /// </value>
            public bool BypassAutoEncryption
            {
                get => _bypassAutoEncryption;
                set => _bypassAutoEncryption = value;
            }

            /// <summary>
            /// Gets or sets the extra options.
            /// </summary>
            /// <value>
            /// The extra options.
            /// </value>
            public IReadOnlyDictionary<string, object> ExtraOptions
            {
                get => _extraOptions;
                set => _extraOptions = value;
            }

            /// <summary>
            /// Gets or sets the key vault client.
            /// </summary>
            /// <value>
            /// The key vault client.
            /// </value>
            public IMongoClient KeyVaultClient
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

            /// <summary>
            /// Gets or sets the schema map.
            /// </summary>
            /// <value>
            /// The schema map.
            /// </value>
            public IReadOnlyDictionary<string, BsonDocument> SchemaMap
            {
                get => _schemaMap;
                set => _schemaMap = value;
            }

            // public methods            
            /// <summary>
            /// Builds the AutoEncryptionOptions instance.
            /// </summary>
            /// <returns>An AutoEncryptionOptions. </returns>
            public AutoEncryptionOptions Build()
            {
                return new AutoEncryptionOptions(
                    _bypassAutoEncryption,
                    _extraOptions,
                    _keyVaultClient,
                    _keyVaultNamespace,
                    _kmsProviders,
                    _schemaMap);
            }
        }
    }
}
