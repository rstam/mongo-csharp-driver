﻿/* Copyright 2010-present MongoDB Inc.
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

namespace MongoDB.Driver
{
    /// <summary>
    /// Vector search options.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public sealed class VectorSearchOptions<TDocument>
    {
        /// <summary>
        /// Gets or sets the filter.
        /// </summary>
        public FilterDefinition<TDocument> Filter { get; set; }

        /// <summary>
        /// Gets or sets the name of the index.
        /// </summary>
        public string IndexName { get; set; }

        /// <summary>
        /// Gets or sets the number of candidates.
        /// </summary>
        public int? NumberOfCandidates { get; set; }

        /// <summary>
        /// Get or sets a value indicating if exact nearest neighbor (ENN) is to be used, false by default.
        /// If false, approximate nearest neighbor (ANN) is used.
        /// </summary>
        public bool Exact { get; set; }
    }
}
