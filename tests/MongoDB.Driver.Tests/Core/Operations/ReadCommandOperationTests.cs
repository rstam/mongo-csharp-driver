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

using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.TestHelpers.XunitExtensions;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Servers;
using MongoDB.Driver.Core.WireProtocol;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;
using Moq;
using Xunit;

namespace MongoDB.Driver.Core.Operations
{
    public class ReadCommandOperationTests : OperationTestBase
    {
        // public methods
        [Fact]
        public void constructor_should_initialize_instance()
        {
            var databaseNamespace = new DatabaseNamespace("databaseName");
            var command = new BsonDocument("command", 1);
            var resultSerializer = BsonDocumentSerializer.Instance;
            var messageEncoderSettings = new MessageEncoderSettings();

            var result = new ReadCommandOperation<BsonDocument>(databaseNamespace, command, resultSerializer, messageEncoderSettings);

            result.AdditionalOptions.Should().BeNull();
            result.Command.Should().BeSameAs(command);
            result.CommandValidator.Should().BeOfType<NoOpElementNameValidator>();
            result.Comment.Should().BeNull();
            result.DatabaseNamespace.Should().BeSameAs(databaseNamespace);
            result.MessageEncoderSettings.Should().BeSameAs(messageEncoderSettings);
            result.ResultSerializer.Should().BeSameAs(resultSerializer);
            result.RetryRequested.Should().BeFalse();
        }

        [Theory]
        [ParameterAttributeData]
        public void RetryRequested_get_and_set_should_work(
            [Values(false, true)] bool value)
        {
            var subject = CreateSubject<BsonDocument>();

            subject.RetryRequested = value;
            var result = subject.RetryRequested;

            result.Should().Be(value);
        }

        [Theory]
        [InlineData(ServerType.Standalone, ReadPreferenceMode.Primary, false)]
        [InlineData(ServerType.Standalone, ReadPreferenceMode.Primary, true)]
        [InlineData(ServerType.Standalone, ReadPreferenceMode.Secondary, false)]
        [InlineData(ServerType.Standalone, ReadPreferenceMode.Secondary, true)]
        [InlineData(ServerType.Standalone, ReadPreferenceMode.SecondaryPreferred, false)]
        [InlineData(ServerType.Standalone, ReadPreferenceMode.SecondaryPreferred, true)]
        [InlineData(ServerType.ShardRouter, ReadPreferenceMode.Primary, false)]
        [InlineData(ServerType.ShardRouter, ReadPreferenceMode.Primary, true)]
        [InlineData(ServerType.ShardRouter, ReadPreferenceMode.SecondaryPreferred, false)]
        [InlineData(ServerType.ShardRouter, ReadPreferenceMode.SecondaryPreferred, true)]
        public void Execute_should_call_channel_Command_with_unwrapped_command_when_wrapping_is_not_necessary(
            ServerType serverType,
            ReadPreferenceMode readPreferenceMode,
            bool async)
        {
            var subject = CreateSubject<BsonDocument>();
            var readPreference = new ReadPreference(readPreferenceMode);
            var serverDescription = CreateServerDescription(serverType);
            var mockChannel = CreateMockChannel();
            var channelSource = CreateMockChannelSource(serverDescription, mockChannel.Object).Object;
            var binding = CreateMockReadBinding(readPreference, channelSource).Object;

            ExecuteOperation(subject, binding, async);
            if (async)
            {
                mockChannel.Verify(
                    c => c.CommandAsync(
                        It.IsAny<OperationContext>(),
                        binding.Session,
                        readPreference,
                        subject.DatabaseNamespace,
                        subject.Command,
                        null, // commandPayloads
                        subject.CommandValidator,
                        null, // additionalOptions
                        null, // postWriteAction
                        CommandResponseHandling.Return,
                        subject.ResultSerializer,
                        subject.MessageEncoderSettings),
                    Times.Once);
            }
            else
            {
                mockChannel.Verify(
                    c => c.Command(
                        It.IsAny<OperationContext>(),
                        binding.Session,
                        readPreference,
                        subject.DatabaseNamespace,
                        subject.Command,
                        null, // commandPayloads
                        subject.CommandValidator,
                        null, // additionalOptions
                        null, // postWriteAction
                        CommandResponseHandling.Return,
                        subject.ResultSerializer,
                        subject.MessageEncoderSettings),
                Times.Once);
            }
        }

