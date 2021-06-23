/* Copyright 2021-present MongoDB Inc.
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
using System.Linq;
using System.Reflection;
using System.Threading;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.TestHelpers;
using MongoDB.Bson.TestHelpers.XunitExtensions;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Operations;
using MongoDB.Driver.Core.TestHelpers.XunitExtensions;
using Xunit;

namespace MongoDB.Driver.Core.Tests
{
    public class LoadBalancedIntegrationTests : OperationTestBase
    {
        public LoadBalancedIntegrationTests()
        {
            _collectionNamespace = CollectionNamespace.FromFullName("db.coll");
        }

        [SkippableTheory]
        [ParameterAttributeData]
        public void Find_should_share_connection_with_cursor(
            [Values(false)] bool async)
        {
            RequireServer.Check().LoadBalancing(enabled: true);

            SetupData();

            var eventCapturer = new EventCapturer()
                .Capture<ConnectionPoolCheckedOutConnectionEvent>()
                .Capture<ConnectionPoolCheckingOutConnectionEvent>()
                .Capture<ConnectionPoolCheckedInConnectionEvent>()
                .Capture<ConnectionPoolCheckingInConnectionEvent>()
                .Capture<CommandSucceededEvent>();

            using (var cluster = CoreTestConfiguration.CreateCluster(builder => builder.Subscribe(eventCapturer)))
            {
                var readPreference = ReadPreference.Primary;

                var cancellationToken = CancellationToken.None;

                IAsyncCursor<BsonDocument> asyncCursor = null;

                eventCapturer.Clear();
                using (var coreSession = cluster.StartSession())
                {
                    GetCoreSessionReferenceCount(coreSession).Should().Be(1);

                    asyncCursor = AssertFindAndCreateCursor(cluster, readPreference, coreSession, eventCapturer, async, isFirstOperation: true, cancellationToken);
                }

                AssertCursor(asyncCursor, eventCapturer);
            }
        }

        [Theory]
        [ParameterAttributeData]
        public void Insert_and_find_should_share_the_same_connection_under_transaction(
            [Values(false)] bool async)
        {
            RequireServer.Check().LoadBalancing(enabled: true);

            SetupData(insertInitialData: false);

            var eventCapturer = new EventCapturer()
                .Capture<ConnectionPoolCheckedOutConnectionEvent>()
                .Capture<ConnectionPoolCheckingOutConnectionEvent>()
                .Capture<ConnectionPoolCheckedInConnectionEvent>()
                .Capture<ConnectionPoolCheckingInConnectionEvent>()
                .Capture<CommandSucceededEvent>();

            using (var cluster = CoreTestConfiguration.CreateCluster(builder => builder.Subscribe(eventCapturer)))
            {
                var cancellationToken = CancellationToken.None;
                var readPreference = ReadPreference.Primary;

                eventCapturer.Clear();

                IAsyncCursor<BsonDocument> asyncCursor = null;
                using (var coreSession = cluster.StartSession())
                {
                    coreSession.StartTransaction();

                    GetCoreSessionReferenceCount(coreSession).Should().Be(1);

                    // Insert 1
                    AssertInsertInTransaction(cluster, coreSession, eventCapturer, async, firstOperation: true, cancellationToken);

                    GetChannelReferenceCount(coreSession.CurrentTransaction._pinnedChannel()).Should().Be(1); // one active channel
                    GetCoreSessionReferenceCount(coreSession).Should().Be(1);

                    // Insert 2
                    AssertInsertInTransaction(cluster, coreSession, eventCapturer, async, firstOperation: false, cancellationToken);

                    GetChannelReferenceCount(coreSession.CurrentTransaction._pinnedChannel()).Should().Be(1); // one active channel
                    GetCoreSessionReferenceCount(coreSession).Should().Be(1);

                    //// Insert 3 (works)
                    //AssertInsertInTransaction(cluster, coreSession, eventCapturer, async, firstOperation: false, cancellationToken);

                    //GetChannelReferenceCount(coreSession.CurrentTransaction._pinnedChannel()).Should().Be(1); // one active channel
                    //GetCoreSessionReferenceCount(coreSession).Should().Be(1);

                    asyncCursor = AssertFindAndCreateCursor(cluster, readPreference, coreSession, eventCapturer, async, isFirstOperation: false, cancellationToken);

                    // TODO: channel is disposed, so channelSource.GetChannel will throw
                    asyncCursor._channelSource_channel().Connection._disposed().Should().BeFalse();

                    GetChannelReferenceCount(coreSession.CurrentTransaction._pinnedChannel()).Should().Be(1); // one active channel
                    GetCoreSessionReferenceCount(coreSession).Should().Be(1);

                    asyncCursor = AssertFindAndCreateCursor(cluster, readPreference, coreSession, eventCapturer, async, isFirstOperation: false, cancellationToken);

                    // TODO: channel is disposed, so channelSource.GetChannel will throw
                    asyncCursor._channelSource_channel().Connection._disposed().Should().BeFalse();

                    GetChannelReferenceCount(coreSession.CurrentTransaction._pinnedChannel()).Should().Be(1); // one active channel
                    GetCoreSessionReferenceCount(coreSession).Should().Be(1);

                    AssertCursor(asyncCursor, eventCapturer);
                }
            }
        }

        // private methods
        private void AssertCheckedOutOnly(EventCapturer eventCapturer)
        {
            eventCapturer.Next().Should().BeOfType<ConnectionPoolCheckingOutConnectionEvent>();
            eventCapturer.Next().Should().BeOfType<CommandSucceededEvent>().Subject.CommandName.Should().Be("isMaster");
            eventCapturer.Next().Should().BeOfType<CommandSucceededEvent>().Subject.CommandName.Should().Be("buildInfo");
            eventCapturer.Next().Should().BeOfType<ConnectionPoolCheckedOutConnectionEvent>();
            eventCapturer.Any().Should().BeFalse();
        }

        private void AssertInsertInTransaction(ICluster cluster, ICoreSessionHandle coreSession, EventCapturer eventCapturer, bool async, bool firstOperation, CancellationToken cancellationToken)
        {
            using (var binding = ChannelPinningHelper.CreateEffectiveReadWriteBindings(cluster, coreSession))
            using (var bindingHandle = new ReadWriteBindingHandle(binding))
            {
                GetCoreSessionReferenceCount(bindingHandle.Session).Should().Be(2);
                using (var context = RetryableWriteContext.Create(binding, retryRequested: false, cancellationToken))
                {
                    if (firstOperation) // this is expected
                    {
                        AssertCheckedOutOnly(eventCapturer);
                    }
                    else
                    {
                        eventCapturer.Any().Should().BeFalse();
                    }

                    GetChannelSourceReferenceCount(context.ChannelSource).Should().Be(1); // one channel source is created
                    if (firstOperation)
                    {
                        GetChannelReferenceCount(context.Channel).Should().Be(1);
                    }
                    else
                    {
                        // + 2 to the firstOperation because get channelSource and get channel increase reference index in ChannelReadWriteBinding
                        GetChannelReferenceCount(context.Channel).Should().Be(3);
                    }
                    GetCoreSessionReferenceCount(context.Binding.Session).Should().Be(3);

                    context.PinConnectionIfRequired();

                    if (firstOperation)
                    {
                        //2 => +1 to original to protect channel from disposing
                        GetChannelReferenceCount(coreSession.CurrentTransaction._pinnedChannel()).Should().Be(2);
                    }
                    else
                    {
                        GetChannelReferenceCount(coreSession.CurrentTransaction._pinnedChannel()).Should().Be(3);
                    }

                    eventCapturer.Any().Should().BeFalse();

                    GetChannelSourceReferenceCount(context.ChannelSource).Should().Be(1); // still one active channel source
                    GetCoreSessionReferenceCount(context.Binding.Session).Should().Be(3);
                    if (firstOperation)
                    {
                        GetChannelReferenceCount(context.Channel).Should().Be(2); //2 => +1 to original to protect channel from disposing
                    }
                    else
                    {
                        GetChannelReferenceCount(context.Channel).Should().Be(3);
                    }

                    // Operation
                    CreateAndRunInsertOperation(context, async, new BsonDocument());
                    // Operation_ended

                    AssertCommandOnly(eventCapturer, "insert");

                    GetChannelSourceReferenceCount(context.ChannelSource).Should().Be(1); // still one active channel source
                    if (firstOperation)
                    {
                        GetChannelReferenceCount(context.Channel).Should().Be(2); //2 => +1 to original to protect channel from disposing
                    }
                    else
                    {
                        GetChannelReferenceCount(context.Channel).Should().Be(3);
                    }
                    GetCoreSessionReferenceCount(context.Binding.Session).Should().Be(3);
                }
                eventCapturer.Any().Should().BeFalse();
                GetCoreSessionReferenceCount(binding.Session).Should().Be(2);
            }
        }

        private IAsyncCursor<BsonDocument> AssertFindAndCreateCursor(ICluster cluster, ReadPreference readPreference, ICoreSessionHandle coreSession, EventCapturer eventCapturer, bool async, bool isFirstOperation, CancellationToken cancellationToken)
        {
            IAsyncCursor<BsonDocument> asyncCursor;
            using (var binding = new ReadBindingHandle(ChannelPinningHelper.CreateEffectiveReadBindings(cluster, coreSession, readPreference)))
            {
                GetCoreSessionReferenceCount(binding.Session).Should().Be(2);
                using (var context = RetryableReadContext.Create(binding, retryRequested: false, cancellationToken))
                {
                    if (isFirstOperation)  // this is expected
                    {
                        AssertCheckedOutOnly(eventCapturer);
                    }
                    else
                    {
                        eventCapturer.Any().Should().BeFalse();
                    }

                    GetChannelSourceReferenceCount(context.ChannelSource).Should().Be(1); // one channel source is created
                    GetCoreSessionReferenceCount(context.Binding.Session).Should().Be(3);
                    if (isFirstOperation)
                    {
                        GetChannelReferenceCount(context.Channel).Should().Be(1); // one channel is created
                    }
                    else
                    {
                        GetChannelReferenceCount(context.Channel).Should().Be(3);
                    }

                    context.PinConnectionIfRequired();
                    eventCapturer.Any().Should().BeFalse();  // no new events

                    GetChannelSourceReferenceCount(context.ChannelSource).Should().Be(1); // still one active channel source
                    GetCoreSessionReferenceCount(context.Binding.Session).Should().Be(3);
                    if (isFirstOperation)
                    {
                        GetChannelReferenceCount(context.Channel).Should().Be(1); // still one active channel
                    }
                    else
                    {
                        GetChannelReferenceCount(context.Channel).Should().Be(3);
                    }

                    // Operation
                    asyncCursor = CreateAndRunFindOperation(context, async);
                    // Operation_ended

                    eventCapturer.Next().Should().BeOfType<CommandSucceededEvent>().Subject.CommandName.Should().Be("find");
                    eventCapturer.Any().Should().BeFalse();

                    GetChannelSourceReferenceCount(context.ChannelSource).Should().Be(1); // still one active channel source
                    if (isFirstOperation)
                    {
                        GetChannelReferenceCount(context.Channel).Should().Be(2); // original one + cursor one
                        GetChannelReferenceCountForCursor(asyncCursor).Should().Be(2); // the same as above
                        GetCoreSessionReferenceCount(context.Binding.Session).Should().Be(4); // + 1 for cursor
                    }
                    else
                    {
                        GetChannelReferenceCount(context.Channel).Should().Be(3); // original one + cursor one
                        GetChannelReferenceCountForCursor(asyncCursor).Should().Be(3); // the same as above
                        GetCoreSessionReferenceCount(context.Binding.Session).Should().Be(3); // + 1 is not added due to workaround
                    }
                }

                eventCapturer.Any().Should().BeFalse();
                if (isFirstOperation)
                {
                    GetChannelReferenceCountForCursor(asyncCursor).Should().Be(1);
                    GetCoreSessionReferenceCount(binding.Session).Should().Be(3);
                }
                else
                {
                    GetChannelReferenceCountForCursor(asyncCursor).Should().Be(2);
                    GetCoreSessionReferenceCount(binding.Session).Should().Be(2);
                }
            }

            GetChannelReferenceCountForCursor(asyncCursor).Should().Be(1);
            if (isFirstOperation)
            {
                GetCoreSessionReferenceCount(coreSession).Should().Be(2);   // 2 to protect the session inside cursor again disposing after main operation ending
            }
            else
            {
                GetCoreSessionReferenceCount(coreSession).Should().Be(1);   // workaround
            }
            eventCapturer.Any().Should().BeFalse();

            return asyncCursor;
        }

        private void AssertCommandOnly(EventCapturer eventCapturer, string commandName)
        {
            eventCapturer.Next().Should().BeOfType<CommandSucceededEvent>().Subject.CommandName.Should().Be(commandName);
            eventCapturer.Any().Should().BeFalse();
        }

        private void AssertCursor(IAsyncCursor<BsonDocument> asyncCursor, EventCapturer eventCapturer)
        {
            asyncCursor.MoveNext();
            GetChannelReferenceCountForCursor(asyncCursor).Should().Be(1); // nothing happens since _firstBatch is already received
            eventCapturer.Any().Should().BeFalse();

            asyncCursor.MoveNext();
            GetChannelReferenceCountForCursor(asyncCursor).Should().Be(1);
            eventCapturer.Next().Should().BeOfType<CommandSucceededEvent>().Subject.CommandName.Should().Be("getMore");
            eventCapturer.Any().Should().BeFalse();
            asyncCursor.Dispose();
            eventCapturer.Next().Should().BeOfType<CommandSucceededEvent>().Subject.CommandName.Should().Be("killCursors");
            eventCapturer.Next().Should().BeOfType<ConnectionPoolCheckingInConnectionEvent>();
            eventCapturer.Next().Should().BeOfType<ConnectionPoolCheckedInConnectionEvent>();
            eventCapturer.Any().Should().BeFalse();
            GetChannelReferenceCountForCursor(asyncCursor).Should().Be(0);
        }

        private void SetupData(bool insertInitialData = true)
        {
            // use the base test cluster
            DropCollection();
            if (insertInitialData)
            {
                Insert(new BsonDocument(), new BsonDocument());
            }
        }

        private void CreateAndRunInsertOperation(RetryableWriteContext context, bool async, params BsonDocument[] documents)
        {
            var operation = new BulkInsertOperation(
                _collectionNamespace,
                documents.Select(d => new InsertRequest(d)),
                _messageEncoderSettings);

            if (async)
            {
                operation.ExecuteAsync(context, CancellationToken.None).GetAwaiter().GetResult();
            }
            else
            {
                operation.Execute(context, CancellationToken.None);
            }
        }

        private IAsyncCursor<BsonDocument> CreateAndRunFindOperation(RetryableReadContext context, bool async)
        {
            var findOperation = new FindCommandOperation<BsonDocument>(
                _collectionNamespace,
                BsonDocumentSerializer.Instance,
                _messageEncoderSettings)
            {
                BatchSize = 1
            };

            if (async)
            {
                return findOperation.ExecuteAsync(context, CancellationToken.None).GetAwaiter().GetResult();
            }
            else
            {
                return findOperation.Execute(context, CancellationToken.None);
            }
        }

        // Reflection methods
        private int GetCoreSessionReferenceCount(ICoreSessionHandle coreSession) => ((ReferenceCountedCoreSession)((CoreSessionHandle)coreSession).Wrapped)._referenceCount();
        private int GetChannelReferenceCountForCursor(IAsyncCursor<BsonDocument> cursor)
        {
            IChannelHandle channel;
            try
            {
                channel = cursor._channelSource_channel();
            }
            catch (NullReferenceException)
            {
                // check difference place
                channel = cursor._channelSource_reference_instance_channel();
            }

            try
            {
                return channel._connection_reference_ReferenceCount();
            }
            catch (NullReferenceException)
            {
                return channel.Connection._reference_ReferenceCount();
            }
        }
        private int GetChannelReferenceCount(IChannelHandle channel) => channel._connection_reference_ReferenceCount();
        private int GetChannelSourceReferenceCount(IChannelSourceHandle channelSource) => channelSource._reference_ReferenceCount();
    }

    internal static class LoadBalancedReflector
    {
        public static int _connection_reference_ReferenceCount(this IChannelHandle channelHandle) // can be taken from private types
        {
            var connection = Reflector.GetFieldValue(channelHandle, "_connection");
            var reference = Reflector.GetFieldValue(connection, "_reference");
            return (int)Reflector.GetPropertyValue(reference, "ReferenceCount", BindingFlags.Public | BindingFlags.Instance);
        }

        public static int _referenceCount(this ReferenceCountedCoreSession referenceCountedCoreSession)
        {
            return (int)Reflector.GetFieldValue(referenceCountedCoreSession, nameof(_referenceCount));
        }

        public static int _reference_ReferenceCount(this IChannelSourceHandle channelSourceHandle)
        {
            var reference = Reflector.GetFieldValue(channelSourceHandle, "_reference");
            return (int)Reflector.GetPropertyValue(reference, "ReferenceCount", BindingFlags.Public | BindingFlags.Instance);
        }

        public static IChannelHandle _channelSource_channel(this IAsyncCursor<BsonDocument> cursor)
        {
            var source = Reflector.GetFieldValue(cursor, "_channelSource");
            return (IChannelHandle)Reflector.GetFieldValue(source, "_channel");
        }

        public static IChannelHandle _channelSource_reference_instance_channel(this IAsyncCursor<BsonDocument> cursor)
        {
            var source = Reflector.GetFieldValue(cursor, "_channelSource");
            var reference = Reflector.GetFieldValue(source, "_reference");
            var instance = Reflector.GetFieldValue(reference, "_instance");
            return (IChannelHandle)Reflector.GetFieldValue(instance, "_channel");
        }

        public static IChannelHandle _pinnedChannel(this CoreTransaction coreTransaction)
        {
            return (IChannelHandle)Reflector.GetFieldValue(coreTransaction, nameof(_pinnedChannel));
        }

        public static int _reference_ReferenceCount(this IConnectionHandle connectionHandle) // can be taken from private types
        {
            var reference = Reflector.GetFieldValue(connectionHandle, "_reference");
            return (int)Reflector.GetPropertyValue(reference, "ReferenceCount", BindingFlags.Public | BindingFlags.Instance);
        }

        public static bool _disposed(this IConnectionHandle connectionHandle)
        {
            return (bool)Reflector.GetFieldValue(connectionHandle, nameof(_disposed));
        }
    }
}
