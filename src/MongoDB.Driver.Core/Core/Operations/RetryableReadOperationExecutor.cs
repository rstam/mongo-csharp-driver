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
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Core.Operations
{
    internal static class RetryableReadOperationExecutor
    {
        delegate TResult ExecuteAttempt<out TResult>(int attempt);
        
        // public static methods
        public static TResult Execute<TResult>(IRetryableReadOperation<TResult> operation, IReadBinding binding, bool retryRequested, CancellationToken cancellationToken)
        {
            using (var context = RetryableReadContext.Create(binding, retryRequested, cancellationToken))
            {
                return Execute(operation, context, cancellationToken);
            }
        }

        public static TResult Execute<TResult>(IRetryableReadOperation<TResult> operation, RetryableReadContext context, CancellationToken cancellationToken)
        {
            var initialServerVersion = context.Channel.ConnectionDescription.ServerVersion;
            ExecuteAttempt<TResult> executeOperation = attempt => 
                operation.ExecuteAttempt(context, attempt, transactionNumber: null, cancellationToken: cancellationToken);
            var shouldNotRetry = !context.RetryRequested 
                                 || !AreRetryableReadsSupported(context.Channel.ConnectionDescription) 
                                 || context.Binding.Session.IsInTransaction; 
            if (shouldNotRetry) // TODO: rename variable, reorder flow (currently parallels retry-writes executor)   
            {
                return executeOperation(attempt: 1);
            }

            Exception originalException;
            try
            {
                return executeOperation(attempt: 1);

            }
            catch (Exception ex) when (RetryabilityHelper.IsRetryableReadException(ex))
            {
                originalException = ex;
            }

            try
            {
                context.ReplaceChannelSource(context.Binding.GetReadChannelSource(cancellationToken));
                context.ReplaceChannel(context.ChannelSource.GetChannel(cancellationToken));
            }
            catch
            {
                throw originalException;
            }

            if (context.Channel.ConnectionDescription.ServerVersion < initialServerVersion)
            {
                throw originalException;
            }
            
            if (!AreRetryableReadsSupported(context.Channel.ConnectionDescription))
            {
                throw originalException;
            }

            try
            {
                return executeOperation(attempt: 2);
            }
            catch (Exception ex) when (ShouldThrowOriginalException(ex))
            {
                throw originalException;
            }
        }

        public static async Task<TResult> ExecuteAsync<TResult>(IRetryableReadOperation<TResult> operation, IReadBinding binding, bool retryRequested, CancellationToken cancellationToken)
        {
            using (var context = await RetryableReadContext.CreateAsync(binding, retryRequested, cancellationToken).ConfigureAwait(false))
            {
                return await ExecuteAsync(operation, context, cancellationToken).ConfigureAwait(false);
            }
        }

        public static async Task<TResult> ExecuteAsync<TResult>(IRetryableReadOperation<TResult> operation, RetryableReadContext context, CancellationToken cancellationToken)
        {
            var initialServerVersion = context.Channel.ConnectionDescription.ServerVersion;
            Func<bool> isRetrySupported = () => AreRetryableReadsSupported(context.Channel.ConnectionDescription);
            ExecuteAttempt<Task<TResult>> executeOperationAsync = attempt => 
                operation.ExecuteAttemptAsync(context, attempt, transactionNumber: null, cancellationToken: cancellationToken);
            var shouldNotRetry = !context.RetryRequested || !isRetrySupported()|| context.Binding.Session.IsInTransaction; 
            
            if (shouldNotRetry)
            {
                return await executeOperationAsync(attempt: 1).ConfigureAwait(false);
            }

            Exception originalException;
            try
            {
                return await executeOperationAsync(attempt: 1).ConfigureAwait(false);

            }
            catch (Exception ex) when (RetryabilityHelper.IsRetryableReadException(ex))
            {
                originalException = ex;
            }

            try
            {
                context.ReplaceChannelSource(context.Binding.GetReadChannelSource(cancellationToken));
                context.ReplaceChannel(context.ChannelSource.GetChannel(cancellationToken));
            }
            catch
            {
                throw originalException;
            }
            
            if (context.Channel.ConnectionDescription.ServerVersion < initialServerVersion)
            {
                throw originalException;
            }

            if (!isRetrySupported())
            {
                throw originalException;
            }

            try
            {
                return await executeOperationAsync(attempt: 2).ConfigureAwait(false);
            }
            catch (Exception ex) when (ShouldThrowOriginalException(ex))
            {
                throw originalException;
            }
        }

        // privates static methods
        private static bool AreRetryableReadsSupported(ConnectionDescription connectionDescription)
        {
            return Feature.RetryableReads.IsSupported(connectionDescription.ServerVersion);
        }

        private static bool ShouldThrowOriginalException(Exception retryException)
        {
            return retryException is MongoException && !(retryException is MongoConnectionException);
        }
    }
}
