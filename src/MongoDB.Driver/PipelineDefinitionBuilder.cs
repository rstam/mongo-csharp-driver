﻿/* Copyright 2016-present MongoDB Inc.
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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.GeoJsonObjectModel;
using MongoDB.Driver.Search;

namespace MongoDB.Driver
{
    /// <summary>
    /// Extension methods for adding stages to a pipeline.
    /// </summary>
    public static class PipelineDefinitionBuilder
    {
        /// <summary>
        /// Appends a stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="stage">The stage.</param>
        /// <param name="outputSerializer">The output serializer.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> AppendStage<TInput, TIntermediate, TOutput>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            PipelineStageDefinition<TIntermediate, TOutput> stage,
            IBsonSerializer<TOutput> outputSerializer = null)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            Ensure.IsNotNull(stage, nameof(stage));
            return new AppendedStagePipelineDefinition<TInput, TIntermediate, TOutput>(pipeline, stage, outputSerializer);
        }

        /// <summary>
        /// Changes the output type of the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="outputSerializer">The output serializer.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> As<TInput, TIntermediate, TOutput>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            IBsonSerializer<TOutput> outputSerializer = null)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return new ReplaceOutputSerializerPipelineDefinition<TInput, TIntermediate, TOutput>(pipeline, outputSerializer);
        }

        /// <summary>
        /// Appends a $bucket stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="groupBy">The group by expression.</param>
        /// <param name="boundaries">The boundaries.</param>
        /// <param name="options">The options.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, AggregateBucketResult<TValue>> Bucket<TInput, TIntermediate, TValue>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            AggregateExpressionDefinition<TIntermediate, TValue> groupBy,
            IEnumerable<TValue> boundaries,
            AggregateBucketOptions<TValue> options = null)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Bucket(groupBy, boundaries, options));
        }

        /// <summary>
        /// Appends a $bucket stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="groupBy">The group by expression.</param>
        /// <param name="boundaries">The boundaries.</param>
        /// <param name="output">The output projection.</param>
        /// <param name="options">The options.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> Bucket<TInput, TIntermediate, TValue, TOutput>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            AggregateExpressionDefinition<TIntermediate, TValue> groupBy,
            IEnumerable<TValue> boundaries,
            ProjectionDefinition<TIntermediate, TOutput> output,
            AggregateBucketOptions<TValue> options = null)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Bucket(groupBy, boundaries, output, options));
        }

        /// <summary>
        /// Appends a $bucket stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="groupBy">The group by expression.</param>
        /// <param name="boundaries">The boundaries.</param>
        /// <param name="options">The options.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, AggregateBucketResult<TValue>> Bucket<TInput, TIntermediate, TValue>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            Expression<Func<TIntermediate, TValue>> groupBy,
            IEnumerable<TValue> boundaries,
            AggregateBucketOptions<TValue> options = null)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Bucket(groupBy, boundaries, options));
        }

        /// <summary>
        /// Appends a $bucket stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="groupBy">The group by expression.</param>
        /// <param name="boundaries">The boundaries.</param>
        /// <param name="output">The output projection.</param>
        /// <param name="options">The options.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> Bucket<TInput, TIntermediate, TValue, TOutput>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            Expression<Func<TIntermediate, TValue>> groupBy,
            IEnumerable<TValue> boundaries,
            Expression<Func<IGrouping<TValue, TIntermediate>, TOutput>> output,
            AggregateBucketOptions<TValue> options = null)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Bucket(groupBy, boundaries, output, options));
        }

        /// <summary>
        /// Appends a $bucketAuto stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="groupBy">The group by expression.</param>
        /// <param name="buckets">The number of buckets.</param>
        /// <param name="options">The options.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, AggregateBucketAutoResult<TValue>> BucketAuto<TInput, TIntermediate, TValue>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            AggregateExpressionDefinition<TIntermediate, TValue> groupBy,
            int buckets,
            AggregateBucketAutoOptions options = null)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.BucketAuto(groupBy, buckets, options));
        }

        /// <summary>
        /// Appends a $bucketAuto stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="groupBy">The group by expression.</param>
        /// <param name="buckets">The number of buckets.</param>
        /// <param name="output">The output projection.</param>
        /// <param name="options">The options.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> BucketAuto<TInput, TIntermediate, TValue, TOutput>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            AggregateExpressionDefinition<TIntermediate, TValue> groupBy,
            int buckets,
            ProjectionDefinition<TIntermediate, TOutput> output,
            AggregateBucketAutoOptions options = null)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.BucketAuto(groupBy, buckets, output, options));
        }

        /// <summary>
        /// Appends a $bucketAuto stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="groupBy">The group by expression.</param>
        /// <param name="buckets">The number of buckets.</param>
        /// <param name="options">The options (optional).</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, AggregateBucketAutoResult<TValue>> BucketAuto<TInput, TIntermediate, TValue>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            Expression<Func<TIntermediate, TValue>> groupBy,
            int buckets,
            AggregateBucketAutoOptions options = null)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.BucketAuto(groupBy, buckets, options));
        }

        /// <summary>
        /// Appends a $bucketAuto stage to the pipeline (this overload can only be used with LINQ3).
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="groupBy">The group by expression.</param>
        /// <param name="buckets">The number of buckets.</param>
        /// <param name="output">The output projection.</param>
        /// <param name="options">The options (optional).</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> BucketAuto<TInput, TIntermediate, TValue, TOutput>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            Expression<Func<TIntermediate, TValue>> groupBy,
            int buckets,
            Expression<Func<IGrouping<AggregateBucketAutoResultId<TValue>, TIntermediate>, TOutput>> output,
            AggregateBucketAutoOptions options = null)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.BucketAuto(groupBy, buckets, output, options));
        }

        /// <summary>
        /// Appends a $changeStream stage to the pipeline.
        /// Normally you would prefer to use the Watch method of <see cref="IMongoCollection{TDocument}" />.
        /// Only use this method if subsequent stages project away the resume token (the _id)
        /// or you don't want the resulting cursor to automatically resume.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="options">The options.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, ChangeStreamDocument<TIntermediate>> ChangeStream<TInput, TIntermediate>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            ChangeStreamStageOptions options = null)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.ChangeStream<TIntermediate>(options));
        }

        /// <summary>
        /// Appends a $changeStreamSplitLargeEvent stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<ChangeStreamDocument<TInput>, ChangeStreamDocument<TInput>> ChangeStreamSplitLargeEvent<TInput>(
           this PipelineDefinition<ChangeStreamDocument<TInput>, ChangeStreamDocument<TInput>> pipeline)
        {
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.ChangeStreamSplitLargeEvent<TInput>());
        }

        /// <summary>
        /// Appends a $count stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, AggregateCountResult> Count<TInput, TIntermediate>(
            this PipelineDefinition<TInput, TIntermediate> pipeline)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Count<TIntermediate>());
        }

        /// <summary>
        /// Appends a $densify stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="field">The field.</param>
        /// <param name="range">The range.</param>
        /// <param name="partitionByFields">The partition by fields.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> Densify<TInput, TOutput>(
            this PipelineDefinition<TInput, TOutput> pipeline,
            FieldDefinition<TOutput> field,
            DensifyRange range,
            IEnumerable<FieldDefinition<TOutput>> partitionByFields = null)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Densify(field, range, partitionByFields));
        }

        /// <summary>
        /// Appends a $densify stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="field">The field.</param>
        /// <param name="range">The range.</param>
        /// <param name="partitionByFields">The partition by fields.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> Densify<TInput, TOutput>(
            this PipelineDefinition<TInput, TOutput> pipeline,
            FieldDefinition<TOutput> field,
            DensifyRange range,
            params FieldDefinition<TOutput>[] partitionByFields)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Densify(field, range, partitionByFields));
        }

        /// <summary>
        /// Appends a $densify stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="field">The field.</param>
        /// <param name="range">The range.</param>
        /// <param name="partitionByFields">The partition by fields.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> Densify<TInput, TOutput>(
            this PipelineDefinition<TInput, TOutput> pipeline,
            Expression<Func<TOutput, object>> field,
            DensifyRange range,
            IEnumerable<Expression<Func<TOutput, object>>> partitionByFields = null)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Densify(field, range, partitionByFields));
        }

        /// <summary>
        /// Appends a $densify stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="field">The field.</param>
        /// <param name="range">The range.</param>
        /// <param name="partitionByFields">The partition by fields.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> Densify<TInput, TOutput>(
            this PipelineDefinition<TInput, TOutput> pipeline,
            Expression<Func<TOutput, object>> field,
            DensifyRange range,
            params Expression<Func<TOutput, object>>[] partitionByFields)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Densify(field, range, partitionByFields));
        }

        /// <summary>
        /// Appends a $documents stage to the pipeline.
        /// </summary>
        /// <typeparam name="TDocument">The type of the documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="documents">The documents.</param>
        /// <param name="documentSerializer">The document serializer.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<NoPipelineInput, TDocument> Documents<TDocument>(
            this PipelineDefinition<NoPipelineInput, NoPipelineInput> pipeline,
            AggregateExpressionDefinition<NoPipelineInput, IEnumerable<TDocument>> documents,
            IBsonSerializer<TDocument> documentSerializer = null)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Documents(documents, documentSerializer));
        }

        /// <summary>
        /// Appends a $documents stage to the pipeline.
        /// </summary>
        /// <typeparam name="TDocument">The type of the documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="documents">The documents.</param>
        /// <param name="documentSerializer">The document serializer.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<NoPipelineInput, TDocument> Documents<TDocument>(
            this PipelineDefinition<NoPipelineInput, NoPipelineInput> pipeline,
            IEnumerable<TDocument> documents,
            IBsonSerializer<TDocument> documentSerializer = null)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Documents(documents, documentSerializer));
        }

        /// <summary>
        /// Appends a $facet stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="facets">The facets.</param>
        /// <param name="options">The options.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> Facet<TInput, TIntermediate, TOutput>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            IEnumerable<AggregateFacet<TIntermediate>> facets,
            AggregateFacetOptions<TOutput> options = null)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Facet(facets, options));
        }

        /// <summary>
        /// Appends a $facet stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="facets">The facets.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, AggregateFacetResults> Facet<TInput, TIntermediate>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            IEnumerable<AggregateFacet<TIntermediate>> facets)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Facet(facets));
        }

        /// <summary>
        /// Appends a $facet stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="facets">The facets.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, AggregateFacetResults> Facet<TInput, TIntermediate>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            params AggregateFacet<TIntermediate>[] facets)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Facet(facets));
        }

        /// <summary>
        /// Appends a $facet stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="facets">The facets.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> Facet<TInput, TIntermediate, TOutput>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            params AggregateFacet<TIntermediate>[] facets)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Facet<TIntermediate, TOutput>(facets));
        }

        /// <summary>
        /// Used to start creating a pipeline for {TInput} documents.
        /// </summary>
        /// <typeparam name="TInput">The type of the output.</typeparam>
        /// <param name="inputSerializer">The inputSerializer serializer.</param>
        /// <returns>An empty pipeline.</returns>
        public static PipelineDefinition<TInput, TInput> For<TInput>(IBsonSerializer<TInput> inputSerializer = null)
        {
            return new EmptyPipelineDefinition<TInput>(inputSerializer);
        }

        /// <summary>
        /// Appends a $geoNear stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <typeparam name="TCoordinates">The type of the coordinates for the point.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="near">The point for which to find the closest documents.</param>
        /// <param name="options">The options.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> GeoNear<TInput, TIntermediate, TCoordinates, TOutput>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            GeoJsonPoint<TCoordinates> near,
            GeoNearOptions<TIntermediate, TOutput> options = null)
            where TCoordinates : GeoJsonCoordinates
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.GeoNear(near, options));
        }

        /// <summary>
        /// Appends a $geoNear stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="near">The point for which to find the closest documents.</param>
        /// <param name="options">The options.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> GeoNear<TInput, TIntermediate, TOutput>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            double[] near,
            GeoNearOptions<TIntermediate, TOutput> options = null)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.GeoNear(near, options));
        }

        /// <summary>
        /// Appends a $graphLookup stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TFrom">The type of the from documents.</typeparam>
        /// <typeparam name="TConnectFrom">The type of the connect from field (must be either TConnectTo or a type that implements IEnumerable{TConnectTo}).</typeparam>
        /// <typeparam name="TConnectTo">The type of the connect to field.</typeparam>
        /// <typeparam name="TStartWith">The type of the start with expression (must be either TConnectTo or a type that implements IEnumerable{TConnectTo}).</typeparam>
        /// <typeparam name="TAsElement">The type of the as field elements.</typeparam>
        /// <typeparam name="TAs">The type of the as field.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="from">The from collection.</param>
        /// <param name="connectFromField">The connect from field.</param>
        /// <param name="connectToField">The connect to field.</param>
        /// <param name="startWith">The start with value.</param>
        /// <param name="as">The as field.</param>
        /// <param name="depthField">The depth field.</param>
        /// <param name="options">The options.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> GraphLookup<TInput, TIntermediate, TFrom, TConnectFrom, TConnectTo, TStartWith, TAsElement, TAs, TOutput>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            IMongoCollection<TFrom> from,
            FieldDefinition<TFrom, TConnectFrom> connectFromField,
            FieldDefinition<TFrom, TConnectTo> connectToField,
            AggregateExpressionDefinition<TIntermediate, TStartWith> startWith,
            FieldDefinition<TOutput, TAs> @as,
            FieldDefinition<TAsElement, int> depthField,
            AggregateGraphLookupOptions<TFrom, TAsElement, TOutput> options = null)
                where TAs : IEnumerable<TAsElement>
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.GraphLookup(from, connectFromField, connectToField, startWith, @as, depthField, options));
        }

        /// <summary>
        /// Appends a $graphLookup stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TFrom">The type of the from documents.</typeparam>
        /// <typeparam name="TConnectFrom">The type of the connect from field (must be either TConnectTo or a type that implements IEnumerable{TConnectTo}).</typeparam>
        /// <typeparam name="TConnectTo">The type of the connect to field.</typeparam>
        /// <typeparam name="TStartWith">The type of the start with expression (must be either TConnectTo or a type that implements IEnumerable{TConnectTo}).</typeparam>
        /// <typeparam name="TAs">The type of the as field.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="from">The from collection.</param>
        /// <param name="connectFromField">The connect from field.</param>
        /// <param name="connectToField">The connect to field.</param>
        /// <param name="startWith">The start with value.</param>
        /// <param name="as">The as field.</param>
        /// <param name="options">The options.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> GraphLookup<TInput, TIntermediate, TFrom, TConnectFrom, TConnectTo, TStartWith, TAs, TOutput>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            IMongoCollection<TFrom> from,
            FieldDefinition<TFrom, TConnectFrom> connectFromField,
            FieldDefinition<TFrom, TConnectTo> connectToField,
            AggregateExpressionDefinition<TIntermediate, TStartWith> startWith,
            FieldDefinition<TOutput, TAs> @as,
            AggregateGraphLookupOptions<TFrom, TFrom, TOutput> options = null)
                where TAs : IEnumerable<TFrom>
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.GraphLookup(from, connectFromField, connectToField, startWith, @as, options));
        }

        /// <summary>
        /// Appends a $graphLookup stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TFrom">The type of the from documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="from">The from collection.</param>
        /// <param name="connectFromField">The connect from field.</param>
        /// <param name="connectToField">The connect to field.</param>
        /// <param name="startWith">The start with value.</param>
        /// <param name="as">The as field.</param>
        /// <param name="depthField">The depth field.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, BsonDocument> GraphLookup<TInput, TIntermediate, TFrom>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            IMongoCollection<TFrom> from,
            FieldDefinition<TFrom, BsonValue> connectFromField,
            FieldDefinition<TFrom, BsonValue> connectToField,
            AggregateExpressionDefinition<TIntermediate, BsonValue> startWith,
            FieldDefinition<BsonDocument, IEnumerable<BsonDocument>> @as,
            FieldDefinition<BsonDocument, int> depthField = null)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.GraphLookup(from, connectFromField, connectToField, startWith, @as, depthField));
        }

        /// <summary>
        /// Appends a $graphLookup stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TFrom">The type of the from documents.</typeparam>
        /// <typeparam name="TConnectFrom">The type of the connect from field (must be either TConnectTo or a type that implements IEnumerable{TConnectTo}).</typeparam>
        /// <typeparam name="TConnectTo">The type of the connect to field.</typeparam>
        /// <typeparam name="TStartWith">The type of the start with expression (must be either TConnectTo or a type that implements IEnumerable{TConnectTo}).</typeparam>
        /// <typeparam name="TAs">The type of the as field.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="from">The from collection.</param>
        /// <param name="connectFromField">The connect from field.</param>
        /// <param name="connectToField">The connect to field.</param>
        /// <param name="startWith">The start with value.</param>
        /// <param name="as">The as field.</param>
        /// <param name="options">The options.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> GraphLookup<TInput, TIntermediate, TFrom, TConnectFrom, TConnectTo, TStartWith, TAs, TOutput>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            IMongoCollection<TFrom> from,
            Expression<Func<TFrom, TConnectFrom>> connectFromField,
            Expression<Func<TFrom, TConnectTo>> connectToField,
            Expression<Func<TIntermediate, TStartWith>> startWith,
            Expression<Func<TOutput, TAs>> @as,
            AggregateGraphLookupOptions<TFrom, TFrom, TOutput> options = null)
                where TAs : IEnumerable<TFrom>
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.GraphLookup(from, connectFromField, connectToField, startWith, @as, options));
        }

        /// <summary>
        /// Appends a $graphLookup stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TFrom">The type of the from documents.</typeparam>
        /// <typeparam name="TConnectFrom">The type of the connect from field (must be either TConnectTo or a type that implements IEnumerable{TConnectTo}).</typeparam>
        /// <typeparam name="TConnectTo">The type of the connect to field.</typeparam>
        /// <typeparam name="TStartWith">The type of the start with expression (must be either TConnectTo or a type that implements IEnumerable{TConnectTo}).</typeparam>
        /// <typeparam name="TAsElement">The type of the as field elements.</typeparam>
        /// <typeparam name="TAs">The type of the as field.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="from">The from collection.</param>
        /// <param name="connectFromField">The connect from field.</param>
        /// <param name="connectToField">The connect to field.</param>
        /// <param name="startWith">The start with value.</param>
        /// <param name="as">The as field.</param>
        /// <param name="depthField">The depth field.</param>
        /// <param name="options">The options.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> GraphLookup<TInput, TIntermediate, TFrom, TConnectFrom, TConnectTo, TStartWith, TAsElement, TAs, TOutput>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            IMongoCollection<TFrom> from,
            Expression<Func<TFrom, TConnectFrom>> connectFromField,
            Expression<Func<TFrom, TConnectTo>> connectToField,
            Expression<Func<TIntermediate, TStartWith>> startWith,
            Expression<Func<TOutput, TAs>> @as,
            Expression<Func<TAsElement, int>> depthField,
            AggregateGraphLookupOptions<TFrom, TAsElement, TOutput> options = null)
                where TAs : IEnumerable<TAsElement>
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.GraphLookup(from, connectFromField, connectToField, startWith, @as, depthField, options));
        }

        /// <summary>
        /// Appends a $group stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="group">The group projection.</param>
        /// <returns>A new pipeline with an additional stage.</returns>s
        public static PipelineDefinition<TInput, TOutput> Group<TInput, TIntermediate, TOutput>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            ProjectionDefinition<TIntermediate, TOutput> group)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Group(group));
        }

        /// <summary>
        /// Appends a group stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="group">The group projection.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, BsonDocument> Group<TInput, TIntermediate>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            ProjectionDefinition<TIntermediate, BsonDocument> group)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Group(group));
        }

        /// <summary>
        /// Appends a group stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="id">The id.</param>
        /// <param name="group">The group projection.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> Group<TInput, TIntermediate, TKey, TOutput>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            Expression<Func<TIntermediate, TKey>> id,
            Expression<Func<IGrouping<TKey, TIntermediate>, TOutput>> group)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Group(id, group));
        }

        /// <summary>
        /// Appends a $limit stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="limit">The limit.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> Limit<TInput, TOutput>(
            this PipelineDefinition<TInput, TOutput> pipeline,
            long limit)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Limit<TOutput>(limit));
        }

        /// <summary>
        /// Appends a $lookup stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TForeignDocument">The type of the foreign collection documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="foreignCollection">The foreign collection.</param>
        /// <param name="localField">The local field.</param>
        /// <param name="foreignField">The foreign field.</param>
        /// <param name="as">The "as" field.</param>
        /// <param name="options">The options.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> Lookup<TInput, TIntermediate, TForeignDocument, TOutput>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            IMongoCollection<TForeignDocument> foreignCollection,
            FieldDefinition<TIntermediate> localField,
            FieldDefinition<TForeignDocument> foreignField,
            FieldDefinition<TOutput> @as,
            AggregateLookupOptions<TForeignDocument, TOutput> options = null)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Lookup(foreignCollection, localField, foreignField, @as, options));
        }

        /// <summary>
        /// Appends a lookup stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TForeignDocument">The type of the foreign collection documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="foreignCollection">The foreign collection.</param>
        /// <param name="localField">The local field.</param>
        /// <param name="foreignField">The foreign field.</param>
        /// <param name="as">The "as" field.</param>
        /// <param name="options">The options.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> Lookup<TInput, TIntermediate, TForeignDocument, TOutput>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            IMongoCollection<TForeignDocument> foreignCollection,
            Expression<Func<TIntermediate, object>> localField,
            Expression<Func<TForeignDocument, object>> foreignField,
            Expression<Func<TOutput, object>> @as,
            AggregateLookupOptions<TForeignDocument, TOutput> options = null)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Lookup(foreignCollection, localField, foreignField, @as, options));
        }

        /// <summary>
        /// Appends a $lookup stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TForeignDocument">The type of the foreign collection documents.</typeparam>
        /// <typeparam name="TAsElement">The type of the as field elements.</typeparam>
        /// <typeparam name="TAs">The type of the as field.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="foreignCollection">The foreign collection.</param>
        /// <param name="let">The "let" definition.</param>
        /// <param name="lookupPipeline">The lookup pipeline.</param>
        /// <param name="as">The as field in <typeparamref name="TOutput" /> in which to place the results of the lookup pipeline.</param>
        /// <param name="options">The options.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> Lookup<TInput, TIntermediate, TForeignDocument, TAsElement, TAs, TOutput>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            IMongoCollection<TForeignDocument> foreignCollection,
            BsonDocument let,
            PipelineDefinition<TForeignDocument, TAsElement> lookupPipeline,
            FieldDefinition<TOutput, TAs> @as,
            AggregateLookupOptions<TForeignDocument, TOutput> options = null)
            where TAs : IEnumerable<TAsElement>
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Lookup<TIntermediate, TForeignDocument, TAsElement, TAs, TOutput>(
                foreignCollection,
                let,
                lookupPipeline,
                @as,
                options
            ));
        }

        /// <summary>
        /// Appends a $lookup stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TForeignDocument">The type of the foreign collection documents.</typeparam>
        /// <typeparam name="TAsElement">The type of the as field elements.</typeparam>
        /// <typeparam name="TAs">The type of the as field.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="foreignCollection">The foreign collection.</param>
        /// <param name="let">The "let" definition.</param>
        /// <param name="lookupPipeline">The lookup pipeline.</param>
        /// <param name="as">The as field in <typeparamref name="TOutput" /> in which to place the results of the lookup pipeline.</param>
        /// <param name="options">The options.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> Lookup<TInput, TIntermediate, TForeignDocument, TAsElement, TAs, TOutput>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            IMongoCollection<TForeignDocument> foreignCollection,
            BsonDocument let,
            PipelineDefinition<TForeignDocument, TAsElement> lookupPipeline,
            Expression<Func<TOutput, TAs>> @as,
            AggregateLookupOptions<TForeignDocument, TOutput> options = null)
            where TAs : IEnumerable<TAsElement>
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Lookup<TIntermediate, TForeignDocument, TAsElement, TAs, TOutput>(
                foreignCollection,
                let,
                lookupPipeline,
                @as,
                options
            ));
        }

        /// <summary>
        /// Appends a $match stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="filter">The filter.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> Match<TInput, TOutput>(
            this PipelineDefinition<TInput, TOutput> pipeline,
            FilterDefinition<TOutput> filter)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Match(filter));
        }

        /// <summary>
        /// Appends a match stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="filter">The filter.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> Match<TInput, TOutput>(
            this PipelineDefinition<TInput, TOutput> pipeline,
            Expression<Func<TOutput, bool>> filter)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Match(filter));
        }

        /// <summary>
        /// Appends a $match stage to the pipeline to select documents of a certain type.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="outputSerializer">The output serializer.</param>
        /// <returns>A new pipeline with an additional stage.</returns>>
        /// <exception cref="System.NotSupportedException"></exception>
        public static PipelineDefinition<TInput, TOutput> OfType<TInput, TIntermediate, TOutput>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            IBsonSerializer<TOutput> outputSerializer = null)
                where TOutput : TIntermediate
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.OfType<TIntermediate, TOutput>(outputSerializer));
        }

        /// <summary>
        /// Appends a $merge stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="outputCollection">The output collection.</param>
        /// <param name="mergeOptions">The merge options.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public static PipelineDefinition<TInput, TOutput> Merge<TInput, TIntermediate, TOutput>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            IMongoCollection<TOutput> outputCollection,
            MergeStageOptions<TOutput> mergeOptions)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Merge<TIntermediate, TOutput>(outputCollection, mergeOptions));
        }

        /// <summary>
        /// Appends a $out stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="outputCollection">The output collection.</param>
        /// <param name="timeSeriesOptions">The time series options.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public static PipelineDefinition<TInput, TOutput> Out<TInput, TOutput>(
            this PipelineDefinition<TInput, TOutput> pipeline,
            IMongoCollection<TOutput> outputCollection,
            TimeSeriesOptions timeSeriesOptions = null)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Out<TOutput>(outputCollection, timeSeriesOptions));
        }

        /// <summary>
        /// Appends a $project stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="projection">The projection.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public static PipelineDefinition<TInput, TOutput> Project<TInput, TIntermediate, TOutput>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            ProjectionDefinition<TIntermediate, TOutput> projection)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Project(projection));
        }

        /// <summary>
        /// Appends a project stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="projection">The projection.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, BsonDocument> Project<TInput, TIntermediate>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            ProjectionDefinition<TIntermediate, BsonDocument> projection)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Project(projection));
        }

        /// <summary>
        /// Appends a project stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="projection">The projection.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> Project<TInput, TIntermediate, TOutput>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            Expression<Func<TIntermediate, TOutput>> projection)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Project(projection));
        }

        /// <summary>
        /// Appends a $rankFusion stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="pipelines">The map of named pipelines whose results will be combined. The pipelines must operate on the same collection.</param>
        /// <param name="weights">The map of pipeline names to non-negative numerical weights determining result importance during combination. Default weight is 1 when unspecified.</param>
        /// <param name="options">The rankFusion options.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> RankFusion<TInput, TIntermediate, TOutput>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            Dictionary<string, PipelineDefinition<TIntermediate, TOutput>> pipelines,
            Dictionary<string, double> weights = null,
            RankFusionOptions<TOutput> options = null)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.RankFusion(pipelines, weights, options));
        }

        /// <summary>
        /// Appends a $rankFusion stage to the pipeline. Pipelines will be automatically named as "pipeline1", "pipeline2", etc.
        /// </summary>
        /// <typeparam name="TInput">The type of the documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="pipelines">The collection of pipelines whose results will be combined. The pipelines must operate on the same collection.</param>
        /// <param name="options">The rankFusion options.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> RankFusion<TInput, TIntermediate, TOutput>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            PipelineDefinition<TIntermediate, TOutput>[] pipelines,
            RankFusionOptions<TOutput> options = null)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.RankFusion(pipelines, options));
        }

        /// <summary>
        /// Appends a $rankFusion stage to the pipeline. Pipelines will be automatically named as "pipeline1", "pipeline2", etc.
        /// </summary>
        /// <typeparam name="TInput">The type of the documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="pipelinesWithWeights">The collection of tuples containing (pipeline, weight) pairs. The pipelines must operate on the same collection.</param>
        /// <param name="options">The rankFusion options.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> RankFusion<TInput, TIntermediate, TOutput>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            (PipelineDefinition<TIntermediate, TOutput>, double?)[] pipelinesWithWeights,
            RankFusionOptions<TOutput> options = null)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.RankFusion(pipelinesWithWeights, options));
        }

        /// <summary>
        /// Appends a $replaceRoot stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="newRoot">The new root.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> ReplaceRoot<TInput, TIntermediate, TOutput>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            AggregateExpressionDefinition<TIntermediate, TOutput> newRoot)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.ReplaceRoot(newRoot));
        }

        /// <summary>
        /// Appends a $replaceRoot stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="newRoot">The new root.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> ReplaceRoot<TInput, TIntermediate, TOutput>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            Expression<Func<TIntermediate, TOutput>> newRoot)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.ReplaceRoot(newRoot));
        }

        /// <summary>
        /// Appends a $replaceWith stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="newRoot">The new root.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> ReplaceWith<TInput, TIntermediate, TOutput>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            AggregateExpressionDefinition<TIntermediate, TOutput> newRoot)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.ReplaceWith(newRoot));
        }

        /// <summary>
        /// Appends a $replaceWith stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="newRoot">The new root.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> ReplaceWith<TInput, TIntermediate, TOutput>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            Expression<Func<TIntermediate, TOutput>> newRoot)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.ReplaceWith(newRoot));
        }

        /// <summary>
        /// Appends a $sample stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="size">The sample size.</param>
        /// <returns>
        /// A new pipeline with an additional stage.
        /// </returns>
        public static PipelineDefinition<TInput, TOutput> Sample<TInput, TOutput>(
            this PipelineDefinition<TInput, TOutput> pipeline,
            long size)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Sample<TOutput>(size));
        }

        /// <summary>
        /// Appends a $search stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="searchDefinition">The search definition.</param>
        /// <param name="highlight">The highlight options.</param>
        /// <param name="indexName">The index name.</param>
        /// <param name="count">The count options.</param>
        /// <param name="returnStoredSource">
        /// Flag that specifies whether to perform a full document lookup on the backend database
        /// or return only stored source fields directly from Atlas Search.
        /// </param>
        /// <param name="scoreDetails">
        /// Flag that specifies whether to return a detailed breakdown
        /// of the score for each document in the result.
        /// </param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> Search<TInput, TOutput>(
            this PipelineDefinition<TInput, TOutput> pipeline,
            SearchDefinition<TOutput> searchDefinition,
            SearchHighlightOptions<TOutput> highlight = null,
            string indexName = null,
            SearchCountOptions count = null,
            bool returnStoredSource = false,
            bool scoreDetails = false)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Search(searchDefinition, highlight, indexName, count, returnStoredSource, scoreDetails));
        }

        /// <summary>
        /// Appends a $search stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="searchDefinition">The search definition.</param>
        /// <param name="searchOptions">The search options.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> Search<TInput, TOutput>(
            this PipelineDefinition<TInput, TOutput> pipeline,
            SearchDefinition<TOutput> searchDefinition,
            SearchOptions<TOutput> searchOptions)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Search(searchDefinition, searchOptions));
        }

        /// <summary>
        /// Appends a $searchMeta stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="query">The search definition.</param>
        /// <param name="indexName">The index name.</param>
        /// <param name="count">The count options.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, SearchMetaResult> SearchMeta<TInput, TOutput>(
            this PipelineDefinition<TInput, TOutput> pipeline,
            SearchDefinition<TOutput> query,
            string indexName = null,
            SearchCountOptions count = null)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.SearchMeta(query, indexName, count));
        }

        /// <summary>
        /// Appends a $set stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="fields">The fields to set.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> Set<TInput, TOutput>(
           this PipelineDefinition<TInput, TOutput> pipeline,
           SetFieldDefinitions<TOutput> fields)
        {
            Ensure.IsNotNull(fields, nameof(fields));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Set(fields));
        }

        /// <summary>
        /// Appends a $set stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <typeparam name="TFields">The type of object specifying the fields to set.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="fields">The fields to set.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> Set<TInput, TOutput, TFields>(
           this PipelineDefinition<TInput, TOutput> pipeline,
           Expression<Func<TOutput, TFields>> fields)
        {
            Ensure.IsNotNull(fields, nameof(fields));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Set(fields));
        }

        /// <summary>
        /// Create a $setWindowFields stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TWindowFields">The type of the added window fields.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="output">The window fields expression.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, BsonDocument> SetWindowFields<TInput, TIntermediate, TWindowFields>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            AggregateExpressionDefinition<ISetWindowFieldsPartition<TIntermediate>, TWindowFields> output)
        {
            Ensure.IsNotNull(output, nameof(output));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.SetWindowFields(output));
        }

        /// <summary>
        /// Appends a $setWindowFields stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TPartitionBy">The type of the value to partition by.</typeparam>
        /// <typeparam name="TWindowFields">The type of the added window fields.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="partitionBy">The partitionBy expression.</param>
        /// <param name="output">The window fields expression.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, BsonDocument> SetWindowFields<TInput, TIntermediate, TPartitionBy, TWindowFields>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            AggregateExpressionDefinition<TIntermediate, TPartitionBy> partitionBy,
            AggregateExpressionDefinition<ISetWindowFieldsPartition<TIntermediate>, TWindowFields> output)
        {
            Ensure.IsNotNull(partitionBy, nameof(partitionBy));
            Ensure.IsNotNull(output, nameof(output));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.SetWindowFields(partitionBy, output));
        }

        /// <summary>
        /// Appends a $setWindowFields stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TPartitionBy">The type of the value to partition by.</typeparam>
        /// <typeparam name="TWindowFields">The type of the added window fields.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="partitionBy">The partitionBy expression.</param>
        /// <param name="sortBy">The sortBy expression.</param>
        /// <param name="output">The window fields expression.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, BsonDocument> SetWindowFields<TInput, TIntermediate, TPartitionBy, TWindowFields>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            AggregateExpressionDefinition<TIntermediate, TPartitionBy> partitionBy,
            SortDefinition<TIntermediate> sortBy,
            AggregateExpressionDefinition<ISetWindowFieldsPartition<TIntermediate>, TWindowFields> output)
        {
            Ensure.IsNotNull(partitionBy, nameof(partitionBy));
            Ensure.IsNotNull(sortBy, nameof(sortBy));
            Ensure.IsNotNull(output, nameof(output));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.SetWindowFields(partitionBy, sortBy, output));
        }

        /// <summary>
        /// Appends a $setWindowFields stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TPartitionBy">The type of the value to partition by.</typeparam>
        /// <typeparam name="TWindowFields">The type of the added window fields.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="output">The window fields expression.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, BsonDocument> SetWindowFields<TInput, TIntermediate, TPartitionBy, TWindowFields>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            Expression<Func<ISetWindowFieldsPartition<TIntermediate>, TWindowFields>> output)
        {
            Ensure.IsNotNull(output, nameof(output));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.SetWindowFields(output));
        }

        /// <summary>
        /// Appends a $setWindowFields stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TPartitionBy">The type of the value to partition by.</typeparam>
        /// <typeparam name="TWindowFields">The type of the added window fields.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="partitionBy">The partitionBy expression.</param>
        /// <param name="output">The window fields expression.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, BsonDocument> SetWindowFields<TInput, TIntermediate, TPartitionBy, TWindowFields>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            Expression<Func<TIntermediate, TPartitionBy>> partitionBy,
            Expression<Func<ISetWindowFieldsPartition<TIntermediate>, TWindowFields>> output)
        {
            Ensure.IsNotNull(partitionBy, nameof(partitionBy));
            Ensure.IsNotNull(output, nameof(output));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.SetWindowFields(partitionBy, output));
        }

        /// <summary>
        /// Appends a $setWindowFields stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TPartitionBy">The type of the value to partition by.</typeparam>
        /// <typeparam name="TWindowFields">The type of the added window fields.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="partitionBy">The partitionBy expression.</param>
        /// <param name="sortBy">The sortBy expression.</param>
        /// <param name="output">The window fields expression.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, BsonDocument> SetWindowFields<TInput, TIntermediate, TPartitionBy, TWindowFields>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            Expression<Func<TIntermediate, TPartitionBy>> partitionBy,
            SortDefinition<TIntermediate> sortBy,
            Expression<Func<ISetWindowFieldsPartition<TIntermediate>, TWindowFields>> output)
        {
            Ensure.IsNotNull(partitionBy, nameof(partitionBy));
            Ensure.IsNotNull(sortBy, nameof(sortBy));
            Ensure.IsNotNull(output, nameof(output));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.SetWindowFields(partitionBy, sortBy, output));
        }

        /// <summary>
        /// Appends a $skip stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="skip">The number of documents to skip.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> Skip<TInput, TOutput>(
            this PipelineDefinition<TInput, TOutput> pipeline,
            long skip)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Skip<TOutput>(skip));
        }

        /// <summary>
        /// Appends a $sort stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="sort">The sort definition.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> Sort<TInput, TOutput>(
            this PipelineDefinition<TInput, TOutput> pipeline,
            SortDefinition<TOutput> sort)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Sort(sort));
        }

        /// <summary>
        /// Appends a $sortByCount stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="value">The value expression.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, AggregateSortByCountResult<TValue>> SortByCount<TInput, TIntermediate, TValue>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            AggregateExpressionDefinition<TIntermediate, TValue> value)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.SortByCount(value));
        }

        /// <summary>
        /// Appends a sortByCount stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="value">The value expression.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, AggregateSortByCountResult<TValue>> SortByCount<TInput, TIntermediate, TValue>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            Expression<Func<TIntermediate, TValue>> value)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.SortByCount(value));
        }

        /// <summary>
        /// Appends a $unionWith stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TWith">The type of the with collection documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="withCollection">The with collection.</param>
        /// <param name="withPipeline">The with pipeline.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> UnionWith<TInput, TWith, TOutput>(
            this PipelineDefinition<TInput, TOutput> pipeline,
            IMongoCollection<TWith> withCollection,
            PipelineDefinition<TWith, TOutput> withPipeline = null)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage<TInput, TOutput, TOutput>(
                PipelineStageDefinitionBuilder.UnionWith<TOutput, TWith>(withCollection, withPipeline),
                pipeline.OutputSerializer);
        }

        /// <summary>
        /// Appends an $unwind stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="field">The field.</param>
        /// <param name="options">The options.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> Unwind<TInput, TIntermediate, TOutput>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            FieldDefinition<TIntermediate> field,
            AggregateUnwindOptions<TOutput> options = null)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Unwind(field, options));
        }

        /// <summary>
        /// Appends an unwind stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="field">The field to unwind.</param>
        /// <param name="options">The options.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, BsonDocument> Unwind<TInput, TIntermediate>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            FieldDefinition<TIntermediate> field,
            AggregateUnwindOptions<BsonDocument> options = null)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Unwind(field, options));
        }

        /// <summary>
        /// Appends an unwind stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="field">The field to unwind.</param>
        /// <param name="options">The options.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, BsonDocument> Unwind<TInput, TIntermediate>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            Expression<Func<TIntermediate, object>> field,
            AggregateUnwindOptions<BsonDocument> options = null)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Unwind(field, options));
        }

        /// <summary>
        /// Appends an unwind stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="field">The field to unwind.</param>
        /// <param name="options">The options.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> Unwind<TInput, TIntermediate, TOutput>(
            this PipelineDefinition<TInput, TIntermediate> pipeline,
            Expression<Func<TIntermediate, object>> field,
            AggregateUnwindOptions<TOutput> options = null)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(PipelineStageDefinitionBuilder.Unwind(field, options));
        }

        /// <summary>
        /// Appends a $vectorSearch stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input.</typeparam>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <typeparam name="TOutput">The type of the output.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="field">The field.</param>
        /// <param name="queryVector">The query vector.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="options">The vector search options.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> VectorSearch<TInput, TField, TOutput>(
            this PipelineDefinition<TInput, TOutput> pipeline,
            Expression<Func<TOutput, TField>> field,
            QueryVector queryVector,
            int limit,
            VectorSearchOptions<TOutput> options = null)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(
                PipelineStageDefinitionBuilder.VectorSearch(field, queryVector, limit, options),
                pipeline.OutputSerializer);
        }

        /// <summary>
        /// Appends a $vectorSearch stage to the pipeline.
        /// </summary>
        /// <typeparam name="TInput">The type of the input.</typeparam>
        /// <typeparam name="TOutput">The type of the output.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="field">The field.</param>
        /// <param name="queryVector">The query vector.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="options">The vector search options.</param>
        /// <returns>A new pipeline with an additional stage.</returns>
        public static PipelineDefinition<TInput, TOutput> VectorSearch<TInput, TOutput>(
            this PipelineDefinition<TInput, TOutput> pipeline,
            FieldDefinition<TOutput> field,
            QueryVector queryVector,
            int limit,
            VectorSearchOptions<TOutput> options = null)
        {
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            return pipeline.AppendStage(
                PipelineStageDefinitionBuilder.VectorSearch(field, queryVector, limit, options),
                pipeline.OutputSerializer);
        }
    }

    /// <summary>
    /// Represents a pipeline consisting of an existing pipeline with one additional stage appended.
    /// </summary>
    /// <typeparam name="TInput">The type of the input documents.</typeparam>
    /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
    /// <typeparam name="TOutput">The type of the output documents.</typeparam>
    public sealed class AppendedStagePipelineDefinition<TInput, TIntermediate, TOutput> : PipelineDefinition<TInput, TOutput>
    {
        private readonly IBsonSerializer<TOutput> _outputSerializer;
        private readonly PipelineDefinition<TInput, TIntermediate> _pipeline;
        private readonly PipelineStageDefinition<TIntermediate, TOutput> _stage;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppendedStagePipelineDefinition{TInput, TIntermediate, TOutput}" /> class.
        /// </summary>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="stage">The stage.</param>
        /// <param name="outputSerializer">The output serializer.</param>
        public AppendedStagePipelineDefinition(
            PipelineDefinition<TInput, TIntermediate> pipeline,
            PipelineStageDefinition<TIntermediate, TOutput> stage,
            IBsonSerializer<TOutput> outputSerializer = null)
        {
            _pipeline = Ensure.IsNotNull(pipeline, nameof(pipeline));
            _stage = Ensure.IsNotNull(stage, nameof(stage));
            _outputSerializer = outputSerializer; // can be null
        }

        /// <inheritdoc/>
        public override IBsonSerializer<TOutput> OutputSerializer => _outputSerializer;

        /// <inheritdoc/>
        public override IEnumerable<IPipelineStageDefinition> Stages => _pipeline.Stages.Concat(new[] { _stage });

        /// <inheritdoc/>
        public override RenderedPipelineDefinition<TOutput> Render(RenderArgs<TInput> args)
        {
            var renderedPipeline = _pipeline.Render(args);
            var renderedStage = _stage.Render(new(renderedPipeline.OutputSerializer, args.SerializerRegistry, translationOptions: args.TranslationOptions));
            var documents = renderedPipeline.Documents.Concat(renderedStage.Documents);
            var outputSerializer = _outputSerializer ?? renderedStage.OutputSerializer;
            return new RenderedPipelineDefinition<TOutput>(documents, outputSerializer);
        }
    }

    /// <summary>
    /// Represents an empty pipeline.
    /// </summary>
    /// <typeparam name="TInput">The type of the input documents.</typeparam>
    public sealed class EmptyPipelineDefinition<TInput> : PipelineDefinition<TInput, TInput>
    {
        private readonly IBsonSerializer<TInput> _inputSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyPipelineDefinition{TOutput}"/> class.
        /// </summary>
        /// <param name="inputSerializer">The output serializer.</param>
        public EmptyPipelineDefinition(IBsonSerializer<TInput> inputSerializer = null)
        {
            _inputSerializer = inputSerializer; // can be null
        }

        /// <inheritdoc/>
        public override IBsonSerializer<TInput> OutputSerializer => _inputSerializer;

        /// <inheritdoc/>
        public override IEnumerable<IPipelineStageDefinition> Stages => Enumerable.Empty<IPipelineStageDefinition>();

        /// <inheritdoc/>
        public override RenderedPipelineDefinition<TInput> Render(RenderArgs<TInput> args)
        {
            var documents = Enumerable.Empty<BsonDocument>();
            return new RenderedPipelineDefinition<TInput>(documents, _inputSerializer ?? args.DocumentSerializer);
        }
    }

    /// <summary>
    /// Represents a pipeline consisting of an existing pipeline with one additional stage prepended.
    /// </summary>
    /// <typeparam name="TInput">The type of the input documents.</typeparam>
    /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
    /// <typeparam name="TOutput">The type of the output documents.</typeparam>
    public sealed class PrependedStagePipelineDefinition<TInput, TIntermediate, TOutput> : PipelineDefinition<TInput, TOutput>
    {
        private readonly IBsonSerializer<TOutput> _outputSerializer;
        private readonly PipelineDefinition<TIntermediate, TOutput> _pipeline;
        private readonly PipelineStageDefinition<TInput, TIntermediate> _stage;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrependedStagePipelineDefinition{TInput, TIntermediate, TOutput}" /> class.
        /// </summary>
        /// <param name="stage">The stage.</param>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="outputSerializer">The output serializer.</param>
        public PrependedStagePipelineDefinition(
            PipelineStageDefinition<TInput, TIntermediate> stage,
            PipelineDefinition<TIntermediate, TOutput> pipeline,
            IBsonSerializer<TOutput> outputSerializer = null)
        {
            _stage = Ensure.IsNotNull(stage, nameof(stage));
            _pipeline = Ensure.IsNotNull(pipeline, nameof(pipeline));
            _outputSerializer = outputSerializer; // can be null
        }

        /// <inheritdoc/>
        public override IBsonSerializer<TOutput> OutputSerializer => _outputSerializer;

        /// <inheritdoc/>
        public override IEnumerable<IPipelineStageDefinition> Stages => new[] { _stage }.Concat(_pipeline.Stages);

        /// <inheritdoc/>
        public override RenderedPipelineDefinition<TOutput> Render(RenderArgs<TInput> args)
        {
            var renderedStage = _stage.Render(args);
            var renderedPipeline = _pipeline.Render(new(renderedStage.OutputSerializer, args.SerializerRegistry, translationOptions: args.TranslationOptions));
            var documents = renderedStage.Documents.Concat(renderedPipeline.Documents);
            var outputSerializer = _outputSerializer ?? renderedPipeline.OutputSerializer;
            return new RenderedPipelineDefinition<TOutput>(documents, outputSerializer);
        }
    }

    /// <summary>
    /// Represents a pipeline with the output serializer replaced.
    /// </summary>
    /// <typeparam name="TInput">The type of the input documents.</typeparam>
    /// <typeparam name="TIntermediate">The type of the intermediate documents.</typeparam>
    /// <typeparam name="TOutput">The type of the output documents.</typeparam>
    /// <seealso cref="MongoDB.Driver.PipelineDefinition{TInput, TOutput}" />
    public sealed class ReplaceOutputSerializerPipelineDefinition<TInput, TIntermediate, TOutput> : PipelineDefinition<TInput, TOutput>
    {
        private readonly IBsonSerializer<TOutput> _outputSerializer;
        private readonly PipelineDefinition<TInput, TIntermediate> _pipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReplaceOutputSerializerPipelineDefinition{TInput, TIntermediate, TOutput}"/> class.
        /// </summary>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="outputSerializer">The output serializer.</param>
        public ReplaceOutputSerializerPipelineDefinition(
            PipelineDefinition<TInput, TIntermediate> pipeline,
            IBsonSerializer<TOutput> outputSerializer = null)
        {
            _pipeline = Ensure.IsNotNull(pipeline, nameof(pipeline));
            _outputSerializer = outputSerializer; // can be null
        }

        /// <inheritdoc/>
        public override IBsonSerializer<TOutput> OutputSerializer => _outputSerializer;

        /// <inheritdoc/>
        public override IEnumerable<IPipelineStageDefinition> Stages => _pipeline.Stages;

        /// <inheritdoc/>
        public override RenderedPipelineDefinition<TOutput> Render(RenderArgs<TInput> args)
        {
            var renderedPipeline = _pipeline.Render(args);
            var outputSerializer = _outputSerializer ?? args.SerializerRegistry.GetSerializer<TOutput>();
            return new RenderedPipelineDefinition<TOutput>(renderedPipeline.Documents, outputSerializer);
        }
    }
}
