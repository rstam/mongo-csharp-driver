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
using System.Reflection;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Expressions;
using MongoDB.Driver.Linq.Linq3Implementation.Misc;
using MongoDB.Driver.Linq.Linq3Implementation.Reflection;
using MongoDB.Driver.Linq.Linq3Implementation.Serializers;

namespace MongoDB.Driver.Linq.Linq3Implementation.Translators.ExpressionToAggregationExpressionTranslators.MethodTranslators
{
    internal static class OfTypeMethodToAggregationExpressionTranslator
    {
        private static readonly MethodInfo[] __ofTypeMethods =
        {
            EnumerableMethod.OfType,
            QueryableMethod.OfType
        };

        public static AggregationExpression Translate(TranslationContext context, MethodCallExpression expression)
        {
            var method = expression.Method;
            var arguments = expression.Arguments;

            if (method.IsOneOf(__ofTypeMethods))
            {
                var sourceExpression = arguments[0];
                var sourceTranslation = ExpressionToAggregationExpressionTranslator.TranslateEnumerable(context, sourceExpression);
                NestedAsQueryableHelper.EnsureQueryableMethodHasNestedAsQueryableSource(expression, sourceTranslation);
                var itemSerializer = ArraySerializerHelper.GetItemSerializer(sourceTranslation.Serializer);

                var nominalType = sourceTranslation.Serializer.ValueType;
                var actualType = method.GetGenericArguments()[0];

                if (nominalType == actualType)
                {
                    return sourceTranslation;
                }

                var discriminatorConvention = itemSerializer.GetDiscriminatorConvention();
                var discriminatorElementName = discriminatorConvention.ElementName;

                var itemVar = AstExpression.Var("this");
                var discriminatorField = AstExpression.GetField(itemVar, discriminatorElementName);
                var ofTypePredicate = discriminatorConvention switch
                {
                    IHierarchicalDiscriminatorConvention hierarchicalDiscriminatorConvention => DiscriminatorAstExpression.TypeIs(discriminatorField, hierarchicalDiscriminatorConvention, nominalType, actualType),
                    IScalarDiscriminatorConvention scalarDiscriminatorConvention => DiscriminatorAstExpression.TypeIs(discriminatorField, scalarDiscriminatorConvention, nominalType, actualType),
                    _ => throw new ExpressionNotSupportedException(expression, because: "is operator is not supported with the configured discriminator convention")
                };

                var ast = AstExpression.Filter(
                    input: sourceTranslation.Ast,
                    @as: itemVar.Name,
                    cond: ofTypePredicate);
                var actualTypeSerializer = BsonSerializer.LookupSerializer(actualType);
                var resultSerializer = NestedAsQueryableSerializer.CreateIEnumerableOrNestedAsQueryableSerializer(expression.Type, actualTypeSerializer);

                return new AggregationExpression(expression, ast, resultSerializer);
            }

            throw new ExpressionNotSupportedException(expression);
        }
    }
}