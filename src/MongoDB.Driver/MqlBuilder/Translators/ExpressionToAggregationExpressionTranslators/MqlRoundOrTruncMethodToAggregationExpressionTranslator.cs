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
using System.Reflection;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Expressions;
using MongoDB.Driver.Linq.Linq3Implementation.Misc;
using MongoDB.Driver.MqlBuilder.Translators.Context;

namespace MongoDB.Driver.MqlBuilder.Translators.ExpressionToAggregationExpressionTranslators
{
    internal class MqlRoundOrTruncMethodToAggregationExpressionTranslator
    {
        private static readonly MethodInfo[] __roundOrTruncMethods =
        {
            MqlMethod.RoundToDouble,
            MqlMethod.RoundToInteger,
            MqlMethod.RoundToIntegerWithPlace,
            MqlMethod.TruncToDouble,
            MqlMethod.TruncToInteger,
            MqlMethod.TruncToIntegerWithPlace
        };

        private static readonly MethodInfo[] __roundOrTruncToDoubleMethods =
        {
            MqlMethod.RoundToDouble,
            MqlMethod.TruncToDouble
        };

        private static readonly MethodInfo[] __roundOrTruncWithPlaceMethods =
        {
            MqlMethod.RoundToDouble,
            MqlMethod.RoundToIntegerWithPlace,
            MqlMethod.TruncToDouble,
            MqlMethod.TruncToIntegerWithPlace
        };

        public static MqlAggregationExpression Translate(MqlTranslationContext context, MethodCallExpression expression)
        {
            var method = expression.Method;
            var args = expression.Arguments;

            if (method.IsOneOf(__roundOrTruncMethods))
            {
                var numberExpression = args[0];
                var numberTranslation = MqlExpressionToAggregationExpressionTranslator.Translate(context, numberExpression);
                var numberAst = numberTranslation.Ast;

                AstExpression placeAst = null;
                if (method.IsOneOf(__roundOrTruncWithPlaceMethods))
                {
                    var placeExpression = args[1];
                    var placeTranslation = MqlExpressionToAggregationExpressionTranslator.Translate(context, placeExpression);
                    placeAst = placeTranslation.Ast;
                }

                var ast = method switch
                {
                    _ when method.Is(MqlMethod.RoundToDouble) => AstExpression.Round(numberAst, placeAst),
                    _ when method.Is(MqlMethod.RoundToInteger) => AstExpression.Round(numberAst),
                    _ when method.Is(MqlMethod.RoundToIntegerWithPlace) => AstExpression.Round(numberAst, placeAst),
                    _ when method.Is(MqlMethod.TruncToDouble) => AstExpression.Trunc(numberAst, placeAst),
                    _ when method.Is(MqlMethod.TruncToInteger) => AstExpression.Trunc(numberAst),
                    _ when method.Is(MqlMethod.TruncToIntegerWithPlace) => AstExpression.Trunc(numberAst, placeAst),
                    _ => throw new Exception($"Unexpected method: {method.Name}")
                };

                IBsonSerializer serializer = method.IsOneOf(__roundOrTruncToDoubleMethods) ? DoubleSerializer.Instance : Int32Serializer.Instance;
                return new MqlAggregationExpression(expression, ast, serializer);
            }

            throw new MqlExpressionNotSupportedException(expression);
        }
    }
}
