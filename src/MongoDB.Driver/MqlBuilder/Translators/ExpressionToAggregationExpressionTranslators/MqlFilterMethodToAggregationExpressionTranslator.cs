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
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Expressions;
using MongoDB.Driver.Linq.Linq3Implementation.Misc;
using MongoDB.Driver.Linq.Linq3Implementation.Serializers;
using MongoDB.Driver.MqlBuilder.Translators.Context;
using MongoDB.Driver.MqlBuilder.Translators.Misc;

namespace MongoDB.Driver.MqlBuilder.Translators.ExpressionToAggregationExpressionTranslators
{
    internal class MqlFilterMethodToAggregationExpressionTranslator
    {
        public static MqlAggregationExpression Translate(MqlTranslationContext context, MethodCallExpression expression)
        {
            var method = expression.Method;
            var args = expression.Arguments;

            if (method.IsOneOf(MqlMethod.Filter, MqlMethod.FilterWithLimit))
            {
                var inputExpression = args[0];
                var inputTranslation = MqlExpressionToAggregationExpressionTranslator.Translate(context, inputExpression);
                var input = inputTranslation.Ast;

                var condExpression = (LambdaExpression)args[1];
                var itemSerializer = MqlArrayHelper.GetItemSerializer(inputExpression, expression, inputTranslation.Serializer);
                var (condContext, varName) = context.WithParameterSymbol(condExpression, itemSerializer);
                var condTranslation = MqlExpressionToAggregationExpressionTranslator.Translate(condContext, condExpression.Body);
                var cond = condTranslation.Ast;

                AstExpression limit = null;
                if (method.Is(MqlMethod.FilterWithLimit))
                {
                    var limitExpression = args[2];
                    var limitTranslation = MqlExpressionToAggregationExpressionTranslator.Translate(context, limitExpression);
                    limit = limitTranslation.Ast;
                }

                var ast = AstExpression.Filter(input, cond, varName, limit);
                var serializer = IEnumerableSerializer.Create(itemSerializer);
                return new MqlAggregationExpression(expression, ast, serializer);
            }

            throw new MqlExpressionNotSupportedException(expression);
        }
    }
}
