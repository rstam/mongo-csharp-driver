/* Copyright 2017-present MongoDB Inc.
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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol.Messages;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents a count command operation.
    /// </summary>
    public class RetryableCountCommandOperation : RetryableReadCommandOperationBase<long>
    {
        
        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="RetryableCountCommandOperation" /> class.
        /// </summary>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        public RetryableCountCommandOperation(
            CollectionNamespace collectionNamespace,
            MessageEncoderSettings messageEncoderSettings)
            : this(collectionNamespace: collectionNamespace,
                collation: null,
                filter: null,
                hint: null,
                limit: null,
                maxTime: null,
                readConcern: new ReadConcern(), 
                skip: null,
                messageEncoderSettings: messageEncoderSettings,
                retryRequested: true)
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="RetryableCountCommandOperation" /> class.
        /// </summary>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        /// <param name="collation">The collation.</param>
        /// <param name="filter">The filter (query).</param>
        /// <param name="hint">The index hint.</param>
        /// <param name="limit">The limit on the number of matching documents to count.</param>
        /// <param name="maxTime">The maximum time the server should spend on this operation.</param>
        /// <param name="readConcern">The readconcern</param>
        /// <param name="skip">The number of documents to skip before counting the remaining matching documents.</param>
        /// <param name="retryRequested">Whether or not retry was requested.</param>
        public RetryableCountCommandOperation(
            CollectionNamespace collectionNamespace,
            Collation collation,
            BsonDocument filter,
            BsonValue hint,
            long? limit,
            TimeSpan? maxTime,
            ReadConcern readConcern,
            long? skip,
            MessageEncoderSettings messageEncoderSettings,
            bool retryRequested)
            : base(
                databaseNamespace: Ensure.IsNotNull(collectionNamespace, nameof(collectionNamespace)).DatabaseNamespace,
                retryRequested: retryRequested,
                readConcern: readConcern,
                messageEncoderSettings: messageEncoderSettings)
        {
            Collation = collation;
            CollectionNamespace = Ensure.IsNotNull(collectionNamespace, nameof(collectionNamespace));
            Filter = filter;
            Hint = hint;
            Limit = limit;
            MaxTime = Ensure.IsNullOrInfiniteOrGreaterThanOrEqualToZero(maxTime, nameof(maxTime));;
            Skip = skip;
        }

        // public properties
        /// <summary>
        /// Gets or sets the collation.
        /// </summary>
        /// <value>
        /// The collation.
        /// </value>
        public Collation Collation { get; }

        /// <summary>
        /// Gets the collection namespace.
        /// </summary>
        /// <value>
        /// The collection namespace.
        /// </value>
        public CollectionNamespace CollectionNamespace { get; }

        /// <summary>
        /// Gets or sets the filter.
        /// </summary>
        /// <value>
        /// The filter.
        /// </value>
        public BsonDocument Filter { get; }

        /// <summary>
        /// Gets or sets the index hint.
        /// </summary>
        /// <value>
        /// The index hint.
        /// </value>
        public BsonValue Hint { get; }

        /// <summary>
        /// Gets or sets a limit on the number of matching documents to count.
        /// </summary>
        /// <value>
        /// A limit on the number of matching documents to count.
        /// </value>
        public long? Limit { get; }

        /// <summary>
        /// Gets or sets the maximum time the server should spend on this operation.
        /// </summary>
        /// <value>
        /// The maximum time the server should spend on this operation.
        /// </value>
        public TimeSpan? MaxTime { get; }

        /// <summary>
        /// Gets or sets the number of documents to skip before counting the remaining matching documents.
        /// </summary>
        /// <value>
        /// The number of documents to skip before counting the remaining matching documents.
        /// </value>
        public long? Skip { get; }

        // protected methods
        /// <inheritdoc />
        protected override BsonDocument CreateCommand(ICoreSessionHandle session, ConnectionDescription connectionDescription, int attempt, long? transactionNumber)
        {
           
            var operation = new CountOperation(CollectionNamespace, MessageEncoderSettings)
            {
                Collation = Collation,
                Filter = Filter,
                Hint = Hint,
                Limit = Limit,
                MaxTime = MaxTime,
                ReadConcern = ReadConcern,
                Skip = Skip
            };
            return operation.CreateCommand(connectionDescription, session);
        }

        /// <inheritdoc />
        protected override async Task<long> ParseCommandResultAsync(RetryableReadContext context, Task<BsonDocument> commandResultTask)
        {
            var commandResult = await commandResultTask.ConfigureAwait(false);
            return commandResult ["n"].ToInt64();
        }

        /// <inheritdoc />
        protected override long ParseCommandResult(RetryableReadContext context, BsonDocument commandResult)
        {
            return commandResult["n"].ToInt64();
        }

    }
}
