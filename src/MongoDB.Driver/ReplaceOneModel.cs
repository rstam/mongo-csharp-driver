/* Copyright 2010-present MongoDB Inc.
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

namespace MongoDB.Driver
{
    /// <summary>
    /// Model for replacing a single document.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public sealed class ReplaceOneModel<TDocument> : WriteModel<TDocument>
    {
        // fields
        private Collation _collation;
        private readonly FilterDefinition<TDocument> _filter;
        private BsonValue _hint;
        private bool _isUpsert;
        private readonly TDocument _replacement;
        private SortDefinition<TDocument> _sort;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ReplaceOneModel{TDocument}"/> class.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="replacement">The replacement.</param>
        public ReplaceOneModel(FilterDefinition<TDocument> filter, TDocument replacement)
        {
            _filter = Ensure.IsNotNull(filter, nameof(filter));
            _replacement = replacement;
        }

        // properties
        /// <summary>
        /// Gets or sets the collation.
        /// </summary>
        public Collation Collation
        {
            get { return _collation; }
            set { _collation = value; }
        }

        /// <summary>
        /// Gets the filter.
        /// </summary>
        public FilterDefinition<TDocument> Filter
        {
            get { return _filter; }
        }

        /// <summary>
        /// Gets or sets the hint.
        /// </summary>
        public BsonValue Hint
        {
            get { return _hint; }
            set { _hint = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to insert the document if it doesn't already exist.
        /// </summary>
        public bool IsUpsert
        {
            get { return _isUpsert; }
            set { _isUpsert = value; }
        }

        /// <summary>
        /// Gets the replacement.
        /// </summary>
        public TDocument Replacement
        {
            get { return _replacement; }
        }

        /// <summary>
        /// Gets or sets the sort definition.
        /// </summary>
        public SortDefinition<TDocument> Sort
        {
            get { return _sort; }
            set { _sort = value; }
        }

        /// <summary>
        /// Gets the type of the model.
        /// </summary>
        public override WriteModelType ModelType
        {
            get { return WriteModelType.ReplaceOne; }
        }
    }
}
