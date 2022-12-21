/* Copyright 2019-present MongoDB Inc.
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
using MongoDB.Bson.Serialization;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents the void output data type to a pipeline with no output, such as after a $out stage. No actual instances of this type can be created.
    /// </summary>
    public sealed class NoPipelineOutput
    {
        private NoPipelineOutput()
        {
        }
    }

    /// <summary>
    /// The serializer for NoPipelineOutput.
    /// </summary>
    internal sealed class NoPipelineOutputSerializer : IBsonSerializer<NoPipelineOutput>, IBsonDocumentSerializer
    {
        #region static
        // private static fields
        private static readonly NoPipelineOutputSerializer __instance = new NoPipelineOutputSerializer();

        // public static properties
        /// <summary>
        ///  Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static NoPipelineOutputSerializer Instance => __instance;
        #endregion

        /// <inheritdoc/>
        public Type ValueType => typeof(NoPipelineOutput);


        object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public NoPipelineOutput Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            throw new NotSupportedException();
        }

        void IBsonSerializer.Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, NoPipelineOutput value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public bool TryGetMemberSerializationInfo(string memberName, out BsonSerializationInfo serializationInfo)
        {
            serializationInfo = null;
            return false;
        }
    }
}
