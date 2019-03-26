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
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol;
using MongoDB.Driver.Core.WireProtocol.Messages;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents a base class for retryable read operation
    /// </summary>
    public abstract class RetryableReadCommandOperationBase<TResult> : IReadOperation<TResult>, IRetryableReadOperation<TResult>
    {
        private ReadConcern _readConcern = ReadConcern.Default;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="RetryableReadCommandOperationBase{TResult}" /> class.
        /// </summary>
        /// <param name="databaseNamespace">The database namespace.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        public RetryableReadCommandOperationBase(
            DatabaseNamespace databaseNamespace,
            MessageEncoderSettings messageEncoderSettings)
            : this (
                databaseNamespace: databaseNamespace,
                retryRequested: false,
                readConcern: ReadConcern.Default,
                messageEncoderSettings: messageEncoderSettings)
        {
            DatabaseNamespace = Ensure.IsNotNull(databaseNamespace, nameof(databaseNamespace));
            MessageEncoderSettings = Ensure.IsNotNull(messageEncoderSettings, nameof(messageEncoderSettings));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryableReadCommandOperationBase{TResult}" /> class.
        /// </summary>
        /// <param name="databaseNamespace">The database namespace.</param>
        /// <param name="retryRequested">Whether retry was requested.</param>
        /// <param name="readConcern">The read concern.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        public RetryableReadCommandOperationBase(
            DatabaseNamespace databaseNamespace,
            bool retryRequested,
            ReadConcern readConcern,
            MessageEncoderSettings messageEncoderSettings)
        {
            DatabaseNamespace = Ensure.IsNotNull(databaseNamespace, nameof(databaseNamespace));
            MessageEncoderSettings = Ensure.IsNotNull(messageEncoderSettings, nameof(messageEncoderSettings));
            ReadConcern = readConcern;
            RetryRequested = retryRequested;
        }


        // public properties
        /// <summary>
        /// Gets the database namespace.
        /// </summary>
        /// <value>
        /// The database namespace.
        /// </value>
        public DatabaseNamespace DatabaseNamespace { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the server should process the requests in order.
        /// </summary>
        /// <value>A value indicating whether the server should process the requests in order.</value>
        public bool IsOrdered { get; set; } = true;

        /// <summary>
        /// Gets the message encoder settings.
        /// </summary>
        /// <value>
        /// The message encoder settings.
        /// </value>
        public MessageEncoderSettings MessageEncoderSettings { get; }

        /// <summary>
        /// Gets or sets a value indicating whether retry is enabled for the operation.
        /// </summary>
        /// <value>A value indicating whether retry is enabled.</value>
        public bool RetryRequested { get; set; }

        /// <summary>
        /// Gets or sets the Read concern.
        /// </summary>
        /// <value>
        /// The Read concern.
        /// </value>
        public ReadConcern ReadConcern
        {
            get { return _readConcern;}
            set { _readConcern = Ensure.IsNotNull(value, nameof(value)); }
        }

        // public methods
        /// <inheritdoc />
        public virtual TResult Execute(IReadBinding binding, CancellationToken cancellationToken)
        {
            using (var context = RetryableReadContext.Create(binding, RetryRequested, cancellationToken))
            {
                return Execute(context, cancellationToken);
            }
        }

        /// <inheritdoc />
        public virtual TResult Execute(RetryableReadContext context, CancellationToken cancellationToken)
        {
            return RetryableReadOperationExecutor.Execute(this, context, cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<TResult> ExecuteAsync(IReadBinding binding, CancellationToken cancellationToken)
        {
            using (var context = await RetryableReadContext.CreateAsync(binding, RetryRequested, cancellationToken).ConfigureAwait(false))
            {
                return await ExecuteAsync(context, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public virtual Task<TResult> ExecuteAsync(RetryableReadContext context, CancellationToken cancellationToken)
        {
            return RetryableReadOperationExecutor.ExecuteAsync(this, context, cancellationToken);
        }

        /// <inheritdoc />
        public virtual TResult ExecuteAttempt(RetryableReadContext context, int attempt, long? transactionNumber, CancellationToken cancellationToken)
        { //TODO: lookatme
            var args = GetCommandArgs(context, attempt, transactionNumber);

            return ParseCommandResult(context, 
                context.Channel.Command<BsonDocument>(
                    session: context.ChannelSource.Session.Fork(),
                    readPreference: ReadPreference.Primary,
                    databaseNamespace: DatabaseNamespace,
                    command: args.Command,
                    commandPayloads: null,
                    commandValidator: NoOpElementNameValidator.Instance,
                    additionalOptions: null, 
                    postWriteAction: args.PostReadAction,
                    responseHandling: args.ResponseHandling,
                    resultSerializer: BsonDocumentSerializer.Instance,
                    messageEncoderSettings: args.MessageEncoderSettings,
                    cancellationToken: cancellationToken));
        }

        /// <inheritdoc />
        public virtual Task<TResult> ExecuteAttemptAsync(RetryableReadContext context, int attempt, long? transactionNumber, CancellationToken cancellationToken)
        {
            var args = GetCommandArgs(context, attempt, transactionNumber);

            return ParseCommandResultAsync(context, context.Channel.CommandAsync<BsonDocument>(
                session: context.ChannelSource.Session.Fork(),
                readPreference: ReadPreference.Primary,
                databaseNamespace: DatabaseNamespace,
                command: args.Command,
                commandPayloads: null,
                commandValidator: NoOpElementNameValidator.Instance,
                additionalOptions: null,
                postWriteAction: args.PostReadAction,
                responseHandling: args.ResponseHandling,
                resultSerializer: BsonDocumentSerializer.Instance,
                messageEncoderSettings: args.MessageEncoderSettings,
                cancellationToken: cancellationToken));
        }

        // protected methods
        // Todo: refactor this
        /// <summary>
        /// Creates the command.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="connectionDescription">The connection description.</param>
        /// <param name="attempt">The attempt.</param>
        /// <param name="transactionNumber">The transaction number.</param>
        /// <returns>
        /// A command.
        /// </returns>
        protected abstract BsonDocument CreateCommand(ICoreSessionHandle session, ConnectionDescription connectionDescription, int attempt, long? transactionNumber);

        // Todo: refactor this
        /// <summary>
        /// Parses the command result.
        /// </summary>
        /// <param name="context">The retryable read context.</param>
        /// <param name="commandResultTask">Task that contains the result of the command</param>
        /// <returns>The result.</returns>
        protected virtual Task<TResult> ParseCommandResultAsync(RetryableReadContext context, Task<BsonDocument> commandResultTask)
        {
            return (Task<TResult>)(object)commandResultTask;
        }
        
        // Todo: refactor this
        /// <summary>
        /// Parses the command result.
        /// </summary>
        /// <param name="context">The retryable read context.</param>
        /// <param name="commandResult">The result of the command</param>
        /// <returns>The result.</returns>
        protected virtual TResult ParseCommandResult(RetryableReadContext context, BsonDocument commandResult)
        {
            return (TResult)(object)commandResult;
        }
        


        // private methods
        private MessageEncoderSettings CreateMessageEncoderSettings(IChannelHandle channel)
        {
            var clone = MessageEncoderSettings.Clone();
            clone.Add(MessageEncoderSettingsName.MaxDocumentSize, channel.ConnectionDescription.MaxDocumentSize);
            clone.Add(MessageEncoderSettingsName.MaxMessageSize, channel.ConnectionDescription.MaxMessageSize);
            clone.Add(MessageEncoderSettingsName.MaxWireDocumentSize, channel.ConnectionDescription.MaxWireDocumentSize);
            return clone;
        }
        
        // TODO: Fill me out
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="attempt"></param>
        /// <param name="transactionNumber"></param>
        /// <returns></returns>
        protected CommandArgs GetCommandArgs(RetryableReadContext context, int attempt, long? transactionNumber)
        {
            return new CommandArgs(
                command: CreateCommand(context.Binding.Session, context.Channel.ConnectionDescription, attempt,
                    transactionNumber),
                postReadAction: GetPostReadAction(),
                responseHandling: GetResponseHandling(),
                messageEncoderSettings: CreateMessageEncoderSettings(context.Channel)
            );
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected Action<IMessageEncoderPostProcessor> GetPostReadAction()
        {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected CommandResponseHandling GetResponseHandling()
        {
            return CommandResponseHandling.Return;
        }

        // nested types
        //TODO: Fill out comments
        /// <summary>
        /// 
        /// </summary>
        protected class CommandArgs
        {

            /// <summary>
            /// 
            /// </summary>
            /// <param name="command"></param>
            /// <param name="postReadAction"></param>
            /// <param name="responseHandling"></param>
            /// <param name="messageEncoderSettings"></param>
            public CommandArgs(
                BsonDocument command,
                Action<IMessageEncoderPostProcessor> postReadAction,
                CommandResponseHandling responseHandling,
                MessageEncoderSettings messageEncoderSettings)
            {
                Command = command;
                PostReadAction = postReadAction;
                ResponseHandling = responseHandling;
                MessageEncoderSettings = messageEncoderSettings;
            }
            /// <summary>
            /// 
            /// </summary>
            public BsonDocument Command { get; }
            /// <summary>
            /// 
            /// </summary>
            public Action<IMessageEncoderPostProcessor> PostReadAction { get; }
            /// <summary>
            /// 
            /// </summary>
            public CommandResponseHandling ResponseHandling { get; }
            /// <summary>
            /// 
            /// </summary>
            public MessageEncoderSettings MessageEncoderSettings { get; }
        }
    }
}
