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
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Expressions;
using MongoDB.Driver.Linq.Linq3Implementation.Misc;
using MongoDB.Driver.Linq.Linq3Implementation.Reflection;

namespace MongoDB.Driver.Linq.Linq3Implementation.Translators.ExpressionToAggregationExpressionTranslators.MethodTranslators
{
    internal static class SequenceEqualMethodToAggregationExpressionTranslator
    {
        public static TranslatedExpression Translate(TranslationContext context, MethodCallExpression expression)
        {
            var method = expression.Method;
            var arguments = expression.Arguments;

            if (method.IsOneOf(EnumerableMethod.SequenceEqual, QueryableMethod.SequenceEqual))
            {
                var firstExpression = arguments[0];
                var secondExpression = arguments[1];

                var firstTranslation = ExpressionToAggregationExpressionTranslator.Translate(context, firstExpression);
                var secondTranslation = ExpressionToAggregationExpressionTranslator.Translate(context, secondExpression);

                var (firstBinding, firstVar) = AstExpression.VarBinding("first", firstTranslation.Ast);
                var (secondBinding, secondVar) = AstExpression.VarBinding("second", secondTranslation.Ast);
                var pairVar = AstExpression.Var("pair");

                var ast = AstExpression.Let(
                    vars: [firstBinding, secondBinding],
                    @in: AstExpression.And(
                        AstExpression.Eq(AstExpression.Type(firstVar), "array"),
                        AstExpression.Eq(AstExpression.Type(secondVar), "array"),
                        AstExpression.Eq(AstExpression.Size(firstVar), AstExpression.Size(secondVar)),
                        AstExpression.AllElementsTrue(
                            AstExpression.Map(
                                input: AstExpression.Zip([firstVar, secondVar]),
                                @as: pairVar,
                                @in: AstExpression.Eq(AstExpression.ArrayElemAt(pairVar, 0), AstExpression.ArrayElemAt(pairVar, 1)))))
                );

                return new TranslatedExpression(expression, ast, new BooleanSerializer());
            }

            throw new ExpressionNotSupportedException(expression);
        }
    }
}
