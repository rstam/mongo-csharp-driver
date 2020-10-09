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
using MongoDB.Driver.Linq3.Methods;
using MongoDB.Driver.Linq3.Misc;

namespace MongoDB.Driver.Linq3.Translators.ExpressionTranslators.MethodTranslators
{
    public static class FloorMethodTranslator
    {
        public static ExpressionTranslation Translate(TranslationContext context, MethodCallExpression expression)
        {
            var method = expression.Method;
            var arguments = expression.Arguments;

            if (method.IsOneOf(MathMethod.FloorWithDecimal, MathMethod.FloorWithDouble))
            {
                var argumentExpression = arguments[0];

                var serverType = method.GetParameters()[0].ParameterType;
                argumentExpression = ConvertHelper.RemoveUnnecessaryConvert(argumentExpression, impliedType: serverType);
                var argumentTranslation = ExpressionTranslator.Translate(context, argumentExpression);
                var ast = new AstUnaryExpression(AstUnaryOperator.Floor, argumentTranslation.Ast);
                var serializer = BsonSerializer.LookupSerializer(expression.Type);

                return new ExpressionTranslation(expression, ast, serializer);
            }

            throw new ExpressionNotSupportedException(expression);
        }
    }
}