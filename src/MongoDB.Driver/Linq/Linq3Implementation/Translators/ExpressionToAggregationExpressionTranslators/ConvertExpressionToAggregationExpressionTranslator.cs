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
using System.Text.RegularExpressions;
using System.Xml.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Expressions;

namespace MongoDB.Driver.Linq.Linq3Implementation.Translators.ExpressionToAggregationExpressionTranslators
{
    internal static class ConvertExpressionToAggregationExpressionTranslator
    {
        public static AggregationExpression Translate(TranslationContext context, UnaryExpression expression)
        {
            if (expression.NodeType == ExpressionType.Convert)
            {
                var operandExpression = expression.Operand;
                var operandTranslation = ExpressionToAggregationExpressionTranslator.Translate(context, operandExpression);

                var expressionType = expression.Type;
                if (expressionType.IsConstructedGenericType && expressionType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    var valueType = expressionType.GetGenericArguments()[0];
                    if (operandExpression.Type == valueType)
                    {
                        // use the same AST but with a new nullable serializer
                        var nullableSerializerType = typeof(NullableSerializer<>).MakeGenericType(valueType);
                        var valueSerializerType = typeof(IBsonSerializer<>).MakeGenericType(valueType);
                        var constructorInfo = nullableSerializerType.GetConstructor(new[] { valueSerializerType });
                        var nullableSerializer = (IBsonSerializer)constructorInfo.Invoke(new[] { operandTranslation.Serializer });
                        return new AggregationExpression(expression, operandTranslation.Ast, nullableSerializer);
                    }
                }

                if (expressionType == typeof(BsonValue))
                {
                    return TranslateConvertToBsonValue(expression, operandTranslation.Ast);
                }

                var ast = operandTranslation.Ast;
                IBsonSerializer serializer;
                if (expressionType.IsInterface)
                {
                    // when an expression is cast to an interface it's a no-op as far as we're concerned
                    // and we can just use the serializer for the concrete type and members not defined in the interface will just be ignored
                    serializer = operandTranslation.Serializer;
                }
                else
                {
                    ast = AstExpression.Convert(ast, expressionType);
                    serializer = context.KnownSerializersRegistry.GetSerializer(expression);
                }

                return new AggregationExpression(expression, ast, serializer);
            }

            throw new ExpressionNotSupportedException(expression);
        }

        private static AggregationExpression TranslateConvertToBsonValue(UnaryExpression expression, AstExpression operandAst)
        {
            var operandType = expression.Operand.Type;
            if (operandType.IsGenericType && operandType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                operandType = operandType.GetGenericArguments()[0];
            }

            var convertedOperandAst = Type.GetTypeCode(operandType) switch
            {
                TypeCode.Object when operandType == typeof(byte[]) => ErrorIfUnexpectedDataType(operandAst, expectedType: "binData"),
                TypeCode.Boolean => AstExpression.ToBool(operandAst),
                TypeCode.DateTime => AstExpression.ToDate(operandAst),
                TypeCode.Decimal => AstExpression.ToDecimal(operandAst),
                TypeCode.Object when operandType == typeof(Decimal128) => AstExpression.ToDecimal(operandAst),
                TypeCode.Double => AstExpression.ToDouble(operandAst),
                TypeCode.Object when operandType == typeof(Guid) => ErrorIfUnexpectedDataType(operandAst, expectedType: "binData"),
                TypeCode.Int32 => AstExpression.ToInt(operandAst),
                TypeCode.Int64 => AstExpression.ToLong(operandAst),
                TypeCode.Object when operandType == typeof(ObjectId) => AstExpression.ToObjectId(operandAst),
                TypeCode.Object when operandType == typeof(Regex) => ErrorIfUnexpectedDataType(operandAst, expectedType: "regex"),
                TypeCode.String => AstExpression.ToString(operandAst),
                _ => throw new ExpressionNotSupportedException(expression, because: $"conversion from {expression.Operand.Type} to {expression.Type} is not supported")
            };

            return new AggregationExpression(expression, convertedOperandAst, BsonValueSerializer.Instance);

            static AstExpression ErrorIfUnexpectedDataType(AstExpression operandAst, string expectedType)
            {
                // this expression is designed to fail server side if the data encountered is not null or of the expected type
                return AstExpression.Cond(
                    AstExpression.In(AstExpression.Type(operandAst), AstExpression.ComputedArray(new AstExpression[] { "null", expectedType })),
                    operandAst,
                    AstExpression.Convert(operandAst, expectedType)); // will fail server side
            }
        }
    }
}
