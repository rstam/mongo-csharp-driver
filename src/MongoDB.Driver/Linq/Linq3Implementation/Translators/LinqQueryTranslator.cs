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

using System.Linq;
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq.Linq3Implementation.Ast;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Optimizers;
using MongoDB.Driver.Linq.Linq3Implementation.Misc;
using MongoDB.Driver.Linq.Linq3Implementation.Translators.ExpressionToPipelineTranslators;

namespace MongoDB.Driver.Linq.Linq3Implementation.Translators
{
    internal static class LinqQueryTranslator
    {
        public static TranslatedPipeline TranslateExpression(
            Expression expression,
            ExpressionTranslationOptions translationOptions)
        {
            expression = PartialEvaluator.EvaluatePartially(expression);
            var context = TranslationContext.Create(translationOptions);
            var unoptimizedPipeline = ExpressionToPipelineTranslator.Translate(context, expression);
            var optimizedAstPipeline = AstPipelineOptimizer.Optimize(unoptimizedPipeline.Ast);
            return new TranslatedPipeline(optimizedAstPipeline, unoptimizedPipeline.OutputSerializer);
        }

        public static BsonDocument[] TranslateExpression(
            Expression expression,
            ExpressionTranslationOptions translationOptions,
            out IBsonSerializer outputSerializer)
        {
            var translatedPipeline = TranslateExpression(expression, translationOptions);
            outputSerializer = translatedPipeline.OutputSerializer;
            return translatedPipeline.Ast.Render().AsBsonArray.Cast<BsonDocument>().ToArray();
        }

        public static TranslatedPipeline TranslateQueryable(
            IQueryable queryable)
        {
            var provider = (IMongoQueryProviderInternal)queryable.Provider;
            var translationOptions = provider.GetTranslationOptions();
            return TranslateExpression(queryable.Expression, translationOptions);
        }

        public static BsonDocument[] TranslateQueryable<TResult>(
            IQueryable<TResult> queryable,
            out IBsonSerializer<TResult> outputSerializer)
        {
            var translatedPipeline = TranslateQueryable(queryable);
            outputSerializer = (IBsonSerializer<TResult>)translatedPipeline.OutputSerializer;
            return translatedPipeline.Ast.Render().AsBsonArray.Cast<BsonDocument>().ToArray();
        }
    }
}
