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
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Filters;
using MongoDB.Driver.MqlBuilder.Translators.Context;

namespace MongoDB.Driver.MqlBuilder.Translators.ExpressionToFilterTranslators
{
    internal static class MqlMemberAccessToFilterFieldTranslator
    {
        public static AstFilterField Translate(MqlTranslationContext context, MemberExpression expression)
        {
            var objectExpression = expression.Expression;
            var objectField = MqlExpressionToFilterFieldTranslator.Translate(context, objectExpression);
            if (objectField.Serializer is IBsonDocumentSerializer documentSerializer)
            {
                var member = expression.Member;
                if (documentSerializer.TryGetMemberSerializationInfo(member.Name, out var memberSerializationInfo))
                {
                    var elementName = memberSerializationInfo.ElementName;
                    var subFieldSerializer = memberSerializationInfo.Serializer;
                    return objectField.SubField(elementName, subFieldSerializer);
                }

                throw new MqlExpressionNotSupportedException(expression, because: $"the serializer for {objectExpression.Type} did not provide any serialization info for member {member.Name}");
            }

            throw new MqlExpressionNotSupportedException(expression, because: $"the serializer for {objectExpression.Type} does not implement IBsonDocumentSerializer");
        }
    }
}