        [Theory]
        [ParameterAttributeData]
        public void Execute_should_call_channel_Command_with_wrapped_command_when_additionalOptions_need_wrapping(
            [Values(false, true)] bool async)
        {
            var subject = CreateSubject<BsonDocument>();
            subject.AdditionalOptions = new BsonDocument("additional", 1);
            subject.Comment = "comment";
            var readPreference = ReadPreference.Primary;
            var serverDescription = CreateServerDescription(ServerType.Standalone);
            var mockChannel = CreateMockChannel();
            var channelSource = CreateMockChannelSource(serverDescription, mockChannel.Object).Object;
            var binding = CreateMockReadBinding(readPreference, channelSource).Object;
            var additionalOptions = BsonDocument.Parse("{ $comment : \"comment\", additional : 1 }");

            ExecuteOperation(subject, binding, async);
            if (async)
            {
                mockChannel.Verify(
                    c => c.CommandAsync(
                        It.IsAny<OperationContext>(),
                        binding.Session,
                        readPreference,
                        subject.DatabaseNamespace,
                        subject.Command,
                        null, // commandPayloads
                        subject.CommandValidator,
                        additionalOptions,
                        null, // postWriteAction
                        CommandResponseHandling.Return,
                        subject.ResultSerializer,
                        subject.MessageEncoderSettings),
                    Times.Once);
            }
            else
            {
                mockChannel.Verify(
                    c => c.Command(
                        It.IsAny<OperationContext>(),
                        binding.Session,
                        readPreference,
                        subject.DatabaseNamespace,
                        subject.Command,
                        null, // commandPayloads
                        subject.CommandValidator,
                        additionalOptions,
                        null, // postWriteAction
                        CommandResponseHandling.Return,
                        subject.ResultSerializer,
                        subject.MessageEncoderSettings),
                    Times.Once);
            }
        }

        [Theory]
        [ParameterAttributeData]
        public void Execute_should_call_channel_Command_with_wrapped_command_when_comment_needs_wrapping(
            [Values(false, true)] bool async)
        {
            var subject = CreateSubject<BsonDocument>();
            subject.Comment = "comment";
            var readPreference = ReadPreference.Primary;
            var serverDescription = CreateServerDescription(ServerType.Standalone);
            var mockChannel = CreateMockChannel();
            var channelSource = CreateMockChannelSource(serverDescription, mockChannel.Object).Object;
            var binding = CreateMockReadBinding(readPreference, channelSource).Object;
            var additionalOptions = BsonDocument.Parse("{ $comment : \"comment\" }");

            ExecuteOperation(subject, binding, async);
            if (async)
            {
                mockChannel.Verify(
                    c => c.CommandAsync(
                        It.IsAny<OperationContext>(),
                        binding.Session,
                        readPreference,
                        subject.DatabaseNamespace,
                        subject.Command,
                        null, // commandPayloads
                        subject.CommandValidator,
                        additionalOptions,
                        null, // postWriteAction
                        CommandResponseHandling.Return,
                        subject.ResultSerializer,
                        subject.MessageEncoderSettings),
                    Times.Once);
            }
            else
            {
                mockChannel.Verify(
                    c => c.Command(
                        It.IsAny<OperationContext>(),
                        binding.Session,
                        readPreference,
                        subject.DatabaseNamespace,
                        subject.Command,
                        null, // commandPayloads
                        subject.CommandValidator,
                        additionalOptions,
                        null, // postWriteAction
                        CommandResponseHandling.Return,
                        subject.ResultSerializer,
                        subject.MessageEncoderSettings),
                    Times.Once);
            }
        }

