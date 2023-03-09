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
using MongoDB.Driver.MqlBuilder.Translators.Context;

namespace MongoDB.Driver.MqlBuilder.Translators.ExpressionToFilterTranslators
{
    internal static class MqlMethodCallToFilterTranslator
    {
        public static AstFilter Translate(MqlTranslationContext context, MethodCallExpression expression)
        {
            var method = expression.Method;

            switch (method.Name)
            {
                case "All": return MqlAllMethodToFilterTranslator.Translate(context, expression);
                case "ElemMatch": return MqlElemMatchMethodToFilterTranslator.Translate(context, expression);
                case "Expr": return MqlExprMethodToFilterTranslator.Translate(context, expression);
                case "JsonSchema": return MqlJsonSchemaMethodToFilterTranslator.Translate(context, expression);
                case "Nor": return MqlNorMethodToFilterTranslator.Translate(context, expression);
                case "Type": return MqlTypeMethodToFilterTranslator.Translate(context, expression);
                case "Regex": return MqlRegexMethodToFilterTranslator.Translate(context, expression);
                case "Size": return MqlSizeMethodToFilterTranslator.Translate(context, expression);
                case "Text": return MqlTextMethodToFilterTranslator.Translate(context, expression);

                case "BitsAllClear":
                case "BitsAllSet":
                case "BitsAnyClear":
                case "BitsAnySet":
                    return MqlBitsMethodToFilterTranslator.Translate(context, expression);

                case "Exists":
                case "NotExists":
                    return MqlExistsMethodToFilterTranslator.Translate(context, expression);

                case "In":
                case "Nin":
                    return MqlInMethodToFilterTranslator.Translate(context, expression);
            }

            throw new MqlExpressionNotSupportedException(expression);
        }
    }
}
