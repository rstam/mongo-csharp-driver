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

namespace MongoDB.Driver
{
    /// <summary>
    /// Options for creating a data key.
    /// </summary>
    public class DataKeyOptions
    {
        // private fields
        private readonly IReadOnlyList<string> _keyAltNames;
        private readonly BsonDocument _masterKey;

        // constructors
        private DataKeyOptions(
            IReadOnlyList<string> keyAltNames,
            BsonDocument masterKey)
        {
            _keyAltNames = keyAltNames;
            _masterKey = masterKey;
        }

        // public properties            
        /// <summary>
        /// Gets the master key.
        /// </summary>
        /// <value>
        /// The master key.
        /// </value>
        public BsonDocument MasterKey => _masterKey;

        /// <summary>
        /// Gets  the key alt names.
        /// </summary>
        /// <value>
        /// The key alt names.
        /// </value>
        public IReadOnlyList<string> KeyAltNames => _keyAltNames;

        // nested types        
        /// <summary>
        /// A builder of DataKeyOptions instances.
        /// </summary>
        public class Builder
        {
            // private fields
            private BsonDocument _masterKey;
            private IReadOnlyList<string> _keyAltNames;

            // public properties            
            /// <summary>
            /// Gets or sets the key alt names.
            /// </summary>
            /// <value>
            /// The key alt names.
            /// </value>
            public IReadOnlyList<string> KeyAltNames
            {
                get => _keyAltNames;
                set => _keyAltNames = value;
            }

            /// <summary>
            /// Gets or sets the master key.
            /// </summary>
            /// <value>
            /// The master key.
            /// </value>
            public BsonDocument MasterKey
            {
                get => _masterKey;
                set => _masterKey = value;
            }

            // public methods            
            /// <summary>
            /// Builds the instance.
            /// </summary>
            /// <returns>An instance of DataKeyOptions.</returns>
            public DataKeyOptions Build()
            {
                return new DataKeyOptions(_keyAltNames, _masterKey);
            }
        }
    }
}
