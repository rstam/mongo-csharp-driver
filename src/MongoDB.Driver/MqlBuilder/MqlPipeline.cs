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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.MqlBuilder.Translators.ExpressionToFilterTranslators;

namespace MongoDB.Driver.MqlBuilder
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class MqlPipeline<TInput, TOutput>
    {
        private readonly IBsonSerializer<TInput> _inputSerializer;
        private readonly IReadOnlyList<MqlStage> _stages;

        public MqlPipeline(IBsonSerializer<TInput> inputSerializer)
        {
            _inputSerializer = Ensure.IsNotNull(inputSerializer, nameof(inputSerializer));
            _stages = new MqlStage[0];
        }

        public MqlPipeline(IBsonSerializer<TInput> inputSerializer, IEnumerable<MqlStage> stages)
        {
            _inputSerializer = Ensure.IsNotNull(inputSerializer, nameof(inputSerializer));
            _stages = Ensure.IsNotNull(stages, nameof(stages)).ToArray();
        }

        public IBsonSerializer<TInput> InputSerializer => _inputSerializer;
        public IReadOnlyList<MqlStage> Stages => _stages;

        public MqlPipeline<TInput, TNewOutput> Append<TNewOutput>(MqlStage<TOutput, TNewOutput> stage)
        {
            var stages = _stages.Append(stage);
            return new MqlPipeline<TInput, TNewOutput>(_inputSerializer, stages);
        }

        public MqlPipeline<TInput, TAs> As<TAs>(IBsonSerializer<TAs> serializer = null)
        {
            throw new NotImplementedException();
        }

        public static implicit operator PipelineDefinition<TInput, TOutput>(MqlPipeline<TInput, TOutput> pipeline)
        {
            var translatedPipeline = MqlPipelineTranslator.Translate(pipeline);
            var stages = translatedPipeline.Stages.Select(s => s.Render()).Cast<BsonDocument>();
            var outputSerializer = (IBsonSerializer<TOutput>)translatedPipeline.OutputSerializer;
            return new BsonDocumentStagePipelineDefinition<TInput, TOutput>(stages, outputSerializer);
        }
    }

    public static class MqlPipelineExtensions
    {
        public static MqlPipeline<TInput, BsonDocument> AddFields<TInput, TOutput, TFields>(
            this MqlPipeline<TInput, TOutput> pipeline,
            Expression<Func<TOutput, TFields>> fields)
        {
            return pipeline.Append(MqlStage.AddFields(fields));
        }

        public static MqlPipeline<TDocument, ChangeStreamDocument<TDocument>> ChangeStream<TDocument>(
            this MqlPipeline<TDocument, TDocument> pipeline,
            ChangeStreamArgs args)
        {
            return pipeline.Append(MqlStage.ChangeStream<TDocument>(args));
        }

        public static MqlPipeline<TInput, TNewOutput> Count<TInput, TOutput, TNewOutput>(
            this MqlPipeline<TInput, TOutput> pipeline,
            Expression<Func<TNewOutput>> countField)
        {
            return pipeline.Append(MqlStage.Count<TOutput, TNewOutput>(countField));
        }

        public static MqlPipeline<NoPipelineInput, TDocument> Documents<TDocument>(
            this MqlPipeline<NoPipelineInput, NoPipelineInput> pipeline,
            params TDocument[] documents)
        {
            return pipeline.Append(MqlStage.Documents(documents));
        }

        public static MqlPipeline<TInput, TResult> Group<TInput, TOutput, TResult>(
            this MqlPipeline<TInput, TOutput> pipeline,
            Expression<Func<TOutput, TResult>> fields)
        {
            return pipeline.Append(MqlStage.Group(fields));
        }

        public static MqlPipeline<TInput, TOutput> Limit<TInput, TOutput>(
            this MqlPipeline<TInput, TOutput> pipeline,
            long limit)
        {
            return pipeline.Append(MqlStage.Limit<TOutput>(limit));
        }

        public static MqlPipeline<TInput, BsonDocument> Lookup<TInput, TOutput, TForeign, TField>(
            this MqlPipeline<TInput, TOutput> pipeline,
            IMongoCollection<TForeign> foreignCollection,
            Expression<Func<TOutput, TField>> localField,
            Expression<Func<TForeign, TField>> foreignField,
            string @as)
        {
            return pipeline.Append(MqlStage.Lookup(foreignCollection, localField, foreignField, @as));
        }

        public static MqlPipeline<TInput, TOutput> Match<TInput, TOutput>(
            this MqlPipeline<TInput, TOutput> pipeline,
            Expression<Func<TOutput, bool>> predicate)
        {
            return pipeline.Append(MqlStage.Match(predicate));
        }

        public static MqlPipeline<TInput, NoPipelineOutput> Out<TInput, TOutput>(
            this MqlPipeline<TInput, TOutput> pipeline,
            string databaseName,
            string collectionName)
        {
            return pipeline.Append(MqlStage.Out<TOutput>(databaseName, collectionName));
        }

        public static MqlPipeline<TInput, TNewOutput> Project<TInput, TOutput, TNewOutput>(
            this MqlPipeline<TInput, TOutput> pipeline,
            Expression<Func<TOutput, TNewOutput>> projection)
        {
            return pipeline.Append(MqlStage.Project(projection));
        }

        public static MqlPipeline<TInput, TNewOutput> ReplaceRoot<TInput, TOutput, TNewOutput>(
            this MqlPipeline<TInput, TOutput> pipeline,
            Expression<Func<TOutput, TNewOutput>> replacement)
        {
            return pipeline.Append(MqlStage.ReplaceRoot(replacement));
        }

        public static MqlPipeline<TInput, TNewOutput> ReplaceWith<TInput, TOutput, TNewOutput>(
            this MqlPipeline<TInput, TOutput> pipeline,
            Expression<Func<TOutput, TNewOutput>> replacement)
        {
            return pipeline.Append(MqlStage.ReplaceWith(replacement));
        }

        public static MqlPipeline<TInput, TOutput> Sample<TInput, TOutput>(
            this MqlPipeline<TInput, TOutput> pipeline,
            long size)
        {
            return pipeline.Append(MqlStage.Sample<TOutput>(size));
        }

        public static MqlPipeline<TInput, BsonDocument> Set<TInput, TOutput, TFields>(
            this MqlPipeline<TInput, TOutput> pipeline,
            Expression<Func<TOutput, TFields>> fields)
        {
            return pipeline.Append(MqlStage.Set(fields));
        }

        public static MqlPipeline<TInput, TOutput> Skip<TInput, TOutput>(
            this MqlPipeline<TInput, TOutput> pipeline,
            long count)
        {
            return pipeline.Append(MqlStage.Skip<TOutput>(count));
        }

        public static MqlPipeline<TInput, TOutput> Sort<TInput, TOutput>(
            this MqlPipeline<TInput, TOutput> pipeline,
            params Expression<Func<TOutput, MqlSortOrder>>[] fields)
        {
            return pipeline.Append(MqlStage.Sort(fields));
        }

        public static MqlPipeline<TInput, SortByCountOutput<TId>> SortByCount<TInput, TOutput, TId>(
            this MqlPipeline<TInput, TOutput> pipeline,
            Expression<Func<TOutput, TId>> id)
        {
            return pipeline.Append(MqlStage.SortByCount(id));
        }

        public static MqlPipeline<TInput, TOutput> UnionWith<TInput, TOutput>(
            this MqlPipeline<TInput, TOutput> pipeline,
            IMongoCollection<TOutput> foreignCollection)
        {
            return pipeline.Append(MqlStage.UnionWith(foreignCollection));
        }

        public static MqlPipeline<TInput, TOutput> UnionWith<TInput, TOutput, TForeign>(
            this MqlPipeline<TInput, TOutput> pipeline,
            IMongoCollection<TForeign> foreignCollection,
            MqlPipeline<TForeign, TOutput> foreignPipeline)
        {
            return pipeline.Append(MqlStage.UnionWith(foreignCollection, foreignPipeline));
        }

        public static MqlPipeline<TInput, TOutput> Unset<TInput, TOutput>(
            this MqlPipeline<TInput, TOutput> pipeline,
            params Expression<Func<TOutput, object>>[] fields)
        {
            return pipeline.Append(MqlStage.Unset(fields));
        }

        public static MqlPipeline<TInput, TNewOutput> Unwind<TInput, TOutput, TField, TNewOutput>(
            this MqlPipeline<TInput, TOutput> pipeline,
            Expression<Func<TOutput, TField>> field,
            TNewOutput prototype, // for type inference only
            string includeArrayIndex = null,
            bool preserveNullAndEmptyArrays = false)
        {
            return pipeline.Append(MqlStage.Unwind(field, prototype, includeArrayIndex, preserveNullAndEmptyArrays));
        }
    }

    public class ChangeStreamArgs
    {
        public bool ShowExpandedEvents { get; set; }
    }

    public class MqlSortOrder
    {
    }

    public class SortByCountOutput<TId>
    {
        public TId Id { get; set; }
        [BsonElement("count")] public long Count { get; set; }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
