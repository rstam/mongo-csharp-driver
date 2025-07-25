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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Operations;
using MongoDB.Driver.Core.Servers;
using MongoDB.Driver.Core.WireProtocol.Messages;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders.BinaryEncoders;

namespace MongoDB.Driver.Core.WireProtocol
{
    internal sealed class CommandUsingQueryMessageWireProtocol<TCommandResult> : IWireProtocol<TCommandResult>
    {
        // fields
        private readonly BsonDocument _additionalOptions;
        private readonly BsonDocument _command;
        private readonly List<BatchableCommandMessageSection> _commandPayloads;
        private readonly IElementNameValidator _commandValidator;
        private readonly DatabaseNamespace _databaseNamespace;
        private readonly Action<IMessageEncoderPostProcessor> _postWriteAction;
        private readonly MessageEncoderSettings _messageEncoderSettings;
        private readonly ReadPreference _readPreference;
        private readonly CommandResponseHandling _responseHandling;
        private readonly IBsonSerializer<TCommandResult> _resultSerializer;
        private readonly ServerApi _serverApi;
        private readonly ICoreSession _session;

        // constructors
        public CommandUsingQueryMessageWireProtocol(
            ICoreSession session,
            ReadPreference readPreference,
            DatabaseNamespace databaseNamespace,
            BsonDocument command,
            IEnumerable<BatchableCommandMessageSection> commandPayloads,
            IElementNameValidator commandValidator,
            BsonDocument additionalOptions,
            CommandResponseHandling responseHandling,
            IBsonSerializer<TCommandResult> resultSerializer,
            MessageEncoderSettings messageEncoderSettings,
            Action<IMessageEncoderPostProcessor> postWriteAction,
            ServerApi serverApi)
        {
            if (responseHandling != CommandResponseHandling.Return && responseHandling != CommandResponseHandling.Ignore)
            {
                throw new ArgumentException("CommandResponseHandling must be Return or Ignore.", nameof(responseHandling));
            }

            _session = Ensure.IsNotNull(session, nameof(session));
            _readPreference = readPreference;
            _databaseNamespace = Ensure.IsNotNull(databaseNamespace, nameof(databaseNamespace));
            _command = Ensure.IsNotNull(command, nameof(command));
            _commandPayloads = commandPayloads?.ToList(); // can be null
            _commandValidator = Ensure.IsNotNull(commandValidator, nameof(commandValidator));
            _additionalOptions = additionalOptions; // can be null
            _responseHandling = responseHandling;
            _resultSerializer = Ensure.IsNotNull(resultSerializer, nameof(resultSerializer));
            _messageEncoderSettings = messageEncoderSettings;
            _postWriteAction = postWriteAction; // can be null
            _serverApi = serverApi; // can be null
        }

        // public properties
        public bool MoreToCome => false;

        // methods
        private QueryMessage CreateMessage(ConnectionDescription connectionDescription, out bool messageContainsSessionId)
        {
            var commandWithPayloads = CombineCommandWithPayloads(connectionDescription);
            var wrappedCommand = WrapCommandForQueryMessage(commandWithPayloads, connectionDescription, out messageContainsSessionId, out var secondaryOk);

#pragma warning disable 618
            return new QueryMessage(
                RequestMessage.GetNextRequestId(),
                _databaseNamespace.CommandCollection,
                wrappedCommand,
                null,
                _commandValidator,
                0,
                -1,
                secondaryOk,
                false,
                false,
                false,
                false,
                false)
            {
                PostWriteAction = _postWriteAction,
                ResponseHandling = _responseHandling
            };
#pragma warning restore 618
        }

        public TCommandResult Execute(OperationContext operationContext, IConnection connection)
        {
            bool messageContainsSessionId;
            var message = CreateMessage(connection.Description, out messageContainsSessionId);
            connection.SendMessage(operationContext, message, _messageEncoderSettings);
            if (messageContainsSessionId)
            {
                _session.WasUsed();
            }

            switch (message.ResponseHandling)
            {
                case CommandResponseHandling.Ignore:
                    IgnoreResponse(operationContext, connection, message);
                    return default(TCommandResult);
                default:
                    var encoderSelector = new ReplyMessageEncoderSelector<RawBsonDocument>(RawBsonDocumentSerializer.Instance);
                    var reply = connection.ReceiveMessage(operationContext, message.RequestId, encoderSelector, _messageEncoderSettings);
                    return ProcessReply(connection.ConnectionId, (ReplyMessage<RawBsonDocument>)reply);
            }
        }

