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
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Filters;
using MongoDB.Driver.Linq.Linq3Implementation.Translators.ExpressionToFilterTranslators.ToFilterFieldTranslators;
using MongoDB.Driver.MqlBuilder.Translators.Context;

namespace MongoDB.Driver.MqlBuilder.Translators.ExpressionToFilterTranslators
{
    internal static class MqlComparisonOperatorToFilterTranslator
    {
        public static AstFilter Translate(MqlTranslationContext context, BinaryExpression expression)
        {
            if (IsModFilter(expression))
            {
                return TranslateModFilter(context, expression);
            }

            if (TryGetComparisonOperator(expression, out var comparisonOperator))
            {
                var field = MqlExpressionToFilterFieldTranslator.Translate(context, expression.Left);
                var value = MqlExpressionToSerializedConstantTranslator.Translate(expression.Right, expression, field.Serializer);
                return AstFilter.Compare(field, comparisonOperator, value);
            }

            throw new MqlExpressionNotSupportedException(expression);
        }

        private static bool IsModFilter(BinaryExpression expression)
        {
            return
                expression.NodeType == ExpressionType.Equal &&
                expression.Left is BinaryExpression leftBinaryExpression &&
                leftBinaryExpression.NodeType == ExpressionType.Modulo;
        }

        private static AstFilter TranslateModFilter(MqlTranslationContext context, BinaryExpression expression)
        {
            if (expression.NodeType == ExpressionType.Equal &&
                expression.Left is BinaryExpression leftBinaryExpression &&
                leftBinaryExpression.NodeType == ExpressionType.Modulo)
            {
                var field = MqlExpressionToFilterFieldTranslator.Translate(context, leftBinaryExpression.Left);
                var divisor = MqlExpressionToConstantTranslator.Translate<int>(leftBinaryExpression.Right, expression); // TODO: handle other integral types
                var remainder = MqlExpressionToConstantTranslator.Translate<int>(expression.Right, expression); // TODO: handle other integral types

                return AstFilter.Mod(field, divisor, remainder);
            }

            throw new MqlExpressionNotSupportedException(expression);
        }

        private static bool TryGetComparisonOperator(Expression expression, out AstComparisonFilterOperator comparisonOperator)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Equal: comparisonOperator = AstComparisonFilterOperator.Eq; return true;
                case ExpressionType.NotEqual: comparisonOperator = AstComparisonFilterOperator.Ne; return true;
                case ExpressionType.LessThan: comparisonOperator = AstComparisonFilterOperator.Lt; return true;
                case ExpressionType.GreaterThan: comparisonOperator = AstComparisonFilterOperator.Gt; return true;
                case ExpressionType.LessThanOrEqual: comparisonOperator = AstComparisonFilterOperator.Lte; return true;
                case ExpressionType.GreaterThanOrEqual: comparisonOperator = AstComparisonFilterOperator.Gte; return true;
                default: comparisonOperator = 0; return false;
            }
        }
    }
}
