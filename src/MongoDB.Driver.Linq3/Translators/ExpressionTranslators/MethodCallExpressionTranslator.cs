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
using MongoDB.Driver.Linq3.Translators.ExpressionTranslators.MethodCallTranslators;

namespace MongoDB.Driver.Linq3.Translators.ExpressionTranslators
{
    public static class MethodCallExpressionTranslator
    {
        public static TranslatedExpression Translate(TranslationContext context, MethodCallExpression expression)
        {
            switch (expression.Method.Name)
            {
                case "All": return AllTranslator.Translate(context, expression);
                case "Any": return AnyTranslator.Translate(context, expression);
                case "Concat": return ConcatTranslator.Translate(context, expression);
                case "Contains": return ContainsTranslator.Translate(context, expression);
                case "Count": return CountTranslator.Translate(context, expression);
                case "Distinct": return DistinctTranslator.Translate(context, expression);
                case "ElementAt": return ElementAtTranslator.Translate(context, expression);
                case "Except": return ExceptTranslator.Translate(context, expression);
                case "Min": return MinTranslator.Translate(context, expression);
                case "Select": return SelectTranslator.Translate(context, expression);
            }

            throw new ExpressionNotSupportedException(expression);
        }
    }
}