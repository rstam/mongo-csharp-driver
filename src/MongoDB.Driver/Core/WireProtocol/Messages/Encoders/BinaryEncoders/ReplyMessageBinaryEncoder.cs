/* Copyright 2010-present MongoDB Inc.
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
using System.IO;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.WireProtocol.Messages.Encoders.BinaryEncoders
{
    internal class ReplyMessageBinaryEncoder<TDocument> : MessageBinaryEncoderBase, IMessageEncoder
    {
        // fields
        private readonly IBsonSerializer<TDocument> _serializer;

        // constructors
        public ReplyMessageBinaryEncoder(Stream stream, MessageEncoderSettings encoderSettings, IBsonSerializer<TDocument> serializer)
            : base(stream, encoderSettings)
        {
            _serializer = Ensure.IsNotNull(serializer, nameof(serializer));
        }

        // methods
        public ReplyMessage<TDocument> ReadMessage()
        {
            var binaryReader = CreateBinaryReader();
            var stream = binaryReader.BsonStream;

            stream.ReadInt32(); // messageSize
            var requestId = stream.ReadInt32();
            var responseTo = stream.ReadInt32();
            var opcode = (Opcode)stream.ReadInt32();
            EnsureOpcodeIsValid(opcode);
            var flags = (ResponseFlags)stream.ReadInt32();
            var cursorId = stream.ReadInt64();
            var startingFrom = stream.ReadInt32();
            var numberReturned = stream.ReadInt32();
            List<TDocument> documents = null;
            BsonDocument queryFailureDocument = null;

            var awaitCapable = (flags & ResponseFlags.AwaitCapable) == ResponseFlags.AwaitCapable;
            var cursorNotFound = (flags & ResponseFlags.CursorNotFound) == ResponseFlags.CursorNotFound;
            var queryFailure = (flags & ResponseFlags.QueryFailure) == ResponseFlags.QueryFailure;

            if (queryFailure)
            {
                var context = BsonDeserializationContext.CreateRoot(binaryReader);
                queryFailureDocument = BsonDocumentSerializer.Instance.Deserialize(context);
            }
            else
            {
                documents = new List<TDocument>();
                for (var i = 0; i < numberReturned; i++)
                {
                    var allowDuplicateElementNames = typeof(TDocument) == typeof(BsonDocument);
                    var context = BsonDeserializationContext.CreateRoot(binaryReader, builder =>
                    {
                        builder.AllowDuplicateElementNames = allowDuplicateElementNames;
                    });
                    documents.Add(_serializer.Deserialize(context));
                }
            }

            return new ReplyMessage<TDocument>(
                awaitCapable,
                cursorId,
                cursorNotFound,
                documents,
                numberReturned,
                queryFailure,
                queryFailureDocument,
                requestId,
                responseTo,
                _serializer,
                startingFrom);
        }

        public void WriteMessage(ReplyMessage<TDocument> message)
        {
            Ensure.IsNotNull(message, nameof(message));

            var binaryWriter = CreateBinaryWriter();
            var stream = binaryWriter.BsonStream;
            var startPosition = stream.Position;

            stream.WriteInt32(0); // messageSize
            stream.WriteInt32(message.RequestId);
            stream.WriteInt32(message.ResponseTo);
            stream.WriteInt32((int)Opcode.Reply);

            var flags = ResponseFlags.None;
            if (message.AwaitCapable)
            {
                flags |= ResponseFlags.AwaitCapable;
            }
            if (message.QueryFailure)
            {
                flags |= ResponseFlags.QueryFailure;
            }
            if (message.CursorNotFound)
            {
                flags |= ResponseFlags.CursorNotFound;
            }
            stream.WriteInt32((int)flags);

            stream.WriteInt64(message.CursorId);
            stream.WriteInt32(message.StartingFrom);
            stream.WriteInt32(message.NumberReturned);
            if (message.QueryFailure)
            {
                var context = BsonSerializationContext.CreateRoot(binaryWriter);
                _serializer.Serialize(context, message.QueryFailureDocument);
            }
            else
            {
                foreach (var doc in message.Documents)
                {
                    var context = BsonSerializationContext.CreateRoot(binaryWriter);
                    _serializer.Serialize(context, doc);
                }
            }
            stream.BackpatchSize(startPosition);
        }

        // private methods
        private void EnsureOpcodeIsValid(Opcode opcode)
        {
            if (opcode != Opcode.Reply)
            {
                throw new FormatException("Reply message opcode is not OP_REPLY.");
            }
        }

        // explicit interface implementations
        MongoDBMessage IMessageEncoder.ReadMessage()
        {
            return ReadMessage();
        }

        void IMessageEncoder.WriteMessage(MongoDBMessage message)
        {
            WriteMessage((ReplyMessage<TDocument>)message);
        }

        // nested types
        [Flags]
        private enum ResponseFlags
        {
            None = 0,
            CursorNotFound = 1,
            QueryFailure = 2,
            AwaitCapable = 8
        }
    }
}
