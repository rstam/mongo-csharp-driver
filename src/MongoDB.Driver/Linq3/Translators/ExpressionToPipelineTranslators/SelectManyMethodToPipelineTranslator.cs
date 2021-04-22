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
using MongoDB.Driver.Linq3.Ast;
using MongoDB.Driver.Linq3.Ast.Expressions;
using MongoDB.Driver.Linq3.Ast.Stages;
using MongoDB.Driver.Linq3.Misc;
using MongoDB.Driver.Linq3.Reflection;
using MongoDB.Driver.Linq3.Serializers;
using MongoDB.Driver.Linq3.Translators.ExpressionToAggregationExpressionTranslators;

namespace MongoDB.Driver.Linq3.Translators.ExpressionToPipelineTranslators
{
    internal static class SelectManyMethodToPipelineTranslator
    {
        // public static methods
        public static AstPipeline Translate(TranslationContext context, MethodCallExpression expression)
        {
            var method = expression.Method;
            var arguments = expression.Arguments;

            var sourceExpression = arguments[0];
            var pipeline = ExpressionToPipelineTranslator.Translate(context, sourceExpression);
            var sourceSerializer = pipeline.OutputSerializer;

            if (method.Is(QueryableMethod.SelectMany))
            {
                var selectorLambda = ExpressionHelper.UnquoteLambda(arguments[1]);
                var selectorTranslation = ExpressionToAggregationExpressionTranslator.TranslateLambdaBody(context, selectorLambda, sourceSerializer, asCurrentSymbol: true);
                var resultValueSerializer = ArraySerializerHelper.GetItemSerializer(selectorTranslation.Serializer);
                var resultWrappedValueSerializer = WrappedValueSerializer.Create(resultValueSerializer);

                pipeline = pipeline.AddStages(
                    resultWrappedValueSerializer,
                    AstStage.Project(
                        AstProject.Set("_v", selectorTranslation.Ast),
                        AstProject.ExcludeId()),
                    AstStage.Unwind("_v"));

                return pipeline;
            }

            if (method.Is(QueryableMethod.SelectManyWithCollectionSelectorAndResultSelector))
            {
                var collectionSelectorLambda = ExpressionHelper.UnquoteLambda(arguments[1]);
                var collectionSelectorTranslation = ExpressionToAggregationExpressionTranslator.TranslateLambdaBody(context, collectionSelectorLambda, sourceSerializer, asCurrentSymbol: true);
                var collectionItemSerializer = ArraySerializerHelper.GetItemSerializer(collectionSelectorTranslation.Serializer);

                var resultSelectorLambda = ExpressionHelper.UnquoteLambda(arguments[2]);
                var resultSelectorSourceParameterExpression = resultSelectorLambda.Parameters[0];
                var resultSelectorCollectionItemParameterExpression = resultSelectorLambda.Parameters[1];

                if (resultSelectorLambda.Body == resultSelectorCollectionItemParameterExpression)
                {
                    // special case identity resultSelector: (x, y) => y
                    var resultValueSerializer = collectionItemSerializer;
                    var resultWrappedValueSerializer = WrappedValueSerializer.Create(resultValueSerializer);

                    pipeline = pipeline.AddStages(
                        resultWrappedValueSerializer,
                        AstStage.Project(
                            AstProject.Set("_v", collectionSelectorTranslation.Ast),
                            AstProject.ExcludeId()),
                        AstStage.Unwind("_v"));
                }
                else
                {
                    var resultSelectorSourceParameterSymbol = new Symbol("$" + resultSelectorSourceParameterExpression.Name, sourceSerializer);
                    var resultSelectorCollectionItemParameterSymbol = new Symbol("$" + resultSelectorCollectionItemParameterExpression.Name, collectionItemSerializer);
                    var resultSelectorContext = context
                        .WithSymbolAsCurrent(resultSelectorSourceParameterExpression, resultSelectorSourceParameterSymbol)
                        .WithSymbol(resultSelectorCollectionItemParameterExpression, resultSelectorCollectionItemParameterSymbol);
                    var resultSelectorTranslation = ExpressionToAggregationExpressionTranslator.Translate(resultSelectorContext, resultSelectorLambda.Body);
                    var resultValueSerializer = resultSelectorTranslation.Serializer;
                    var resultWrappedValueSerializer = WrappedValueSerializer.Create(resultValueSerializer);
                    var resultAst = AstExpression.Map(
                        input: collectionSelectorTranslation.Ast,
                        @as: resultSelectorCollectionItemParameterExpression.Name,
                        @in: resultSelectorTranslation.Ast);

                    pipeline = pipeline.AddStages(
                        resultWrappedValueSerializer,
                        AstStage.Project(
                            AstProject.Set("_v", resultAst),
                            AstProject.ExcludeId()),
                        AstStage.Unwind("_v"));
                }

                return pipeline;
            }

            throw new ExpressionNotSupportedException(expression);
        }
    }
}