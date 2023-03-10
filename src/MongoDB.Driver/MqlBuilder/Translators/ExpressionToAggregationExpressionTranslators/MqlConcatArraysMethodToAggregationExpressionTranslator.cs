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

using System.Collections.Generic;
using System.Linq.Expressions;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Expressions;
using MongoDB.Driver.Linq.Linq3Implementation.Misc;
using MongoDB.Driver.MqlBuilder.Translators.Context;

namespace MongoDB.Driver.MqlBuilder.Translators.ExpressionToAggregationExpressionTranslators
{
    internal class MqlConcatArraysMethodToAggregationExpressionTranslator
    {
        public static MqlAggregationExpression Translate(MqlTranslationContext context, MethodCallExpression expression)
        {
            var method = expression.Method;
            var args = expression.Arguments;

            if (method.Is(MqlMethod.ConcatArrays))
            {
                var firstExpression = args[0];
                var firstTranslation = MqlExpressionToAggregationExpressionTranslator.Translate(context, firstExpression);
                var arrays = new List<AstExpression> { firstTranslation.Ast };
                var serializer = firstTranslation.Serializer;

                var otherExpressions = (NewArrayExpression)args[1];
                foreach (var otherExpression in otherExpressions.Expressions)
                {
                    var otherTranslation = MqlExpressionToAggregationExpressionTranslator.Translate(context, otherExpression);
                    arrays.Add(otherTranslation.Ast);
                }

                var ast = AstExpression.ConcatArrays(arrays.ToArray());
                return new MqlAggregationExpression(expression, ast, serializer);
            }

            throw new MqlExpressionNotSupportedException(expression);
        }
    }
}
