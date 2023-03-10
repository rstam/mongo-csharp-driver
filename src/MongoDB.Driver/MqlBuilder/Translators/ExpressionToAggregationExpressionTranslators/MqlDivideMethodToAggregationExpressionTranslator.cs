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
using MongoDB.Driver.MqlBuilder.Translators.Context;

namespace MongoDB.Driver.MqlBuilder.Translators.ExpressionToAggregationExpressionTranslators
{
    internal class MqlDivideMethodToAggregationExpressionTranslator
    {
        public static MqlAggregationExpression Translate(MqlTranslationContext context, MethodCallExpression expression)
        {
            var method = expression.Method;
            var args = expression.Arguments;

            if (method.Is(MqlMethod.Divide))
            {
                var xExpression = args[0];
                var yExpression = args[1];
                var xTranslation = MqlExpressionToAggregationExpressionTranslator.Translate(context, xExpression);
                var yTranslation = MqlExpressionToAggregationExpressionTranslator.Translate(context, yExpression);
                var ast = AstExpression.Divide(xTranslation.Ast, yTranslation.Ast);
                return new MqlAggregationExpression(expression, ast, DoubleSerializer.Instance);
            }

            throw new MqlExpressionNotSupportedException(expression);
        }
    }
}
