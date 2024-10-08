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

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for LazyBsonDocuments.
    /// </summary>
    public sealed class LazyBsonDocumentSerializer : BsonValueSerializerBase<LazyBsonDocument>
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="LazyBsonDocumentSerializer"/> class.
        /// </summary>
        public LazyBsonDocumentSerializer()
            : base(BsonType.Document)
        {
        }

        // protected methods
        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>A deserialized value.</returns>
        protected override LazyBsonDocument DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;
            var slice = bsonReader.ReadRawBsonDocument();
            return new LazyBsonDocument(slice);
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The object.</param>
        protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, LazyBsonDocument value)
        {
            var bsonWriter = context.Writer;

            var slice = value.Slice;
            if (slice == null)
            {
                BsonDocumentSerializer.Instance.Serialize(context, value);
            }
            else
            {
                bsonWriter.WriteRawBsonDocument(slice);
            }
        }
    }
}
