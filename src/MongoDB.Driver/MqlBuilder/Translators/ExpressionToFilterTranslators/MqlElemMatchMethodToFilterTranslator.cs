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
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Filters;
using MongoDB.Driver.Linq.Linq3Implementation.Misc;
using MongoDB.Driver.Linq.Linq3Implementation.Serializers;
using MongoDB.Driver.MqlBuilder.Translators.Context;

namespace MongoDB.Driver.MqlBuilder.Translators.ExpressionToFilterTranslators
{
    internal static class MqlElemMatchMethodToFilterTranslator
    {
        public static AstFilter Translate(MqlTranslationContext context, MethodCallExpression expression)
        {
            var method = expression.Method;
            var arguments = expression.Arguments;

            if (method.Is(MqlMethod.ElemMatch))
            {
                var fieldExpression = arguments[0];
                var predicateExpression = arguments[1];

                var field = MqlExpressionToFilterFieldTranslator.Translate(context, fieldExpression);
                if (field.Serializer is IBsonArraySerializer arraySerializer)
                {
                    if (arraySerializer.TryGetItemSerializationInfo(out var itemSerializationInfo))
                    {
                        var elementSerializer = itemSerializationInfo.Serializer;
                        if (predicateExpression is LambdaExpression predicateLambda)
                        {
                            var parameterExpression = predicateLambda.Parameters.Single();
                            var parameterSymbol = context.CreateSymbol(parameterExpression, "@<elem>", elementSerializer); // @<elem> represents the implied element
                            var predicateContext = context.WithSingleSymbol(parameterSymbol); // @<elem> is the only symbol visible inside an $elemMatch
                            var predicateFilter = MqlExpressionToFilterTranslator.Translate(predicateContext, predicateLambda.Body);

                            return AstFilter.ElemMatch(field, predicateFilter);
                        }

                        throw new MqlExpressionNotSupportedException(fieldExpression, expression, because: "the predicate is not a lambda expression");
                    }

                    throw new MqlExpressionNotSupportedException(fieldExpression, expression, because: $"the serializer for {field.Serializer.ValueType} did not provide the item serializer");
                }

                throw new MqlExpressionNotSupportedException(fieldExpression, expression, because: $"the serializer for {field.Serializer.ValueType} does not implement the IBsonArraySerializer interface");
            }

            throw new MqlExpressionNotSupportedException(expression);
        }
    }
}
