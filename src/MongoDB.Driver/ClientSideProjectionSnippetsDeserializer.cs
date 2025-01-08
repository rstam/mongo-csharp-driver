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
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Driver
{
    internal static class ClientSideProjectionSnippetsDeserializer
    {
        private static readonly Type[] __deserializerGenericTypeDefinitions =
        [
            null,
            typeof(ClientSideProjectionSnippetsDeserializer<,>),
            typeof(ClientSideProjectionSnippetsDeserializer<,,>),
            typeof(ClientSideProjectionSnippetsDeserializer<,,,>),
            typeof(ClientSideProjectionSnippetsDeserializer<,,,,>)
        ];

        public const int MaxNumberOfSnippets = 4; // could be expanded up to 16

        public static IBsonSerializer Create(
            Type projectionType,
            IBsonSerializer[] snippetDeserializers,
            Delegate projector)
        {
            var snippetTypes = snippetDeserializers.Select(s => s.ValueType).ToArray();
            var deserializerGenericTypeDefinition = __deserializerGenericTypeDefinitions[snippetTypes.Length];
            var deserializerGenericTypeArguments = snippetTypes.Append(projectionType).ToArray();
            var deserializerType = deserializerGenericTypeDefinition.MakeGenericType(deserializerGenericTypeArguments);
            return (IBsonSerializer)Activator.CreateInstance(deserializerType, [snippetDeserializers, projector]);
        }
    }

    internal abstract class ClientSideProjectionSnippetsDeserializer<TProjection> : SerializerBase<TProjection>, IClientSideProjectionDeserializer
    {
        private readonly IBsonSerializer[] _snippetDeserializers;

        public ClientSideProjectionSnippetsDeserializer(IBsonSerializer[] snippetDeserializers)
        {
            _snippetDeserializers = snippetDeserializers;
        }

        protected object[] DeserializeSnippets(BsonDeserializationContext context)
        {
            var reader = context.Reader;
            var snippets = new object[_snippetDeserializers.Length];

            reader.ReadStartDocument();
            while (reader.ReadBsonType() != BsonType.EndOfDocument)
            {
                var name = reader.ReadName();
                var i = ParseIndex(name);
                snippets[i] = _snippetDeserializers[i].Deserialize(context);
            }
            reader.ReadEndDocument();

            return snippets;

            int ParseIndex(string name)
            {
                if (name.StartsWith("_") &&
                    int.TryParse(name.Substring(1), out var index) &&
                    index >= 0 && index < _snippetDeserializers.Length)
                {
                    return index;
                }

                throw new FormatException("Invalid snippet name: " + name);
            }
        }
    }

    internal class ClientSideProjectionSnippetsDeserializer<T1, TProjection> : ClientSideProjectionSnippetsDeserializer<TProjection>
    {
        private readonly Func<T1, TProjection> _projector;

        public ClientSideProjectionSnippetsDeserializer(IBsonSerializer[] snippetDeserializers, Func<T1, TProjection> projector)
            : base(snippetDeserializers)
        {
            _projector = projector;
        }

        public override TProjection Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var snippets = DeserializeSnippets(context);
            return _projector((T1)snippets[0]);
        }
    }

    internal class ClientSideProjectionSnippetsDeserializer<T1, T2, TProjection> : ClientSideProjectionSnippetsDeserializer<TProjection>
    {
        private readonly Func<T1, T2, TProjection> _projector;

        public ClientSideProjectionSnippetsDeserializer(IBsonSerializer[] snippetDeserializers, Func<T1, T2, TProjection> projector)
            : base(snippetDeserializers)
        {
            _projector = projector;
        }

        public override TProjection Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var snippets = DeserializeSnippets(context);
            return _projector((T1)snippets[0], (T2)snippets[1]);
        }
    }

    internal class ClientSideProjectionSnippetsDeserializer<T1, T2, T3, TProjection> : ClientSideProjectionSnippetsDeserializer<TProjection>
    {
        private readonly Func<T1, T2, T3, TProjection> _projector;

        public ClientSideProjectionSnippetsDeserializer(IBsonSerializer[] snippetDeserializers, Func<T1, T2, T3, TProjection> projector)
            : base(snippetDeserializers)
        {
            _projector = projector;
        }

        public override TProjection Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var snippets = DeserializeSnippets(context);
            return _projector((T1)snippets[0], (T2)snippets[1], (T3)snippets[2]);
        }
    }

    internal class ClientSideProjectionSnippetsDeserializer<T1, T2, T3, T4, TProjection> : ClientSideProjectionSnippetsDeserializer<TProjection>
    {
        private readonly Func<T1, T2, T3, T4, TProjection> _projector;

        public ClientSideProjectionSnippetsDeserializer(IBsonSerializer[] snippetDeserializers, Func<T1, T2, T3, T4, TProjection> projector)
            : base(snippetDeserializers)
        {
            _projector = projector;
        }

        public override TProjection Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var snippets = DeserializeSnippets(context);
            return _projector((T1)snippets[0], (T2)snippets[1], (T3)snippets[2], (T4)snippets[3]);
        }
    }
}
