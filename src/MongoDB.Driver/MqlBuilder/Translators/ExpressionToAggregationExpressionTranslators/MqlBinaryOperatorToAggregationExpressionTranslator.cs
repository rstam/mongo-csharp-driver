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
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Expressions;
using MongoDB.Driver.MqlBuilder.Translators.Context;

namespace MongoDB.Driver.MqlBuilder.Translators.ExpressionToAggregationExpressionTranslators
{
    internal static class MqlBinaryOperatorToAggregationExpressionTranslator
    {
        public static MqlAggregationExpression Translate(MqlTranslationContext context, BinaryExpression expression)
        {
            var isBinaryOperator = TryGetBinaryOperator(expression, out var binaryOperator);
            var isNaryOperator = TryGetNaryOperator(expression, out var naryOperator);
            if (isBinaryOperator || isNaryOperator)
            {
                var arg1Expression = expression.Left;
                var arg2Expression = expression.Right;

                AstExpression arg1Ast, arg2Ast;
                IBsonSerializer serializer;
                if (arg1Expression is ConstantExpression arg1ConstantExpression)
                {
                    var arg2Translation = MqlExpressionToAggregationExpressionTranslator.Translate(context, arg2Expression);
                    serializer = arg2Translation.Serializer;
                    var comparand = MqlExpressionToSerializedConstantTranslator.Translate(arg1Expression, expression, serializer);
                    arg1Ast = AstExpression.Constant(comparand);
                    arg2Ast = arg2Translation.Ast;
                }
                else if (arg2Expression is ConstantExpression arg2ConstantExpression)
                {
                    var arg1Translation = MqlExpressionToAggregationExpressionTranslator.Translate(context, arg1Expression);
                    serializer = arg1Translation.Serializer;
                    var comparand = MqlExpressionToSerializedConstantTranslator.Translate(arg2Expression, expression, serializer);
                    arg1Ast = arg1Translation.Ast;
                    arg2Ast = AstExpression.Constant(comparand);
                }
                else
                {
                    var arg1Translation = MqlExpressionToAggregationExpressionTranslator.Translate(context, arg1Expression);
                    var arg2Translation = MqlExpressionToAggregationExpressionTranslator.Translate(context, arg2Expression);
                    arg1Ast = arg1Translation.Ast;
                    arg2Ast = arg2Translation.Ast;
                    serializer = arg1Translation.Serializer;
                }

                var ast = isBinaryOperator ? AstExpression.Binary(binaryOperator, arg1Ast, arg2Ast) : AstExpression.Nary(naryOperator, arg1Ast, arg2Ast);
                return new MqlAggregationExpression(expression, ast, serializer);
            }

            throw new MqlExpressionNotSupportedException(expression);
        }

        private static bool TryGetBinaryOperator(Expression expression, out AstBinaryOperator binaryOperator)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Divide: binaryOperator = AstBinaryOperator.Divide; return true;
                case ExpressionType.Modulo: binaryOperator = AstBinaryOperator.Mod; return true;
                case ExpressionType.Subtract: binaryOperator = AstBinaryOperator.Subtract; return true;
                default: binaryOperator = 0; return false;
            }
        }

        private static bool TryGetNaryOperator(Expression expression, out AstNaryOperator naryOperator)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Add: naryOperator = AstNaryOperator.Add; return true;
                case ExpressionType.Multiply: naryOperator = AstNaryOperator.Multiply; return true;
                default: naryOperator = 0; return false;
            }
        }
    }
}
