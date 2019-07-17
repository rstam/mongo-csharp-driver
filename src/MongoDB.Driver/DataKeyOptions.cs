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
        /// <summary>
        /// Initializes a new instance of the <see cref="DataKeyOptions"/> class.
        /// </summary>
        /// <param name="keyAltNames">The key alt names.</param>
        /// <param name="masterKey">The master key.</param>
        public DataKeyOptions(
            Optional<IReadOnlyList<string>> keyAltNames = default,
            Optional<BsonDocument> masterKey = default)
        {
            _keyAltNames = keyAltNames.WithDefault(null);
            _masterKey = masterKey.WithDefault(null);
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
    }
}
