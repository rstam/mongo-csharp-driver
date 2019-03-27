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
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents extension methods for operations.
    /// </summary>
    public static class OperationExtensionMethods
    {
        /// <summary>
        /// Executes a read operation using a channel source.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="operation">The read operation.</param>
        /// <param name="channelSource">The channel source.</param>
        /// <param name="readPreference">The read preference.</param>
        /// <param name="session">The session.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The result of the operation.
        /// </returns>
        public static TResult Execute<TResult>(
            this IReadOperation<TResult> operation,
            IChannelSourceHandle channelSource,
            ReadPreference readPreference,
            ICoreSessionHandle session,
            CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(operation, nameof(operation));
            using (var readBinding = new ChannelSourceReadWriteBinding(channelSource.Fork(), readPreference, session.Fork()))
            {
                return operation.Execute(readBinding, cancellationToken);
            }
        }

        /// <summary>
        /// Executes a write operation using a channel source.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="operation">The write operation.</param>
        /// <param name="channelSource">The channel source.</param>
        /// <param name="session">The session.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The result of the operation.
        /// </returns>
        public static TResult Execute<TResult>(
            this IWriteOperation<TResult> operation,
            IChannelSourceHandle channelSource,
            ICoreSessionHandle session,
            CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(operation, nameof(operation));
            using (var writeBinding = new ChannelSourceReadWriteBinding(channelSource.Fork(), ReadPreference.Primary, session.Fork()))
            {
                return operation.Execute(writeBinding, cancellationToken);
            }
        }

        /// <summary>
        /// Executes a read operation using a channel source.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="operation">The read operation.</param>
        /// <param name="channelSource">The channel source.</param>
        /// <param name="readPreference">The read preference.</param>
        /// <param name="session">The session.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A Task whose result is the result of the operation.
        /// </returns>
        public static async Task<TResult> ExecuteAsync<TResult>(
            this IReadOperation<TResult> operation,
            IChannelSourceHandle channelSource,
            ReadPreference readPreference,
            ICoreSessionHandle session,
            CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(operation, nameof(operation));
            using (var readBinding = new ChannelSourceReadWriteBinding(channelSource.Fork(), readPreference, session.Fork()))
            {
                return await operation.ExecuteAsync(readBinding, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Executes a write operation using a channel source.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="operation">The write operation.</param>
        /// <param name="channelSource">The channel source.</param>
        /// <param name="session">The session.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A Task whose result is the result of the operation.
        /// </returns>
        public static async Task<TResult> ExecuteAsync<TResult>(
            this IWriteOperation<TResult> operation,
            IChannelSourceHandle channelSource,
            ICoreSessionHandle session,
            CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(operation, nameof(operation));
            using (var writeBinding = new ChannelSourceReadWriteBinding(channelSource.Fork(), ReadPreference.Primary, session.Fork()))
            {
                return await operation.ExecuteAsync(writeBinding, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Executes a read operation using a channel source.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="operation">The read operation.</param>
        /// <param name="context">The retryable read context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The result of the operation.
        /// </returns>
        public static TResult ExecuteWithChannelBinding<TResult>(
            this IReadOperation<TResult> operation,
            RetryableReadContext context,
            CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(operation, nameof(operation));

            var server = context.ChannelSource.Server;
            var channel = context.Channel;
            var readPreference = context.Binding.ReadPreference;
            var session = context.Binding.Session;

            using (var channelBinding = new ChannelReadBinding(server, channel.Fork(), readPreference, session.Fork()))
            {
                return operation.Execute(channelBinding, cancellationToken);
            }
        }

        /// <summary>
        /// Executes a read operation using a channel source.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="operation">The read operation.</param>
        /// <param name="context">The retryable read context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The result of the operation.
        /// </returns>
        public static async Task<TResult> ExecuteWithChannelBindingAsync<TResult>(
            this IReadOperation<TResult> operation,
            RetryableReadContext context,
            CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(operation, nameof(operation));

            var server = context.ChannelSource.Server;
            var channel = context.Channel;
            var readPreference = context.Binding.ReadPreference;
            var session = context.Binding.Session;

            using (var channelBinding = new ChannelReadBinding(server, channel.Fork(), readPreference, session.Fork()))
            {
                return await operation.ExecuteAsync(channelBinding, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>Executes the with retries if retryable.</summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="operation">The operation.</param>
        /// <param name="context">The context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result.</returns>
        public static TResult ExecuteWithRetriesIfRetryable<TResult>(
            this IReadOperation<TResult> operation,
            RetryableReadContext context,
            CancellationToken cancellationToken)
        {
            if (operation is IRetryableReadOperation<TResult> retryableOperation)
            {
                return retryableOperation.Execute(context, cancellationToken);
            }
            else
            {
                var binding = context.Binding;
                var channel = context.Channel;
                var channelSource = context.ChannelSource;
                using (var channelBinding = new ChannelReadBinding(channelSource.Server, channel.Fork(), binding.ReadPreference, binding.Session.Fork()))
                {
                    return operation.Execute(channelBinding, cancellationToken);
                }
            }
        }

        /// <summary>Executes the with retries if retryable.</summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="operation">The operation.</param>
        /// <param name="context">The context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result.</returns>
        public static async Task<TResult> ExecuteWithRetriesIfRetryableAsync<TResult>(
            this IReadOperation<TResult> operation,
            RetryableReadContext context,
            CancellationToken cancellationToken)
        {
            if (operation is IRetryableReadOperation<TResult> retryableOperation)
            {
                return await retryableOperation.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var binding = context.Binding;
                var channel = context.Channel;
                var channelSource = context.ChannelSource;
                using (var channelBinding = new ChannelReadBinding(channelSource.Server, channel.Fork(), binding.ReadPreference, binding.Session.Fork()))
                {
                    return await operation.ExecuteAsync(channelBinding, cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}
