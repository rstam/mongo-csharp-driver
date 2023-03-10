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
using MongoDB.Driver.MqlBuilder.Translators.Context;

namespace MongoDB.Driver.MqlBuilder.Translators.ExpressionToAggregationExpressionTranslators
{
    internal static class MqlExpressionToAggregationExpressionTranslator
    {
        public static MqlAggregationExpression Translate(MqlTranslationContext context, Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Call: return MqlMethodToAggregationExpressionTranslator.Translate(context, (MethodCallExpression)expression);
                case ExpressionType.MemberAccess: return MqlMemberAccessToAggregationExpressionTranslator.Translate(context, (MemberExpression)expression);
                case ExpressionType.Parameter: return MqlParameterToAggregationExpressionTranslator.Translate(context, (ParameterExpression)expression);

                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                    return MqlComparisonOperatorToAggregationExpressionTranslator.Translate(context, (BinaryExpression)expression);

                default:
                    throw new MqlExpressionNotSupportedException(expression);
            }
        }
    }
}
