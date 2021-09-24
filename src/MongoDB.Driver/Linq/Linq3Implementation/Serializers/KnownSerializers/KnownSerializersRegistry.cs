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
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Bson.Serialization;

namespace MongoDB.Driver.Linq.Linq3Implementation.Serializers.KnownSerializers
{
    internal class KnownSerializersRegistry
    {
        // private fields
        private readonly BsonClassMapSerializationProvider _bsonClassMapSerializationProvider = new();
        private readonly CollectionsSerializationProvider _collectionsSerializationProvider = new();
        private readonly PrimitiveSerializationProvider _primitiveSerializationProvider = new();
        private readonly Dictionary<Expression, KnownSerializersNode> _registry = new();

        // public methods
        public void Add(Expression expression, KnownSerializersNode knownSerializers)
        {
            if (_registry.ContainsKey(expression)) return;

            _registry.Add(expression, knownSerializers);
        }

        public HashSet<IBsonSerializer> GetPossibleSerializers(Expression expression)
        {
            if (_registry.TryGetValue(expression, out var knownSerializers))
            {
                return knownSerializers.GetPossibleSerializers(expression.Type);
            }
            else
            {
                return new HashSet<IBsonSerializer>();
            }
        }

        public IBsonSerializer GetSerializer(Expression expr)
        {
            var possibleSerializers = GetPossibleSerializers(expr);
            if (possibleSerializers.Count == 0)
            {
                var type = expr.Type;
                var serializer = _primitiveSerializationProvider.GetSerializer(type)
                                 ?? _collectionsSerializationProvider.GetSerializer(type)
                                 ?? _bsonClassMapSerializationProvider.GetSerializer(type);
                if (serializer != null)
                {
                    return serializer;
                }
                throw new InvalidOperationException($"Cannot find serializer for {expr}.");
            }
            if (possibleSerializers.Count > 1)
            {
                throw new InvalidOperationException($"More than one possible serializer found for {expr}.");
            }
            return possibleSerializers.First();
        }

        public IBsonSerializer GetSerializer(Type type)
        {
            return _primitiveSerializationProvider.GetSerializer(type);
        }
    }
}
