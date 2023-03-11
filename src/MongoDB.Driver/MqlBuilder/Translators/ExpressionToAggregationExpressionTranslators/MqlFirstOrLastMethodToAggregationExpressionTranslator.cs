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
using System.Reflection;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Expressions;
using MongoDB.Driver.Linq.Linq3Implementation.Misc;
using MongoDB.Driver.Linq.Linq3Implementation.Reflection;
using MongoDB.Driver.MqlBuilder.Translators.Context;
using MongoDB.Driver.MqlBuilder.Translators.Misc;

namespace MongoDB.Driver.MqlBuilder.Translators.ExpressionToAggregationExpressionTranslators
{
    internal class MqlFirstOrLastMethodToAggregationExpressionTranslator
    {
        private static readonly MethodInfo[] __firstOrLastMethods =
        {
            EnumerableMethod.First,
            EnumerableMethod.Last,
            MqlMethod.FirstN,
            MqlMethod.LastN
        };

        private static readonly MethodInfo[] __firstOrLastWithNMethods =
        {
            MqlMethod.FirstN,
            MqlMethod.LastN
        };

        public static MqlAggregationExpression Translate(MqlTranslationContext context, MethodCallExpression expression)
        {
            var method = expression.Method;
            var args = expression.Arguments;

            if (method.IsOneOf(__firstOrLastMethods))
            {
                var inputExpression = args[0];
                var inputTranslation = MqlExpressionToAggregationExpressionTranslator.Translate(context, inputExpression);
                var input = inputTranslation.Ast;

                AstExpression n = null;
                if (method.IsOneOf(__firstOrLastWithNMethods))
                {
                    var nExpression = args[1];
                    var nTranslation = MqlExpressionToAggregationExpressionTranslator.Translate(context, nExpression);
                    n = nTranslation.Ast;
                }

                var ast = method switch
                {
                    _ when method.Is(EnumerableMethod.First) => AstExpression.First(input),
                    _ when method.Is(EnumerableMethod.Last) => AstExpression.Last(input),
                    _ when method.Is(MqlMethod.FirstN) => AstExpression.FirstN(input ,n),
                    _ when method.Is(MqlMethod.LastN) => AstExpression.LastN(input, n),
                    _ => throw new Exception($"Unexpected method: {method.Name}")
                };

                var itemSerializer = MqlArrayHelper.GetItemSerializer(inputExpression, expression, inputTranslation.Serializer);
                return new MqlAggregationExpression(expression, ast, itemSerializer);
            }

            throw new MqlExpressionNotSupportedException(expression);
        }
    }
}
