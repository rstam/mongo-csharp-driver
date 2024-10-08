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
    /// Represents a serializer for BsonDocumentWrappers.
    /// </summary>
    public sealed class BsonDocumentWrapperSerializer : BsonValueSerializerBase<BsonDocumentWrapper>
    {
        // private static fields
        private static BsonDocumentWrapperSerializer __instance = new BsonDocumentWrapperSerializer();

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonDocumentWrapperSerializer class.
        /// </summary>
        public BsonDocumentWrapperSerializer()
            : base(BsonType.Document)
        {
        }

        // public static properties
        /// <summary>
        /// Gets an instance of the BsonDocumentWrapperSerializer class.
        /// </summary>
        public static BsonDocumentWrapperSerializer Instance
        {
            get { return __instance; }
        }

        // public methods
        /// <summary>
        /// Deserializes a class.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>A deserialized value.</returns>
        public override BsonDocumentWrapper Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            throw CreateCannotBeDeserializedException();
        }

        // protected methods
        /// <summary>
        /// Deserializes a class.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>An object.</returns>
        protected override BsonDocumentWrapper DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            throw CreateCannotBeDeserializedException();
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The object.</param>
        protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, BsonDocumentWrapper value)
        {
            value.Serializer.Serialize(context, value.Wrapped);
        }
    }
}