        public async Task<TCommandResult> ExecuteAsync(OperationContext operationContext, IConnection connection)
        {
            bool messageContainsSessionId;
            var message = CreateMessage(connection.Description, out messageContainsSessionId);
            await connection.SendMessageAsync(operationContext, message, _messageEncoderSettings).ConfigureAwait(false);
            if (messageContainsSessionId)
            {
                _session.WasUsed();
            }

            switch (message.ResponseHandling)
            {
                case CommandResponseHandling.Ignore:
                    IgnoreResponse(operationContext, connection, message);
                    return default(TCommandResult);
                default:
                    var encoderSelector = new ReplyMessageEncoderSelector<RawBsonDocument>(RawBsonDocumentSerializer.Instance);
                    var reply = await connection.ReceiveMessageAsync(operationContext, message.RequestId, encoderSelector, _messageEncoderSettings).ConfigureAwait(false);
                    return ProcessReply(connection.ConnectionId, (ReplyMessage<RawBsonDocument>)reply);
            }
        }

        // private methods
        private BsonDocument CombineCommandWithPayloads(ConnectionDescription connectionDescription)
        {
            if (_commandPayloads == null || _commandPayloads.Count == 0)
            {
                return _command;
            }

            var extraElements = new List<BsonElement>();
            foreach (var payloadSection in _commandPayloads)
            {
                if (payloadSection is not Type1CommandMessageSection type1Section)
                {
                    throw new NotSupportedException("Query protocol supports only Type1CommandMessageSection payload sections.");
                }

                var name = type1Section.Identifier;
                var value = CreatePayloadArray(type1Section, connectionDescription);
                var element = new BsonElement(name, value);
                extraElements.Add(element);
            }

            var payloadAppendingSerializer = new ElementAppendingSerializer<BsonDocument>(BsonDocumentSerializer.Instance, extraElements);
            return new BsonDocumentWrapper(_command, payloadAppendingSerializer);
        }

        private BsonArray CreatePayloadArray(Type1CommandMessageSection payload, ConnectionDescription connectionDescription)
        {
            IBsonSerializer payloadSerializer;
            if (payload.Documents.CanBeSplit)
            {
                payloadSerializer = CreateSizeLimitingPayloadSerializer(payload, connectionDescription);
            }
            else
            {
                payloadSerializer = CreateFixedCountPayloadSerializer(payload);
            }

            var documents = new BsonDocumentWrapper(payload.Documents, payloadSerializer);
            return new BsonArray { documents };
        }

        private IBsonSerializer CreateFixedCountPayloadSerializer(Type1CommandMessageSection payload)
        {
            var documentType = payload.DocumentType;
            var serializerType = typeof(FixedCountBatchableSourceSerializer<>).MakeGenericType(documentType);
            var itemSerializerType = typeof(IBsonSerializer<>).MakeGenericType(documentType);
            var constructorParameterTypes = new[] { itemSerializerType, typeof(IElementNameValidator), typeof(int) };
            var constructorInfo = serializerType.GetTypeInfo().GetConstructor(constructorParameterTypes);
            var itemSerializer = payload.DocumentSerializer;
            var itemElementNameValidator = payload.ElementNameValidator;
            var count = payload.Documents.Count;
            return (IBsonSerializer)constructorInfo.Invoke(new object[] { itemSerializer, itemElementNameValidator, count });
        }

