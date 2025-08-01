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

using System;
using MongoDB.Bson;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver
{
    /// <summary>
    /// Options for an aggregate operation.
    /// </summary>
    public class AggregateOptions
    {
        // fields
        private bool? _allowDiskUse;
        private int? _batchSize;
        private bool? _bypassDocumentValidation;
        private Collation _collation;
        private BsonValue _comment;
        private BsonValue _hint;
        private BsonDocument _let;
        private TimeSpan? _maxAwaitTime;
        private TimeSpan? _maxTime;
        private TimeSpan? _timeout;
        private ExpressionTranslationOptions _translationOptions;
        private bool? _useCursor;

        // implicit conversions
        /// <summary>
        /// Creates an AggregateOptions instance with the specified translation options.
        /// </summary>
        /// <param name="translationOptions">The translation options.</param>
        /// <returns>An AggregateOptions instance.</returns>
        public static implicit operator AggregateOptions(ExpressionTranslationOptions translationOptions) =>
            new AggregateOptions { TranslationOptions = translationOptions };

        // properties
        /// <summary>
        /// Gets or sets a value indicating whether to allow disk use.
        /// </summary>
        public bool? AllowDiskUse
        {
            get { return _allowDiskUse; }
            set { _allowDiskUse = value; }
        }

        /// <summary>
        /// Gets or sets the size of a batch.
        /// </summary>
        public int? BatchSize
        {
            get { return _batchSize; }
            set { _batchSize = Ensure.IsNullOrGreaterThanOrEqualToZero(value, nameof(value)); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to bypass document validation.
        /// </summary>
        public bool? BypassDocumentValidation
        {
            get { return _bypassDocumentValidation; }
            set { _bypassDocumentValidation = value; }
        }

        /// <summary>
        /// Gets or sets the collation.
        /// </summary>
        public Collation Collation
        {
            get { return _collation; }
            set { _collation = value; }
        }
        /// <summary>
        /// Gets or sets the comment.
        /// </summary>
        public BsonValue Comment
        {
            get { return _comment; }
            set { _comment = value; }
        }

        /// <summary>
        /// Gets or sets the hint. This must either be a BsonString representing the index name or a BsonDocument representing the key pattern of the index.
        /// </summary>
        public BsonValue Hint
        {
            get { return _hint; }
            set { _hint = value; }
        }

        /// <summary>
        /// Gets or sets the "let" definition.
        /// </summary>
        public BsonDocument Let
        {
            get { return _let; }
            set { _let = value; }
        }

        /// <summary>
        /// Gets or sets the maximum await time.
        /// </summary>
        public TimeSpan? MaxAwaitTime
        {
            get { return _maxAwaitTime; }
            set { _maxAwaitTime = value; }
        }

        /// <summary>
        /// Gets or sets the maximum time.
        /// </summary>
        public TimeSpan? MaxTime
        {
            get { return _maxTime; }
            set { _maxTime = Ensure.IsNullOrInfiniteOrGreaterThanOrEqualToZero(value, nameof(value)); }
        }

        /// <summary>
        /// Gets or sets the operation timeout.
        /// </summary>
        // TODO: CSOT: Make it public when CSOT will be ready for GA
        internal TimeSpan? Timeout
        {
            get => _timeout;
            set => _timeout = Ensure.IsNullOrValidTimeout(value, nameof(Timeout));
        }

        /// <summary>
        /// Gets or sets the translation options.
        /// </summary>
        public ExpressionTranslationOptions TranslationOptions
        {
            get { return _translationOptions; }
            set { _translationOptions = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use a cursor.
        /// </summary>
        [Obsolete("Server versions 3.6 and newer always use a cursor.")]
        public bool? UseCursor
        {
            get { return _useCursor; }
            set { _useCursor = value; }
        }
    }
}
