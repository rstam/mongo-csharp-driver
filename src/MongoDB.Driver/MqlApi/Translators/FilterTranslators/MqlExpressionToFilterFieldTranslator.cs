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
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Filters;
using MongoDB.Driver.MqlApi.Translators.Context;

namespace MongoDB.Driver.MqlApi.Translators.FilterTranslators
{
    internal static class MqlExpressionToFilterFieldTranslator
    {
        public static AstFilterField Translate(MqlTranslationContext context, Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.MemberAccess: return MqlMemberExpressionToFilterFieldTranslator.Translate(context, (MemberExpression)expression);
                case ExpressionType.Parameter: return MqlParameterExpressionToFilterFieldTranslator.Translate(context, (ParameterExpression)expression);
                default: throw new MqlExpressionNotSupportedException(expression, because: "the expression cannot be translated to a filter field");
            }
        }
    }
}
