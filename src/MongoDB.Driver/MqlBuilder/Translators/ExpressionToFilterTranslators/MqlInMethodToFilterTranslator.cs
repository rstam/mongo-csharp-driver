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

using System.Collections;
using System.Linq.Expressions;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Filters;
using MongoDB.Driver.Linq.Linq3Implementation.Misc;
using MongoDB.Driver.MqlBuilder.Translators.Context;

namespace MongoDB.Driver.MqlBuilder.Translators.ExpressionToFilterTranslators
{
    internal static class MqlInMethodToFilterTranslator
    {
        public static AstFilter Translate(MqlTranslationContext context, MethodCallExpression expression)
        {
            var method = expression.Method;
            var arguments = expression.Arguments;

            if (method.IsOneOf(MqlMethod.In, MqlMethod.Nin))
            {
                var valueExpression = arguments[0];
                var field = MqlExpressionToFilterFieldTranslator.Translate(context, valueExpression);

                var valuesExpression = arguments[1];
                if (valuesExpression is ConstantExpression constantValuesExpression)
                {
                    var values = (IEnumerable)constantValuesExpression.Value;
                    var serializedValues = SerializationHelper.SerializeValues(field.Serializer, values);
                    return method.Is(MqlMethod.In) ?
                        AstFilter.In(field, serializedValues) :
                        AstFilter.Nin(field, serializedValues);
                }

                throw new MqlExpressionNotSupportedException(expression, because: "values must be a constant array");
            }

            throw new MqlExpressionNotSupportedException(expression);
        }
    }
}
