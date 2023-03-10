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
using MongoDB.Bson.Serialization;
using MongoDB.Driver.MqlBuilder.Translators.Context;

namespace MongoDB.Driver.MqlBuilder.Translators.ExpressionToAggregationExpressionTranslators
{
    internal class MqlConvertToAggregationExpressionTranslator
    {
        public static MqlAggregationExpression Translate(MqlTranslationContext context, UnaryExpression expression)
        {
            if (expression.NodeType == ExpressionType.Convert)
            {
                var sourceType = expression.Operand.Type;
                var targetType = expression.Type;
                if (IsWideningConvert(sourceType, targetType))
                {
                    var operandExpression = expression.Operand;
                    var operandTranslation = MqlExpressionToAggregationExpressionTranslator.Translate(context, operandExpression);
                    var targetTypeSerializer = BsonSerializer.LookupSerializer(targetType);
                    return new MqlAggregationExpression(expression, operandTranslation.Ast, targetTypeSerializer);
                }
            }

            throw new MqlExpressionNotSupportedException(expression);
        }

        private static bool IsWideningConvert(Type sourceType, Type targetType)
        {
            return (Type.GetTypeCode(sourceType), Type.GetTypeCode(targetType)) switch
            {
                (TypeCode.Byte, TypeCode.Int32) => true,
                (TypeCode.Int16, TypeCode.Int32) => true,
                (TypeCode.Int32, TypeCode.Double) => true,
                _ => false
            };
        }
    }
}
