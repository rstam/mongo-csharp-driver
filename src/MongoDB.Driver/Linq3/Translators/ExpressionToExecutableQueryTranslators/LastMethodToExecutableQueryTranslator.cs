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
using MongoDB.Bson;
using MongoDB.Driver.Linq3.Ast.Expressions;
using MongoDB.Driver.Linq3.Ast.Stages;
using MongoDB.Driver.Linq3.Misc;
using MongoDB.Driver.Linq3.Reflection;
using MongoDB.Driver.Linq3.Translators.ExpressionToExecutableQueryTranslators.Finalizers;
using MongoDB.Driver.Linq3.Translators.ExpressionToPipelineTranslators;

namespace MongoDB.Driver.Linq3.Translators.ExpressionToExecutableQueryTranslators
{
    internal static class LastMethodToExecutableQueryTranslator<TOutput>
    {
        // private static fields
        private static readonly IExecutableQueryFinalizer<TOutput, TOutput> __singleFinalizer = new SingleFinalizer<TOutput>();
        private static readonly IExecutableQueryFinalizer<TOutput, TOutput> __singleOrDefaultFinalizer = new SingleOrDefaultFinalizer<TOutput>();

        // public methods
        public static ExecutableQuery<TDocument, TOutput> Translate<TDocument>(MongoQueryProvider<TDocument> provider, TranslationContext context, MethodCallExpression expression)
        {
            var method = expression.Method;
            var arguments = expression.Arguments;

            if (method.IsOneOf(QueryableMethod.Last, QueryableMethod.LastWithPredicate, QueryableMethod.LastOrDefault, QueryableMethod.LastOrDefaultWithPredicate))
            {
                var sourceExpression = arguments[0];
                if (method.IsOneOf(QueryableMethod.LastWithPredicate, QueryableMethod.LastOrDefaultWithPredicate))
                {
                    var predicateExpression = arguments[1];
                    var tsource = sourceExpression.Type.GetGenericArguments()[0];
                    sourceExpression = Expression.Call(QueryableMethod.MakeWhere(tsource), sourceExpression, predicateExpression);
                }
                var pipeline = ExpressionToPipelineTranslator.Translate(context, sourceExpression);

                pipeline = pipeline.AddStages(
                    pipeline.OutputSerializer,
                    AstStage.Group(
                        id: BsonNull.Value,
                        fields: AstExpression.ComputedField("_last", AstExpression.Last(AstExpression.Field("$ROOT")))));

                var finalizer = method.Name == "LastOrDefault" ? __singleOrDefaultFinalizer : __singleFinalizer;

                return ExecutableQuery.Create(
                    provider.Collection,
                    provider.Options,
                    pipeline,
                    finalizer);
            }

            throw new ExpressionNotSupportedException(expression);
        }
    }
}