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
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Visitors;

namespace MongoDB.Driver.Linq.Linq3Implementation.Ast.Expressions
{
    internal sealed class AstQueryParameterExpression : AstExpression
    {
        private readonly string _name;
        private readonly ParameterExpression _parameter;

        public AstQueryParameterExpression(ParameterExpression parameter, string name)
        {
            _parameter = Ensure.IsNotNull(parameter, nameof(parameter));
            _name = Ensure.IsNotNullOrEmpty(name, nameof(name));
        }

        public string Name => _name;
        public override AstNodeType NodeType => AstNodeType.QueryParameterExpression;
        public ParameterExpression Parameter => _parameter;

        public override AstNode Accept(AstNodeVisitor visitor)
        {
            return visitor.VisitQueryParameterExpression(this);
        }

        public override BsonValue Render()
        {
            throw new NotSupportedException();
        }
    }
}
