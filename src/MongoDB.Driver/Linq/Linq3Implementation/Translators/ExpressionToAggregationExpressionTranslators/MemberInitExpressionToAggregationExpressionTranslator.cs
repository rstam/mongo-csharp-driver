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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq.Linq3Implementation.Ast;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Expressions;

namespace MongoDB.Driver.Linq.Linq3Implementation.Translators.ExpressionToAggregationExpressionTranslators
{
    internal static class MemberInitExpressionToAggregationExpressionTranslator
    {
        public static AggregationExpression Translate(TranslationContext context, MemberInitExpression expression)
        {
            var computedFields = new List<AstComputedField>();
            var newExpression = expression.NewExpression;
            var constructorInfo = newExpression.Constructor;
            var classMap = CreateClassMap(expression.Type, newExpression.Constructor);
            var creatorMap = classMap.CreatorMaps.Single(x => x.MemberInfo == constructorInfo);
            if (constructorInfo.GetParameters().Length > 0 && creatorMap.Arguments == null )
            {
                throw new ExpressionNotSupportedException(expression, because: $"can't find matching properties for constructor parameters.");
            }

            var constructorArguments = newExpression.Arguments;
            var parameterIndex = -1;
            var creatorArguments = creatorMap.Arguments ?? Array.Empty<MemberInfo>();
            foreach (var argument in creatorArguments)
            {
                parameterIndex++;
                var valueExpression = constructorArguments[parameterIndex];
                var valueTranslation = ExpressionToAggregationExpressionTranslator.Translate(context, valueExpression);
                var valueType = valueExpression.Type;
                var memberMap = EnsureMemberMap(expression, classMap, argument);
                memberMap.SetSerializer(valueTranslation.Serializer ?? BsonSerializer.LookupSerializer(valueType));
                computedFields.Add(AstExpression.ComputedField(memberMap.ElementName, valueTranslation.Ast));
            }

            foreach (var binding in expression.Bindings)
            {
                var memberAssignment = (MemberAssignment)binding;
                var member = memberAssignment.Member;
                var memberMap = FindMemberMap(expression, classMap, member.Name);

                var valueExpression = memberAssignment.Expression;
                var valueTranslation = ExpressionToAggregationExpressionTranslator.Translate(context, valueExpression);
                computedFields.Add(AstExpression.ComputedField(memberMap.ElementName, valueTranslation.Ast));

                memberMap.SetSerializer(valueTranslation.Serializer);
            }

            var ast = AstExpression.ComputedDocument(computedFields);
            classMap.Freeze();
            var serializerType = typeof(BsonClassMapSerializer<>).MakeGenericType(expression.Type);
            var serializer = (IBsonSerializer)Activator.CreateInstance(serializerType, classMap);

            return new AggregationExpression(expression, ast, serializer);
        }

        private static BsonClassMap CreateClassMap(Type classType, ConstructorInfo constructor)
        {
            BsonClassMap baseClassMap = null;
            if (classType.BaseType  != null)
            {
                baseClassMap = CreateClassMap(classType.BaseType, null);
            }

            var classMapType = typeof(BsonClassMap<>).MakeGenericType(classType);
            var constructorInfo = classMapType.GetConstructor(new Type[] { typeof(BsonClassMap) });
            var classMap = (BsonClassMap)constructorInfo.Invoke(new object[] { baseClassMap });
            if (constructor != null)
            {
                classMap.MapConstructor(constructor);
            }

            classMap.AutoMap();
            classMap.IdMemberMap?.SetElementName("_id"); // normally happens when Freeze is called but we need it sooner here

            return classMap;
        }

        private static BsonMemberMap EnsureMemberMap(Expression expression, BsonClassMap classMap, MemberInfo memberInfo)
        {
            var declaringClassMap = classMap;
            while (declaringClassMap.ClassType != memberInfo.DeclaringType)
            {
                if (declaringClassMap.BaseClassMap == null)
                {
                    throw new ExpressionNotSupportedException(expression, because: $"can't find matching property for constructor parameter : {memberInfo.Name}");
                }

                declaringClassMap = declaringClassMap.BaseClassMap;
            }

            var result = declaringClassMap.DeclaredMemberMaps
                .SingleOrDefault(x => x.MemberInfo.MemberType == memberInfo.MemberType && x.MemberInfo.Name == memberInfo.Name);

            if (result == null)
            {
                result = declaringClassMap.MapMember(memberInfo);
            }

            return result;
        }

        private static BsonMemberMap FindMemberMap(Expression expression, BsonClassMap classMap, string memberName)
        {
            foreach (var memberMap in classMap.DeclaredMemberMaps)
            {
                if (memberMap.MemberName == memberName)
                {
                    return memberMap;
                }
            }

            if (classMap.BaseClassMap != null)
            {
                return FindMemberMap(expression, classMap.BaseClassMap, memberName);
            }

            throw new ExpressionNotSupportedException(expression, because: $"can't find member map: {memberName}");
        }
    }
}
