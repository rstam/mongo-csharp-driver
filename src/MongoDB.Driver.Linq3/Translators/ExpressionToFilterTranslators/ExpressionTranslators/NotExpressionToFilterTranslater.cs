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
using MongoDB.Driver.Linq3.Ast.Filters;

namespace MongoDB.Driver.Linq3.Translators.ExpressionToFilterTranslators.ExpressionTranslators
{
    public static class NotExpressionToFilterTranslator
    {
        public static AstFilter Translate(TranslationContext context, UnaryExpression expression)
        {
            var operandExpression = expression.Operand;

            if (operandExpression.Type == typeof(bool))
            {
                var operandTranslation = ExpressionToFilterTranslator.Translate(context, operandExpression);

                if (operandTranslation is AstNorFilter innerNorFilter)
                {
                    var innerArgs = innerNorFilter.Args;
                    if (innerArgs.Length == 1)
                    {
                        return innerArgs[0];
                    }
                }

                return new AstNorFilter(operandTranslation);
            }

            throw new ExpressionNotSupportedException(expression);
        }
    }
}