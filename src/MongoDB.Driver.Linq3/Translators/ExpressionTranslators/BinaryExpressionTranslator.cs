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
using MongoDB.Driver.Linq3.Ast.Expressions;

namespace MongoDB.Driver.Linq3.Translators.ExpressionTranslators
{
    public static class BinaryExpressionTranslator
    {
        public static TranslatedExpression Translate(TranslationContext context, BinaryExpression expression)
        {
            AstBinaryOperator? binaryOperator = null;
            AstNaryOperator? naryOperator = null;
            switch (expression.NodeType)
            {
                case ExpressionType.Add: naryOperator = AstNaryOperator.Add; break;
                case ExpressionType.And: naryOperator = AstNaryOperator.And; break;
                case ExpressionType.AndAlso: naryOperator = AstNaryOperator.And; break;
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
                case ExpressionType.Or: naryOperator = AstNaryOperator.Or; break;
                case ExpressionType.OrElse: naryOperator = AstNaryOperator.Or; break;
                case ExpressionType.Power: binaryOperator = AstBinaryOperator.Pow; break;
                case ExpressionType.Subtract: binaryOperator = AstBinaryOperator.Subtract; break;
            }

            if (binaryOperator != null | naryOperator != null)
            {
                var translatedLeft = ExpressionTranslator.Translate(context, expression.Left);
                var translatedRight = ExpressionTranslator.Translate(context, expression.Right);

                //var translation = new BsonDocument(operatorName, new BsonArray { translatedLeft.Translation, translatedRight.Translation });
                var translation = binaryOperator != null ?
                    (AstExpression)new AstBinaryExpression(binaryOperator.Value, translatedLeft.Translation, translatedRight.Translation) :
                    (AstExpression)new AstNaryExpression(naryOperator.Value, translatedLeft.Translation, translatedRight.Translation);
                var serializer = translatedLeft.Serializer ?? translatedRight.Serializer;
                return new TranslatedExpression(expression, translation, serializer);
            }

            throw new ExpressionNotSupportedException(expression);
        }
    }
}