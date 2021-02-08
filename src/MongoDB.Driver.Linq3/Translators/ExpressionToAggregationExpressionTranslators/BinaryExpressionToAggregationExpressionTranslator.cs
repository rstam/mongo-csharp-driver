﻿/* Copyright 2010-present MongoDB Inc.
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
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq3.Ast.Expressions;

namespace MongoDB.Driver.Linq3.Translators.ExpressionToAggregationExpressionTranslators
{
    public static class BinaryExpressionToAggregationExpressionTranslator
    {
        public static AggregationExpression Translate(TranslationContext context, BinaryExpression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Add:
                    return AddExpressionToAggregationExpressionTranslator.Translate(context, expression);

                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return AndExpressionToAggregationExpressionTranslator.Translate(context, expression);

                case ExpressionType.Divide:
                    return DivideExpressionToAggregationExpressionTranslator.Translate(context, expression);

                case ExpressionType.Multiply:
                    return MultiplyExpressionToAggregationExpressionTranslator.Translate(context, expression);

                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return OrExpressionToAggregationExpressionTranslator.Translate(context, expression);
            }

            AstBinaryOperator? binaryOperator = null;
            AstNaryOperator? naryOperator = null;
            switch (expression.NodeType)
            {
                case ExpressionType.Coalesce: binaryOperator = AstBinaryOperator.IfNull; break;
                case ExpressionType.Divide: binaryOperator = AstBinaryOperator.Divide; break;
                case ExpressionType.Equal: binaryOperator = AstBinaryOperator.Eq; break;
                case ExpressionType.GreaterThan: binaryOperator = AstBinaryOperator.Gt; break;
                case ExpressionType.GreaterThanOrEqual: binaryOperator = AstBinaryOperator.Gte; break;
                case ExpressionType.LessThan: binaryOperator = AstBinaryOperator.Lt; break;
                case ExpressionType.LessThanOrEqual: binaryOperator = AstBinaryOperator.Lte; break;
                case ExpressionType.Modulo: binaryOperator = AstBinaryOperator.Mod; break;
                case ExpressionType.Multiply: binaryOperator = AstBinaryOperator.Multiply; break;
                case ExpressionType.NotEqual: binaryOperator = AstBinaryOperator.Ne; break;
                case ExpressionType.Power: binaryOperator = AstBinaryOperator.Pow; break;
                case ExpressionType.Subtract: binaryOperator = AstBinaryOperator.Subtract; break;
            }

            if (binaryOperator != null | naryOperator != null)
            {
                var leftTranslation = ExpressionToAggregationExpressionTranslator.Translate(context, expression.Left);
                var rightTranslation = ExpressionToAggregationExpressionTranslator.Translate(context, expression.Right);

                var ast = binaryOperator != null ?
                    (AstExpression)new AstBinaryExpression(binaryOperator.Value, leftTranslation.Ast, rightTranslation.Ast) :
                    (AstExpression)new AstNaryExpression(naryOperator.Value, leftTranslation.Ast, rightTranslation.Ast);
                var serializer = BsonSerializer.LookupSerializer(expression.Type); // TODO: get correct serializer

                return new AggregationExpression(expression, ast, serializer);
            }

            throw new ExpressionNotSupportedException(expression);
        }
    }
}