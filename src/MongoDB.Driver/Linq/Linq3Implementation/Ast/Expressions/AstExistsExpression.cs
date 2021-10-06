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

using MongoDB.Bson;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Visitors;

namespace MongoDB.Driver.Linq.Linq3Implementation.Ast.Expressions
{
    internal sealed class AstExistsExpression : AstExpression
    {
        private readonly bool _exists;
        private readonly AstExpression _field;

        public AstExistsExpression(AstExpression field, bool exists)
        {
            _field = Ensure.IsNotNull(field, nameof(field));
            _exists = exists;
        }

        public new bool Exists => _exists;
        public AstExpression Field => _field;
        public override AstNodeType NodeType => AstNodeType.ExistsExpression;

        public override AstNode Accept(AstNodeVisitor visitor)
        {
            return visitor.VisitExistsExpression(this);
        }

        public override BsonValue Render()
        {
            // the $exists operator doesn't actually exist (yet?)
            // it will be replaced with a comparison to BsonUndefined by the AstSimplifier
            return new BsonDocument("$exists", new BsonArray { _field.Render(), _exists });
        }

        public AstExistsExpression Update(AstExpression arg)
        {
            if (arg == _field)
            {
                return this;
            }

            return new AstExistsExpression(arg, _exists);
        }
    }
}
