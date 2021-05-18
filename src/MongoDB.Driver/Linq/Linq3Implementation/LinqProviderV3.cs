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

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq;
using MongoDB.Driver.Linq.Linq3Implementation.Misc;
using MongoDB.Driver.Linq.Linq3Implementation.Translators;
using MongoDB.Driver.Linq.Linq3Implementation.Translators.ExpressionToAggregationExpressionTranslators;
using MongoDB.Driver.Linq.Linq3Implementation.Translators.ExpressionToFilterTranslators;
using MongoDB.Driver.Linq.Linq3Implementation.Translators.ExpressionToFilterTranslators.ToFilterFieldTranslators;

namespace MongoDB.Driver.Linq.Linq3Implementation
{
    internal sealed class LinqProviderV3 : LinqProvider
    {
        internal override IMongoQueryable<TDocument> AsQueryable<TDocument>(
            IMongoCollection<TDocument> collection,
            IClientSessionHandle session,
            AggregateOptions options,
            CancellationToken cancellationToken)
        {
            var queryProvider = new MongoQueryProvider<TDocument>(collection, session, options, cancellationToken);
            return new MongoQuery<TDocument, TDocument>(queryProvider);
        }

        public override string ToString() => "V3";

        internal override BsonValue TranslateExpressionToAggregateExpression<TSource, TResult>(
            Expression<Func<TSource, TResult>> expression,
            IBsonSerializer<TSource> sourceSerializer,
            IBsonSerializerRegistry serializerRegistry,
            ExpressionTranslationOptions translationOptions)
        {
            var context = new TranslationContext();
            var translation = ExpressionToAggregationExpressionTranslator.TranslateLambdaBody(context, expression, sourceSerializer, asCurrentSymbol: true);

            return translation.Ast.Render();
        }

        internal override RenderedProjectionDefinition<TOutput> TranslateExpressionToBucketOutputProjection<TInput, TValue, TOutput>(
            Expression<Func<TInput, TValue>> valueExpression,
            Expression<Func<IGrouping<TValue, TInput>, TOutput>> outputExpression,
            IBsonSerializer<TInput> documentSerializer,
            IBsonSerializerRegistry serializerRegistry,
            ExpressionTranslationOptions translationOptions)
        {
            throw new NotImplementedException();
        }

        internal override RenderedFieldDefinition TranslateExpressionToField<TDocument>(
            LambdaExpression expression,
            IBsonSerializer<TDocument> documentSerializer,
            IBsonSerializerRegistry serializerRegistry)
        {
            var parameter = expression.Parameters.Single();
            var symbol = new Symbol(parameter.Name, documentSerializer);
            var symbolTable = new SymbolTable().WithSymbolAsCurrent(parameter, symbol);
            var context = new TranslationContext(symbolTable);
            var field = ExpressionToFilterFieldTranslator.Translate(context, expression.Body);

            return new RenderedFieldDefinition(field.Path, field.Serializer);
        }

        internal override RenderedFieldDefinition<TField> TranslateExpressionToField<TDocument, TField>(
            Expression<Func<TDocument, TField>> expression,
            IBsonSerializer<TDocument> documentSerializer,
            IBsonSerializerRegistry serializerRegistry,
            bool allowScalarValueForArrayField)
        {
            var parameter = expression.Parameters.Single();
            var symbol = new Symbol(parameter.Name, documentSerializer);
            var symbolTable = new SymbolTable().WithSymbolAsCurrent(parameter, symbol);
            var context = new TranslationContext(symbolTable);
            var field = ExpressionToFilterFieldTranslator.Translate(context, expression.Body);

            var underlyingSerializer = field.Serializer;
            var fieldSerializer = underlyingSerializer as IBsonSerializer<TField>;
            var valueSerializer = (IBsonSerializer<TField>)FieldValueSerializerHelper.GetSerializerForValueType(underlyingSerializer, serializerRegistry, typeof(TField), allowScalarValueForArrayField);

            return new RenderedFieldDefinition<TField>(field.Path, fieldSerializer, valueSerializer, underlyingSerializer);
        }

        internal override BsonDocument TranslateExpressionToFilter<TDocument>(
            Expression<Func<TDocument, bool>> expression,
            IBsonSerializer<TDocument> documentSerializer,
            IBsonSerializerRegistry serializerRegistry)
        {
            var context = new TranslationContext();
            var filter = ExpressionToFilterTranslator.TranslateLambda(context, expression, documentSerializer);

            return filter.Render().AsBsonDocument;
        }

        internal override RenderedProjectionDefinition<TProjection> TranslateExpressionToFindProjection<TSource, TProjection>(
            Expression<Func<TSource, TProjection>> expression,
            IBsonSerializer<TSource> sourceSerializer,
            IBsonSerializerRegistry serializerRegistry)
        {
            throw new NotImplementedException();
        }

        internal override RenderedProjectionDefinition<TOutput> TranslateExpressionToGroupProjection<TInput, TKey, TOutput>(
            Expression<Func<TInput, TKey>> idExpression,
            Expression<Func<IGrouping<TKey, TInput>, TOutput>> groupExpression,
            IBsonSerializer<TInput> documentSerializer,
            IBsonSerializerRegistry serializerRegistry,
            ExpressionTranslationOptions translationOptions)
        {
            throw new NotImplementedException();
        }

        internal override RenderedProjectionDefinition<TOutput> TranslateExpressionToProjection<TInput, TOutput>(
            Expression<Func<TInput, TOutput>> expression,
            IBsonSerializer<TInput> inputSerializer,
            IBsonSerializerRegistry serializerRegistry,
            ExpressionTranslationOptions translationOptions)
        {
            var context = new TranslationContext();
            var translation = ExpressionToAggregationExpressionTranslator.TranslateLambdaBody(context, expression, inputSerializer, asCurrentSymbol: true);
            var (projectStage, projectionSerializer) = ProjectionHelper.CreateProjectStage(translation);
            var renderedProjection = projectStage.Render().AsBsonDocument["$project"].AsBsonDocument;

            return new RenderedProjectionDefinition<TOutput>(renderedProjection, (IBsonSerializer<TOutput>)projectionSerializer);
        }
    }
}