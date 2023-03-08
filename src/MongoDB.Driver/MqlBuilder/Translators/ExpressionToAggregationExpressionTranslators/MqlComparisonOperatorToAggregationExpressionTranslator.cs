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
using MongoDB.Driver.MqlBuilder.Translators.Context;

namespace MongoDB.Driver.MqlBuilder.Translators.ExpressionToAggregationExpressionTranslators
{
    internal static class MqlComparisonOperatorToAggregationExpressionTranslator
    {
        public static MqlAggregationExpression Translate(MqlTranslationContext context, BinaryExpression expression)
        {
            if (TryGetComparisonOperator(expression, out var comparisonOperator))
            {
                var arg1Expression = expression.Left;
                var arg2Expression = expression.Right;

                AstExpression arg1Ast, arg2Ast;
                if (arg1Expression is ConstantExpression arg1ConstantExpression)
                {
                    var arg2Translation = MqlExpressionToAggregationExpressionTranslator.Translate(context, arg2Expression);
                    var comparand = MqlExpressionToSerializedConstantTranslator.Translate(arg1Expression, expression, arg2Translation.Serializer);
                    arg1Ast = AstExpression.Constant(comparand);
                    arg2Ast = arg2Translation.Ast;
                }
                else if (arg2Expression is ConstantExpression arg2ConstantExpression)
                {
                    var arg1Translation = MqlExpressionToAggregationExpressionTranslator.Translate(context, arg1Expression);
                    var comparand = MqlExpressionToSerializedConstantTranslator.Translate(arg2Expression, expression, arg1Translation.Serializer);
                    arg1Ast = arg1Translation.Ast;
                    arg2Ast = AstExpression.Constant(comparand);
                }
                else
                {
                    var arg1Translation = MqlExpressionToAggregationExpressionTranslator.Translate(context, arg1Expression);
                    var arg2Translation = MqlExpressionToAggregationExpressionTranslator.Translate(context, arg2Expression);
                    arg1Ast = arg1Translation.Ast;
                    arg2Ast = arg2Translation.Ast;
                }

                var ast = AstExpression.Binary(comparisonOperator, arg1Ast, arg2Ast);
                return new MqlAggregationExpression(expression, ast, BsonBooleanSerializer.Instance);
            }

            throw new MqlExpressionNotSupportedException(expression);
        }

        private static bool TryGetComparisonOperator(Expression expression, out AstBinaryOperator comparisonOperator)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Equal: comparisonOperator = AstBinaryOperator.Eq; return true;
                case ExpressionType.NotEqual: comparisonOperator = AstBinaryOperator.Ne; return true;
                case ExpressionType.LessThan: comparisonOperator = AstBinaryOperator.Lt; return true;
                case ExpressionType.GreaterThan: comparisonOperator = AstBinaryOperator.Gt; return true;
                case ExpressionType.LessThanOrEqual: comparisonOperator = AstBinaryOperator.Lte; return true;
                case ExpressionType.GreaterThanOrEqual: comparisonOperator = AstBinaryOperator.Gte; return true;
                default: comparisonOperator = 0; return false;
            }
        }
    }
}
