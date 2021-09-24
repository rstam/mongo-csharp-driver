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

using System.Linq.Expressions;
using MongoDB.Bson.Serialization;
using ExpressionVisitor = System.Linq.Expressions.ExpressionVisitor;

namespace MongoDB.Driver.Linq.Linq3Implementation.Serializers.KnownSerializers
{
    internal class KnownSerializerFinder : ExpressionVisitor
    {
        #region static
        // public static methods
        public static KnownSerializersRegistry FindKnownSerializers(Expression root, IBsonDocumentSerializer rootSerializer)
        {
            var visitor = new KnownSerializerFinder(root, rootSerializer);
            visitor.Visit(root);
            return visitor._registry;
        }
        #endregion

        // private fields
        private KnownSerializersNode _currentKnownSerializersNode;
        private IBsonDocumentSerializer _parentSerializer;
        private readonly KnownSerializersRegistry _registry = new();
        private readonly Expression _root;
        private readonly IBsonDocumentSerializer _rootSerializer;

        // constructors
        private KnownSerializerFinder(Expression root, IBsonDocumentSerializer rootSerializer)
        {
            _rootSerializer = rootSerializer;
            _root = root;
        }

        // public methods
        public override Expression Visit(Expression node)
        {
            if (node == null) return null;

            _currentKnownSerializersNode = new KnownSerializersNode(_currentKnownSerializersNode);

            if (node == _root)
            {
                _parentSerializer = _rootSerializer;
            }

            var result = base.Visit(node);
            _registry.Add(node, _currentKnownSerializersNode);
            _currentKnownSerializersNode = _currentKnownSerializersNode.Parent;
            return result;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var result = base.VisitMember(node);

            if (_parentSerializer.TryGetMemberSerializationInfo(node.Member.Name, out var memberSerializer))
            {
                PropagateToRoot(node, memberSerializer.Serializer);

                if (memberSerializer.Serializer is IBsonDocumentSerializer bsonDocumentSerializer)
                {
                    _parentSerializer = bsonDocumentSerializer;
                }
            }

            return result;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            var result = base.VisitParameter(node);

            if (node.Type == _rootSerializer.ValueType)
            {
                PropagateToRoot(_root, _rootSerializer);
            }

            return result;
        }

        private void PropagateToRoot(Expression node, IBsonSerializer memberSerializer)
        {
            var knownSerializers = _currentKnownSerializersNode;
            while (knownSerializers != null)
            {
                knownSerializers.AddKnownSerializer(node.Type, memberSerializer);
                knownSerializers = knownSerializers.Parent;
            }
        }
    }
}
