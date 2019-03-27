/* Copyright 2013-present MongoDB Inc.
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
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents the listDatabases command.
    /// </summary>
    public class ListDatabasesOperation : RetryableReadCommandOperationBase<IAsyncCursor<BsonDocument>>
    {
        // fields
        private BsonDocument _filter;
        private bool? _nameOnly;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ListDatabasesOperation"/> class.
        /// </summary>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        public ListDatabasesOperation(MessageEncoderSettings messageEncoderSettings) 
            : base(DatabaseNamespace.Admin, messageEncoderSettings)
        {
        }

        // properties
        /// <summary>
        /// Gets or sets the filter.
        /// </summary>
        /// <value>
        /// The filter.
        /// </value>
        public BsonDocument Filter
        {
            get { return _filter; }
            set { _filter = value; }
        }

        /// <summary>
        /// Gets or sets the NameOnly flag.
        /// </summary>
        /// <value>
        /// The NameOnly flag.
        /// </value>
        public bool? NameOnly
        {
            get { return _nameOnly; }
            set { _nameOnly = value; }
        }

        // public methods
        /// <inheritdoc />
        public override IAsyncCursor<BsonDocument> ExecuteAttempt(RetryableReadContext context, int attempt, long? transactionNumber, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(context, nameof(context));
            var operation = CreateOperation();
            var reply = operation.ExecuteWithChannelBinding(context, cancellationToken);
            return CreateCursor(reply);
        }

        /// <inheritdoc />
        public override async Task<IAsyncCursor<BsonDocument>> ExecuteAttemptAsync(RetryableReadContext context, int attempt, long? transactionNumber, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(context, nameof(context));
            var operation = CreateOperation();
            var reply = await operation.ExecuteWithChannelBindingAsync(context, cancellationToken).ConfigureAwait(false);
            return CreateCursor(reply);
        }

        /// <inheritdoc />
        protected override BsonDocument CreateCommand(ICoreSessionHandle session, ConnectionDescription connectionDescription, int attempt, long? transactionNumber)
        {
            return CreateCommand();
        }

        // private methods
        internal BsonDocument CreateCommand()
        {
            return new BsonDocument
            {
                { "listDatabases", 1 },
                { "filter", _filter, _filter != null },
                { "nameOnly", _nameOnly, _nameOnly != null }
            };
        }

        private IAsyncCursor<BsonDocument> CreateCursor(BsonDocument reply)
        {
            var databases = reply["databases"].AsBsonArray.OfType<BsonDocument>();
            return new SingleBatchAsyncCursor<BsonDocument>(databases.ToList());
        }

        private ReadCommandOperation<BsonDocument> CreateOperation()
        {
            var command = CreateCommand();
            return new ReadCommandOperation<BsonDocument>(DatabaseNamespace.Admin, command, BsonDocumentSerializer.Instance, MessageEncoderSettings);
        }
    }
}
