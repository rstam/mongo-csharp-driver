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

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Connections;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// A change stream operation.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public interface IChangeStreamOperation<TResult> : IReadOperation<IAsyncCursor<TResult>>
    {
        // properties
        /// <summary>
        /// Gets or sets the document resume token.
        /// </summary>
        /// <value>
        /// The document resume token.
        /// </value>
        BsonDocument DocumentResumeToken { get; set; }

        /// <summary>
        /// Gets or sets the resume after value.
        /// </summary>
        /// <value>
        /// The resume after value.
        /// </value>
        BsonDocument ResumeAfter { get; set; }

        /// <summary>
        /// Gets or sets the start at operation time.
        /// </summary>
        /// <value>
        /// The start at operation time.
        /// </value>
        BsonTimestamp StartAtOperationTime { get; set; }

        // methods
        /// <summary>
        /// Resumes the operation after a resume token.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A cursor.</returns>
        IAsyncCursor<RawBsonDocument> Resume(IReadBinding binding, CancellationToken cancellationToken);

        /// <summary>
        /// Resumes the operation after a resume token.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is a cursor.</returns>
        Task<IAsyncCursor<RawBsonDocument>> ResumeAsync(IReadBinding binding, CancellationToken cancellationToken);
    }

    /// <summary>
    /// A change stream operation.
    /// </summary>
    /// <typeparam name="TResult">The type of the result values.</typeparam>
    public class ChangeStreamOperation<TResult> : RetryableReadCommandOperationBase<IAsyncCursor<TResult>>, IChangeStreamOperation<TResult>
    {
        // private fields
        private readonly bool _allChangesForCluster;
        private int? _batchSize;
        private Collation _collation;
        private readonly CollectionNamespace _collectionNamespace;
        private BsonDocument _documentResumeToken;
        private ChangeStreamFullDocumentOption _fullDocument = ChangeStreamFullDocumentOption.Default;
        private BsonTimestamp _initialOperationTime;
        private TimeSpan? _maxAwaitTime;
        private readonly IReadOnlyList<BsonDocument> _pipeline;
        private readonly IBsonSerializer<TResult> _resultSerializer;
        private BsonDocument _resumeAfter;
        private BsonDocument _startAfter;
        private BsonTimestamp _startAtOperationTime;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeStreamOperation{TResult}"/> class.
        /// </summary>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="resultSerializer">The result value serializer.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        public ChangeStreamOperation(
            IEnumerable<BsonDocument> pipeline,
            IBsonSerializer<TResult> resultSerializer,
            MessageEncoderSettings messageEncoderSettings)
            : base(DatabaseNamespace.Admin, messageEncoderSettings)
        {
            _allChangesForCluster = true;
            _pipeline = Ensure.IsNotNull(pipeline, nameof(pipeline)).ToList();
            _resultSerializer = Ensure.IsNotNull(resultSerializer, nameof(resultSerializer));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeStreamOperation{TResult}"/> class.
        /// </summary>
        /// <param name="databaseNamespace">The database namespace.</param>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="resultSerializer">The result value serializer.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        public ChangeStreamOperation(
            DatabaseNamespace databaseNamespace,
            IEnumerable<BsonDocument> pipeline,
            IBsonSerializer<TResult> resultSerializer,
            MessageEncoderSettings messageEncoderSettings)
            : base(databaseNamespace, messageEncoderSettings)
        {
            _pipeline = Ensure.IsNotNull(pipeline, nameof(pipeline)).ToList();
            _resultSerializer = Ensure.IsNotNull(resultSerializer, nameof(resultSerializer));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeStreamOperation{TResult}"/> class.
        /// </summary>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="resultSerializer">The result value serializer.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        public ChangeStreamOperation(
            CollectionNamespace collectionNamespace,
            IEnumerable<BsonDocument> pipeline,
            IBsonSerializer<TResult> resultSerializer,
            MessageEncoderSettings messageEncoderSettings)
            : this(Ensure.IsNotNull(collectionNamespace, nameof(collectionNamespace)).DatabaseNamespace, pipeline, resultSerializer, messageEncoderSettings)
        {
            _collectionNamespace = Ensure.IsNotNull(collectionNamespace, nameof(collectionNamespace));
        }

        // public properties
        /// <summary>
        /// Gets or sets the size of the batch.
        /// </summary>
        /// <value>
        /// The size of the batch.
        /// </value>
        public int? BatchSize
        {
            get { return _batchSize; }
            set { _batchSize = value; }
        }

        /// <summary>
        /// Gets or sets the collation.
        /// </summary>
        /// <value>
        /// The collation.
        /// </value>
        public Collation Collation
        {
            get { return _collation; }
            set { _collation = value; }
        }

        /// <summary>
        /// Gets the collection namespace.
        /// </summary>
        /// <value>
        /// The collection namespace.
        /// </value>
        public CollectionNamespace CollectionNamespace => _collectionNamespace;

        /// <inheritdoc/>
        public BsonDocument DocumentResumeToken
        {
            get { return _documentResumeToken; }
            set { _documentResumeToken = value; }
        }

        /// <summary>
        /// Gets or sets the full document option.
        /// </summary>
        /// <value>
        /// The full document option.
        /// </value>
        public ChangeStreamFullDocumentOption FullDocument
        {
            get { return _fullDocument; }
            set { _fullDocument = value; }
        }

        /// <summary>
        /// Gets or sets the maximum await time.
        /// </summary>
        /// <value>
        /// The maximum await time.
        /// </value>
        public TimeSpan? MaxAwaitTime
        {
            get { return _maxAwaitTime; }
            set { _maxAwaitTime = value; }
        }

        /// <summary>
        /// Gets the pipeline.
        /// </summary>
        /// <value>
        /// The pipeline.
        /// </value>
        public IReadOnlyList<BsonDocument> Pipeline => _pipeline;

        /// <summary>
        /// Gets the result serializer.
        /// </summary>
        /// <value>
        /// The result serializer.
        /// </value>
        public IBsonSerializer<TResult> ResultSerializer => _resultSerializer;

        /// <inheritdoc />
        public BsonDocument ResumeAfter
        {
            get { return _resumeAfter; }
            set { _resumeAfter = value; }
        }

        /// <inheritdoc />
        public BsonDocument StartAfter
        {
            get { return _startAfter; }
            set { _startAfter = value; }
        }

        /// <inheritdoc />
        public BsonTimestamp StartAtOperationTime
        {
            get { return _startAtOperationTime; }
            set { _startAtOperationTime = value; }
        }

        // public methods
        /// <inheritdoc />
        public override IAsyncCursor<TResult> Execute(IReadBinding binding, CancellationToken cancellationToken)
        {
            var bindingHandle = binding as IReadBindingHandle;
            if (bindingHandle == null)
            {
                throw new ArgumentException("The binding value passed to ChangeStreamOperation.Execute must implement IReadBindingHandle.", nameof(binding));
            }

            using (var context = RetryableReadContext.Create(binding, RetryRequested, cancellationToken))
            {
                return Execute(context, cancellationToken);
            }

        }

        /// <inheritdoc />
        public override async Task<IAsyncCursor<TResult>> ExecuteAsync(IReadBinding binding, CancellationToken cancellationToken)
        {
            var bindingHandle = binding as IReadBindingHandle;
            if (bindingHandle == null)
            {
                throw new ArgumentException("The binding value passed to ChangeStreamOperation.Execute must implement IReadBindingHandle.", nameof(binding));
            }

            using (var context = RetryableReadContext.Create(binding, RetryRequested, cancellationToken))
            {
                return await ExecuteAsync(context, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        protected override BsonDocument CreateCommand(
            ICoreSessionHandle session, 
            ConnectionDescription connectionDescription, 
            int attempt,
            long? transactionNumber)
        {
            return CreateAggregateOperation(resuming: false).CreateCommand(connectionDescription, session);
        }

        /// <inheritdoc />
        public override IAsyncCursor<TResult> ExecuteAttempt(RetryableReadContext context, int attempt, long? transactionNumber,
            CancellationToken cancellationToken)
        {

            var binding = context.Binding;
            var bindingHandle = binding as IReadBindingHandle;
            if (bindingHandle == null)
            {
                throw new ArgumentException("RetryableReadContext's Binding must implement IReadBindingHandle.", nameof(context));
            }
            var session = binding.Session;
            var channelSource = context.ChannelSource;
            var server = channelSource.Server;
            var channel = context.Channel;
            var readPreference = context.Binding.ReadPreference;
            IAsyncCursor<RawBsonDocument> cursor;
            using (var channelBinding = new ChannelReadBinding(server, channel, readPreference, session.Fork()))
            {
                cursor = ExecuteAggregateOperation(channelBinding, resuming: false, cancellationToken);
                SaveInitialOperationTimeIfRequired(channel, channelBinding, cursor);
            }
            return new ChangeStreamCursor<TResult>(cursor, _resultSerializer, bindingHandle.Fork(), this);
        }

        /// <inheritdoc />
        public override async Task<IAsyncCursor<TResult>> ExecuteAttemptAsync(RetryableReadContext context, int attempt, long? transactionNumber,
            CancellationToken cancellationToken)
        {
            var binding = context.Binding;
            var bindingHandle = binding as IReadBindingHandle;
            if (bindingHandle == null)
            {
                throw new ArgumentException("RetryableReadContext's Binding must implement IReadBindingHandle.", nameof(context));
            }
            var session = binding.Session;
            var channelSource = context.ChannelSource;
            var server = channelSource.Server;
            var channel = context.Channel;
            var readPreference = context.Binding.ReadPreference;
            IAsyncCursor<RawBsonDocument> cursor;
            using (var channelBinding = new ChannelReadBinding(server, channel, readPreference, session.Fork()))
            {
                cursor = await ExecuteAggregateOperationAsync(channelBinding, resuming: false, cancellationToken).ConfigureAwait(false);
                SaveInitialOperationTimeIfRequired(channel, channelBinding, cursor);
            }
            return new ChangeStreamCursor<TResult>(cursor, _resultSerializer, bindingHandle.Fork(), this);
        }

        /// <inheritdoc />
        public IAsyncCursor<RawBsonDocument> Resume(IReadBinding binding, CancellationToken cancellationToken)
        {
            return ExecuteAggregateOperation(binding, resuming: true, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IAsyncCursor<RawBsonDocument>> ResumeAsync(IReadBinding binding, CancellationToken cancellationToken)
        {
            return ExecuteAggregateOperationAsync(binding, resuming: true, cancellationToken);
        }

        // private methods
        private AggregateOperation<RawBsonDocument> CreateAggregateOperation(bool resuming)
        {
            var changeStreamStage = CreateChangeStreamStage(resuming);
            var combinedPipeline = CreateCombinedPipeline(changeStreamStage);

            AggregateOperation<RawBsonDocument> operation;
            if (_collectionNamespace != null)
            {
                operation = new AggregateOperation<RawBsonDocument>(_collectionNamespace, combinedPipeline, RawBsonDocumentSerializer.Instance, MessageEncoderSettings);
            }
            else
            {
                var databaseNamespace = DatabaseNamespace ?? DatabaseNamespace.Admin;
                operation = new AggregateOperation<RawBsonDocument>(databaseNamespace, combinedPipeline, RawBsonDocumentSerializer.Instance, MessageEncoderSettings);
            }

            operation.BatchSize = _batchSize;
            operation.Collation = _collation;
            operation.MaxAwaitTime = _maxAwaitTime;
            operation.ReadConcern = ReadConcern;

            return operation;
        }

        private BsonDocument CreateChangeStreamStage(bool resuming)
        {
            var resumeStartValues = GetEffectiveResumeStartValues(resuming);

            var changeStreamOptions = new BsonDocument
            {
                { "fullDocument", ToString(_fullDocument) },
                { "allChangesForCluster", true, _allChangesForCluster },
                { "startAfter", resumeStartValues.StartAfter, resumeStartValues.StartAfter != null},
                { "startAtOperationTime", resumeStartValues.StartAtOperationTime, resumeStartValues.StartAtOperationTime != null },
                { "resumeAfter", resumeStartValues.ResumeAfter, resumeStartValues.ResumeAfter != null }
            };
            return new BsonDocument("$changeStream", changeStreamOptions);
        }

        private List<BsonDocument> CreateCombinedPipeline(BsonDocument changeStreamStage)
        {
            var combinedPipeline = new List<BsonDocument>();
            combinedPipeline.Add(changeStreamStage);
            combinedPipeline.AddRange(_pipeline);
            return combinedPipeline;
        }

        private IAsyncCursor<RawBsonDocument> ExecuteAggregateOperation(IReadBinding binding, bool resuming, CancellationToken cancellationToken)
        {
            var aggregateOperation = CreateAggregateOperation(resuming);
            return aggregateOperation.Execute(binding, cancellationToken);
        }

        private Task<IAsyncCursor<RawBsonDocument>> ExecuteAggregateOperationAsync(IReadBinding binding, bool resuming, CancellationToken cancellationToken)
        {
            var aggregateOperation = CreateAggregateOperation(resuming);
            return aggregateOperation.ExecuteAsync(binding, cancellationToken);
        }

        private ResumeStartValue GetEffectiveResumeStartValues(bool resuming)
        {
            if (resuming)
            {
                if (_documentResumeToken != null)
                {
                    return new ResumeStartValue { ResumeAfter = _documentResumeToken };
                }

                if (_startAfter != null)
                {
                    return new ResumeStartValue { ResumeAfter = _startAfter };
                }

                if (_resumeAfter != null)
                {
                    return new ResumeStartValue { ResumeAfter = _resumeAfter };
                }

                if (_startAtOperationTime != null || _initialOperationTime != null)
                {
                    return new ResumeStartValue { StartAtOperationTime = _startAtOperationTime ?? _initialOperationTime };
                }
            }

            return new ResumeStartValue { ResumeAfter = _resumeAfter, StartAfter = _startAfter, StartAtOperationTime = _startAtOperationTime };
        }

        private void SaveInitialOperationTimeIfRequired(IChannelHandle channel, IReadBinding binding, IAsyncCursor<RawBsonDocument> cursor)
        {
            if (_startAtOperationTime == null && _resumeAfter == null && _startAfter == null)
            {
                var maxWireVersion = channel.ConnectionDescription.IsMasterResult.MaxWireVersion;
                if (maxWireVersion >= 7)
                {
                    if (_documentResumeToken != null)
                    {
                        _initialOperationTime = binding.Session.OperationTime;
                    }
                }
            }
        }

        private string ToString(ChangeStreamFullDocumentOption fullDocument)
        {
            switch (fullDocument)
            {
                case ChangeStreamFullDocumentOption.Default: return "default";
                case ChangeStreamFullDocumentOption.UpdateLookup: return "updateLookup";
                default: throw new ArgumentException($"Invalid FullDocument option: {fullDocument}.", nameof(fullDocument));
            }
        }

        // nested types
        private struct ResumeStartValue
        {
            public BsonDocument ResumeAfter { get; set; }
            public BsonDocument StartAfter { get; set; }
            public BsonTimestamp StartAtOperationTime { get; set; }
        }
    }
}