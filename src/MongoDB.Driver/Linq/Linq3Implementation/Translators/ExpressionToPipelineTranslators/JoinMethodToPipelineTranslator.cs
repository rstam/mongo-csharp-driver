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
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Linq.Linq3Implementation.Ast;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Expressions;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Filters;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Stages;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Visitors;
using MongoDB.Driver.Linq.Linq3Implementation.ExtensionMethods;
using MongoDB.Driver.Linq.Linq3Implementation.Misc;
using MongoDB.Driver.Linq.Linq3Implementation.Reflection;
using MongoDB.Driver.Linq.Linq3Implementation.Serializers;
using MongoDB.Driver.Linq.Linq3Implementation.Translators.ExpressionToAggregationExpressionTranslators;

namespace MongoDB.Driver.Linq.Linq3Implementation.Translators.ExpressionToPipelineTranslators
{
    internal static class JoinMethodToPipelineTranslator
    {
        // public static methods
        public static AstPipeline Translate(TranslationContext context, MethodCallExpression expression)
        {
            var method = expression.Method;
            var arguments = expression.Arguments;

            if (method.Is(QueryableMethod.Join))
            {
                var outerExpression = arguments[0];
                var innerExpression = arguments[1];
                var outerKeySelectorLambda = ExpressionHelper.UnquoteLambda(arguments[2]);
                var innerKeySelectorLambda = ExpressionHelper.UnquoteLambda(arguments[3]);
                var resultSelectorLambda = ExpressionHelper.UnquoteLambda(arguments[4]);
                var outerParameter = resultSelectorLambda.Parameters[0];
                var innerParameter = resultSelectorLambda.Parameters[1];

                var pipeline = ExpressionToPipelineTranslator.Translate(context, outerExpression);
                var outerSerializer = pipeline.OutputSerializer;

                var (innerCollectionName, innerSerializer) = innerExpression.GetCollectionInfo(containerExpression: expression);
                var outerKeySelectorTranslation = ExpressionToAggregationExpressionTranslator.TranslateLambdaBody(context, outerKeySelectorLambda, outerSerializer, asRoot: true);
                var innerKeySelectorTranslation = ExpressionToAggregationExpressionTranslator.TranslateLambdaBody(context, innerKeySelectorLambda, innerSerializer, asRoot: true);

                var localFieldPath = GetFieldPath(outerKeySelectorTranslation.Ast);
                var foreignFieldPath = GetFieldPath(innerKeySelectorTranslation.Ast);

                if (resultSelectorLambda.Body == innerParameter &&
                    localFieldPath != null &&
                    foreignFieldPath != null)
                {
                    var lookupStage = AstStage.Lookup(
                        innerCollectionName,
                        localFieldPath,
                        foreignFieldPath,
                        @as: "_v");

                    var projectStage = AstStage.Project(
                        AstProject.Include("_v"),
                        AstProject.Exclude("_id"));

                    var unwindStage = AstStage.Unwind("_v");

                    var resultSerializer = WrappedValueSerializer.Create("_v", innerSerializer);

                    pipeline = pipeline.AddStages(
                        resultSerializer,
                        lookupStage,
                        projectStage,
                        unwindStage);
                }
                else
                {
                    var isCorrelatedSubquery = resultSelectorLambda.LambdaBodyReferencesParameter(outerParameter);
                    var resultPipeline = AstPipeline.Empty(innerSerializer);

                    var targetWireVersion = (context.TranslationOptions?.CompatibilityLevel).ToWireVersion();
                    if (!Feature.ConciseCorrelatedSubqueries.IsSupported(targetWireVersion) ||
                        localFieldPath == null ||
                        foreignFieldPath == null)
                    {
                        isCorrelatedSubquery = true;
                        var localAst = ReplaceRootVarVisitor.ReplaceRootVar(outerKeySelectorTranslation.Ast, AstExpression.Var("outer"));
                        var foreignAst = innerKeySelectorTranslation.Ast;
                        var filter = AstFilter.Expr(AstExpression.Eq(localAst, foreignAst));
                        var matchStage = AstStage.Match(filter);
                        resultPipeline = resultPipeline.AddStages(innerSerializer, matchStage);

                        localFieldPath = null;
                        foreignFieldPath = null;
                    }

                    var outerVar = AstExpression.Var("outer");
                    var outerSymbol = context.CreateSymbol(outerParameter, outerVar, outerSerializer);
                    var innerVar = AstExpression.Var("ROOT", isCurrent: true);
                    var innerSymbol = context.CreateSymbol(innerParameter, innerVar, innerSerializer);
                    var resultSelectorContext = context.WithSymbols(outerSymbol, innerSymbol);
                    var resultSelectorTranslation = ExpressionToAggregationExpressionTranslator.Translate(resultSelectorContext, resultSelectorLambda.Body);
                    var (resultProjectStage, resultSerializer) = ProjectionHelper.CreateProjectStage(resultSelectorTranslation);
                    resultPipeline = resultPipeline.AddStages(resultSerializer, new[] { resultProjectStage });

                    var let = isCorrelatedSubquery ?
                        new[] { AstExpression.ComputedField("outer", AstExpression.Var("ROOT", isCurrent: true)) } :
                        null;

                    var lookupStage = (localFieldPath != null && foreignFieldPath != null) ?
                        AstStage.Lookup(
                            innerCollectionName,
                            localFieldPath,
                            foreignFieldPath,
                            let, // will be null if subquery is uncorrelated
                            pipeline: resultPipeline,
                            @as: "_v")
                        :
                        AstStage.Lookup(
                            innerCollectionName,
                            let, // will be null if subquery is uncorrelated
                            pipeline: resultPipeline,
                            @as: "_v");


                    var projectStage = AstStage.Project(
                        AstProject.Include("_v"),
                        AstProject.Exclude("_id"));

                    var unwindStage = AstStage.Unwind("_v");

                    pipeline = pipeline.AddStages(
                        newOutputSerializer: WrappedValueSerializer.Create("_v", resultSerializer),
                        lookupStage,
                        projectStage,
                        unwindStage);
                }

                return pipeline;
            }

            throw new ExpressionNotSupportedException(expression);
        }

        private static string GetFieldPath(AstExpression expression)
        {
            if (expression.CanBeConvertedToFieldPath())
            {
                var fieldPath = expression.ConvertToFieldPath();
                if (fieldPath.Length >= 2 && fieldPath[0] == '$' && fieldPath[1] != '$')
                {
                    return fieldPath.Substring(1);
                }
            }

            return null;
        }

        private class ReplaceRootVarVisitor : AstNodeVisitor
        {
            public static AstExpression ReplaceRootVar(AstExpression expression, AstExpression replacement)
            {
                var visitor = new ReplaceRootVarVisitor(replacement);
                return visitor.VisitAndConvert(expression);
            }

            private readonly AstExpression _replacement;

            private ReplaceRootVarVisitor(AstExpression replacement)
            {
                _replacement = replacement;
            }

            public override AstNode VisitVarExpression(AstVarExpression node)
                => node.Name == "ROOT" ? _replacement : node;
        }
    }
}
