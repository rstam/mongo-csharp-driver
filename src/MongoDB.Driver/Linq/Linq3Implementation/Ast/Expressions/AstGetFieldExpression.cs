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

using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Visitors;

namespace MongoDB.Driver.Linq.Linq3Implementation.Ast.Expressions
{
    internal sealed class AstGetFieldExpression : AstExpression
    {
        private readonly AstExpression _fieldName;
        private readonly AstExpression _input;

        public AstGetFieldExpression(AstExpression input, AstExpression fieldName)
        {
            _input = Ensure.IsNotNull(input, nameof(input));
            _fieldName = Ensure.IsNotNull(fieldName, nameof(fieldName));
        }

        public AstExpression FieldName => _fieldName;
        public AstExpression Input => _input;
        public override AstNodeType NodeType => AstNodeType.GetFieldExpression;

        public override AstNode Accept(AstNodeVisitor visitor)
        {
            return visitor.VisitGetFieldExpression(this);
        }

        public override bool CanBeRenderedAsFieldPath()
        {
            return IsSafeFieldName(_fieldName) && _input.CanBeRenderedAsFieldPath();
        }

        public override BsonValue Render()
        {
            var renderedInput = _input.Render();

            if (_fieldName is AstConstantExpression constantExpression &&
                constantExpression.Value.IsString)
            {
                var fieldName = constantExpression.Value.AsString;
                if (IsSafeFieldName(fieldName))
                {
                    if (_input is AstVarExpression varExpression)
                    {
                        return varExpression.IsCurrent ? $"${fieldName}" : $"$${varExpression.Name}.{fieldName}";
                    }

                    if (renderedInput.IsString)
                    {
                        var inputPath = renderedInput.AsString;
                        if (inputPath.StartsWith("$"))
                        {
                            return $"{inputPath}.{fieldName}";
                        }
                    }

                    // $getField is only supported in server versions >= 5.0
                    return new BsonDocument
                    {
                        { "$let", new BsonDocument
                            {
                                { "vars", new BsonDocument("this", renderedInput) },
                                { "in", $"$$this.{fieldName}" }
                            }
                        }
                    };
                }
            }

            return new BsonDocument("$getField", new BsonDocument { { "field", _fieldName.Render() }, { "input", renderedInput } });
        }

        public AstGetFieldExpression Update(AstExpression input, AstExpression fieldName)
        {
            if (input == _input && fieldName == _fieldName)
            {
                return this;
            }

            return new AstGetFieldExpression(input, fieldName);
        }

        private bool IsSafeFieldName(AstExpression fieldName)
        {
            return
                fieldName is AstConstantExpression constantExpression &&
                constantExpression.Value is BsonString constantString &&
                IsSafeFieldName(constantString.Value);
        }

        private bool IsSafeFieldName(string fieldName)
        {
            return fieldName.Length > 0 && IsSafeFirstChar(fieldName[0]) && fieldName.Skip(1).All(c => IsSafeSubsequentChar(c));

            static bool IsSafeFirstChar(char c) => c == '_' || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
            static bool IsSafeSubsequentChar(char c) => IsSafeFirstChar(c) || (c >= '0' && c <= '9');
        }
    }
}