        [Theory]
        [ParameterAttributeData]
        public void Execute_should_call_channel_Command_with_wrapped_command_when_readPreference_needs_wrapping(
            [Values(false, true)] bool async)
        {
            var subject = CreateSubject<BsonDocument>();
            subject.AdditionalOptions = new BsonDocument("additional", 1);
            subject.Comment = "comment";
            var readPreference = ReadPreference.Secondary;
            var serverDescription = CreateServerDescription(ServerType.ShardRouter);
            var mockChannel = CreateMockChannel();
            var channelSource = CreateMockChannelSource(serverDescription, mockChannel.Object).Object;
            var binding = CreateMockReadBinding(readPreference, channelSource).Object;
            var additionalOptions = BsonDocument.Parse("{ $comment : \"comment\", additional : 1 }");

            ExecuteOperation(subject, binding, async);
            if (async)
            {
                mockChannel.Verify(
                    c => c.CommandAsync(
                        It.IsAny<OperationContext>(),
                        binding.Session,
                        readPreference,
                        subject.DatabaseNamespace,
                        subject.Command,
                        null, // commandPayloads
                        subject.CommandValidator,
                        additionalOptions,
                        null, // postWriteAction
                        CommandResponseHandling.Return,
                        subject.ResultSerializer,
                        subject.MessageEncoderSettings),
                    Times.Once);
            }
            else
            {
                mockChannel.Verify(
                    c => c.Command(
                        It.IsAny<OperationContext>(),
                        binding.Session,
                        readPreference,
                        subject.DatabaseNamespace,
                        subject.Command,
                        null, // commandPayloads
                        subject.CommandValidator,
                        additionalOptions,
                        null, // postWriteAction
                        CommandResponseHandling.Return,
                        subject.ResultSerializer,
                        subject.MessageEncoderSettings),
                    Times.Once);
            }
        }

        [Theory]
        [ParameterAttributeData]
        public void Execute_should_call_GetChannel_only_once([Values(false, true)] bool async)
        {
            var subject = CreateSubject<BsonDocument>();
            var readPreference = ReadPreference.Primary;
            var serverDescription = CreateServerDescription(ServerType.Standalone);
            var mockChannel = CreateMockChannel();
            var mockChannelSource = CreateMockChannelSource(serverDescription, mockChannel.Object);
            var binding = CreateMockReadBinding(readPreference, mockChannelSource.Object).Object;

            ExecuteOperation(subject, binding, async);
            if (async)
            {
                mockChannelSource.Verify(c => c.GetChannelAsync(It.IsAny<OperationContext>()), Times.Once);
            }
            else
            {
                mockChannelSource.Verify(c => c.GetChannel(It.IsAny<OperationContext>()), Times.Once);
            }
        }

        // private methods
        private Mock<IReadBinding> CreateMockReadBinding(ReadPreference readPreference, IChannelSourceHandle channelSource)
        {
            var mockBinding = new Mock<IReadBinding>();
            var mockSession = new Mock<ICoreSessionHandle>();
            mockBinding.SetupGet(b => b.ReadPreference).Returns(readPreference);
            mockBinding.SetupGet(b => b.Session).Returns(mockSession.Object);
            mockBinding.Setup(b => b.GetReadChannelSource(It.IsAny<OperationContext>(), It.IsAny<IReadOnlyCollection<ServerDescription>>())).Returns(channelSource);
            mockBinding.Setup(b => b.GetReadChannelSourceAsync(It.IsAny<OperationContext>(), It.IsAny<IReadOnlyCollection<ServerDescription>>())).Returns(Task.FromResult(channelSource));
            return mockBinding;
        }

        private Mock<IChannelHandle> CreateMockChannel()
        {
            var mockChannel = new Mock<IChannelHandle>();
            return mockChannel;
        }

        private Mock<IChannelSourceHandle> CreateMockChannelSource(ServerDescription serverDescription, IChannelHandle channel)
        {
            var mockChannelSource = new Mock<IChannelSourceHandle>();
            mockChannelSource.SetupGet(s => s.ServerDescription).Returns(serverDescription);
            mockChannelSource.Setup(s => s.GetChannel(It.IsAny<OperationContext>())).Returns(channel);
            mockChannelSource.Setup(s => s.GetChannelAsync(It.IsAny<OperationContext>())).Returns(Task.FromResult(channel));
            return mockChannelSource;
        }

        private ServerDescription CreateServerDescription(ServerType serverType)
        {
            var clusterId = new ClusterId(1);
            var endPoint = new DnsEndPoint("localhost", 27017);
            var serverId = new ServerId(clusterId, endPoint);
            return new ServerDescription(serverId, endPoint, type: serverType);
        }

        private ReadCommandOperation<TCommandResult> CreateSubject<TCommandResult>(
            string databaseName = "databaseName",
            BsonDocument command = null,
            IBsonSerializer<TCommandResult> resultSerializer = null,
            MessageEncoderSettings messageEncoderSettings = null)
        {
            var databaseNamespace = new DatabaseNamespace(databaseName);
            command = command ?? new BsonDocument("command", 1);
            resultSerializer = resultSerializer ?? BsonSerializer.LookupSerializer<TCommandResult>();
            return new ReadCommandOperation<TCommandResult>(databaseNamespace, command, resultSerializer, messageEncoderSettings);
        }
    }
}
