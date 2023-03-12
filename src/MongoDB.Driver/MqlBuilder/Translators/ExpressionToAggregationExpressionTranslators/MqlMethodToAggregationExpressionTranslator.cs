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
using MongoDB.Driver.MqlBuilder.Translators.Context;

namespace MongoDB.Driver.MqlBuilder.Translators.ExpressionToAggregationExpressionTranslators
{
    internal class MqlMethodToAggregationExpressionTranslator
    {
        public static MqlAggregationExpression Translate(MqlTranslationContext context, MethodCallExpression expression)
        {
            switch (expression.Method.Name)
            {
                case "Abs": return MqlAbsMethodToAggregationExpressionTranslator.Translate(context, expression);
                case "ConcatArrays": return MqlConcatArraysMethodToAggregationExpressionTranslator.Translate(context, expression);
                case "Divide": return MqlDivideMethodToAggregationExpressionTranslator.Translate(context, expression);
                case "Exp": return MqlExpMethodToAggregationExpressionTranslator.Translate(context, expression);
                case "Filter": return MqlFilterMethodToAggregationExpressionTranslator.Translate(context, expression);
                case "In": return MqlInMethodToAggregationExpressionTranslator.Translate(context, expression);
                case "Pow": return MqlPowMethodToAggregationExpressionTranslator.Translate(context, expression);
                case "Sqrt": return MqlSqrtMethodToAggregationExpressionTranslator.Translate(context, expression);

                case "Ceil":
                case "Floor":
                    return MqlCeilOrFloorMethodToAggregationExpressionTranslator.Translate(context, expression);

                case "First":
                case "FirstN":
                case "Last":
                case "LastN":
                    return MqlFirstOrLastMethodToAggregationExpressionTranslator.Translate(context, expression);

                case "Ln":
                case "Log":
                case "Log10":
                    return MqlLogMethodToAggregationExpressionTranslator.Translate(context, expression);

                case "RoundToDouble":
                case "RoundToInteger":
                case "TruncToDouble":
                case "TruncToInteger":
                    return MqlRoundOrTruncMethodToAggregationExpressionTranslator.Translate(context, expression);
            }

            throw new MqlExpressionNotSupportedException(expression);
        }
    }
}