        private IBsonSerializer CreateSizeLimitingPayloadSerializer(Type1CommandMessageSection payload, ConnectionDescription connectionDescription)
        {
            var documentType = payload.DocumentType;
            var serializerType = typeof(SizeLimitingBatchableSourceSerializer<>).MakeGenericType(documentType);
            var itemSerializerType = typeof(IBsonSerializer<>).MakeGenericType(documentType);
            var constructorParameterTypes = new[] { itemSerializerType, typeof(IElementNameValidator), typeof(int), typeof(int), typeof(int) };
            var constructorInfo = serializerType.GetTypeInfo().GetConstructor(constructorParameterTypes);
            var itemSerializer = payload.DocumentSerializer;
            var itemElementNameValidator = payload.ElementNameValidator;
            var maxBatchCount = Math.Min(payload.MaxBatchCount ?? int.MaxValue, connectionDescription.MaxBatchCount);
            var maxItemSize = payload.MaxDocumentSize ?? connectionDescription.MaxDocumentSize;
            var maxBatchSize = connectionDescription.MaxDocumentSize;
            return (IBsonSerializer)constructorInfo.Invoke(new object[] { itemSerializer, itemElementNameValidator, maxBatchCount, maxItemSize, maxBatchSize });
        }

        private void IgnoreResponse(OperationContext operationContext, IConnection connection, QueryMessage message)
        {
            var encoderSelector = new ReplyMessageEncoderSelector<IgnoredReply>(IgnoredReplySerializer.Instance);
            connection.ReceiveMessageAsync(operationContext, message.RequestId, encoderSelector, _messageEncoderSettings).IgnoreExceptions();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        private TCommandResult ProcessReply(ConnectionId connectionId, ReplyMessage<RawBsonDocument> reply)
        {
            if (reply.NumberReturned == 0)
            {
                throw new MongoCommandException(connectionId, "Command returned no documents.", _command);
            }
            if (reply.NumberReturned > 1)
            {
                throw new MongoCommandException(connectionId, "Command returned multiple documents.", _command);
            }
            if (reply.QueryFailure)
            {
                var failureDocument = reply.QueryFailureDocument;
                throw ExceptionMapper.Map(connectionId, failureDocument) ?? new MongoCommandException(connectionId, "Command failed.", _command, failureDocument);
            }

            using (var rawDocument = reply.Documents[0])
            {
                var binaryReaderSettings = new BsonBinaryReaderSettings();
                if (_messageEncoderSettings != null)
                {
                    binaryReaderSettings.Encoding = _messageEncoderSettings.GetOrDefault<UTF8Encoding>(MessageEncoderSettingsName.ReadEncoding, Utf8Encodings.Strict);
                };

                BsonValue clusterTime;
                if (rawDocument.TryGetValue("$clusterTime", out clusterTime))
                {
                    // note: we are assuming that _session is an instance of ClusterClockAdvancingClusterTime
                    // and that calling _session.AdvanceClusterTime will have the side effect of advancing the cluster's ClusterTime also
                    var materializedClusterTime = ((RawBsonDocument)clusterTime).Materialize(binaryReaderSettings);
                    _session.AdvanceClusterTime(materializedClusterTime);
                }
                BsonValue operationTime;
                if (rawDocument.TryGetValue("operationTime", out operationTime))
                {
                    _session.AdvanceOperationTime(operationTime.AsBsonTimestamp);
                }

                if (!rawDocument.GetValue("ok", false).ToBoolean())
                {
                    var materializedDocument = rawDocument.Materialize(binaryReaderSettings);

                    var commandName = _command.GetElement(0).Name;
                    if (commandName == "$query")
                    {
                        commandName = _command["$query"].AsBsonDocument.GetElement(0).Name;
                    }

                    var notPrimaryOrNodeIsRecoveringException = ExceptionMapper.MapNotPrimaryOrNodeIsRecovering(connectionId, _command, materializedDocument, "errmsg");
                    if (notPrimaryOrNodeIsRecoveringException != null)
                    {
                        throw notPrimaryOrNodeIsRecoveringException;
                    }

                    string message;
                    BsonValue errmsgBsonValue;
                    if (materializedDocument.TryGetValue("errmsg", out errmsgBsonValue) && errmsgBsonValue.IsString)
                    {
                        var errmsg = errmsgBsonValue.ToString();
                        message = string.Format("Command {0} failed: {1}.", commandName, errmsg);
                    }
                    else
                    {
                        message = string.Format("Command {0} failed.", commandName);
                    }

                    var mappedException = ExceptionMapper.Map(connectionId, materializedDocument);
                    if (mappedException != null)
                    {
                        throw mappedException;
                    }

                    throw new MongoCommandException(connectionId, message, _command, materializedDocument);
                }

                if (rawDocument.Contains("writeConcernError"))
                {
                    var materializedDocument = rawDocument.Materialize(binaryReaderSettings);
                    var writeConcernError = materializedDocument["writeConcernError"].AsBsonDocument;
                    var message = writeConcernError.AsBsonDocument.GetValue("errmsg", null)?.AsString;
                    var writeConcernResult = new WriteConcernResult(materializedDocument);
                    throw new MongoWriteConcernException(connectionId, message, writeConcernResult);
                }

                using (var stream = new ByteBufferStream(rawDocument.Slice, ownsBuffer: false))
                {
                    var encoderFactory = new BinaryMessageEncoderFactory(stream, _messageEncoderSettings);
                    var encoder = (ReplyMessageBinaryEncoder<TCommandResult>)encoderFactory.GetReplyMessageEncoder<TCommandResult>(_resultSerializer);
                    using (var reader = encoder.CreateBinaryReader())
                    {
                        var context = BsonDeserializationContext.CreateRoot(reader);
                        return _resultSerializer.Deserialize(context);
                    }
                }
            }
        }

        private BsonDocument WrapCommandForQueryMessage(BsonDocument command, ConnectionDescription connectionDescription, out bool messageContainsSessionId, out bool secondaryOk)
        {
            messageContainsSessionId = false;
            var extraElements = new List<BsonElement>();
            if (_session.Id != null)
            {
                var areSessionsSupported =
                    connectionDescription.HelloResult.LogicalSessionTimeout.HasValue ||
                    connectionDescription.HelloResult.ServiceId.HasValue;
                if (areSessionsSupported)
                {
                    var lsid = new BsonElement("lsid", _session.Id);
                    extraElements.Add(lsid);
                    messageContainsSessionId = true;
                }
                else
                {
                    if (!_session.IsImplicit)
                    {
                        throw new MongoClientException("Sessions are not supported.");
                    }
                }
            }
            if (_serverApi != null)
            {
                extraElements.Add(new BsonElement("apiVersion", _serverApi.Version.ToString()));
                if (_serverApi.Strict.HasValue)
                {
                    extraElements.Add(new BsonElement("apiStrict", _serverApi.Strict.Value));
                }
                if (_serverApi.DeprecationErrors.HasValue)
                {
                    extraElements.Add(new BsonElement("apiDeprecationErrors", _serverApi.DeprecationErrors.Value));
                }
            }
            if (_session.ClusterTime != null)
            {
                var clusterTime = new BsonElement("$clusterTime", _session.ClusterTime);
                extraElements.Add(clusterTime);
            }
            var appendExtraElementsSerializer = new ElementAppendingSerializer<BsonDocument>(BsonDocumentSerializer.Instance, extraElements);
            var commandWithExtraElements = new BsonDocumentWrapper(command, appendExtraElementsSerializer);

            var serverType = connectionDescription != null ? connectionDescription.HelloResult.ServerType : ServerType.Unknown;
            var readPreferenceDocument = QueryHelper.CreateReadPreferenceDocument(serverType, _readPreference, out secondaryOk);

            var wrappedCommand = new BsonDocument
            {
                { "$query", commandWithExtraElements },
                { "$readPreference", readPreferenceDocument, readPreferenceDocument != null }
            };
            if (_additionalOptions != null)
            {
                wrappedCommand.Merge(_additionalOptions, overwriteExistingElements: false);
            }

            if (wrappedCommand.ElementCount == 1)
            {
                return wrappedCommand["$query"].AsBsonDocument;
            }
            else
            {
                return wrappedCommand;
            }
        }

        // nested types
        private class IgnoredReply
        {
            public static IgnoredReply Instance = new IgnoredReply();
        }

        private class IgnoredReplySerializer : SerializerBase<IgnoredReply>
        {
            public static IgnoredReplySerializer Instance = new IgnoredReplySerializer();

            public override IgnoredReply Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
            {
                context.Reader.ReadStartDocument();
                while (context.Reader.ReadBsonType() != BsonType.EndOfDocument)
                {
                    context.Reader.SkipName();
                    context.Reader.SkipValue();
                }
                context.Reader.ReadEndDocument();
                return IgnoredReply.Instance;
            }
        }
    }
}
