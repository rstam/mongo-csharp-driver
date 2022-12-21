using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDB.Driver.MqlApi
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class MqlPipeline<TInput, TOutput>
    {
        private readonly IBsonSerializer<TInput> _inputSerializer;
        private readonly IBsonSerializer<TOutput> _outputSerializer;

        public MqlPipeline(IBsonSerializer<TInput> inputSerializer, IBsonSerializer<TOutput> outputSerializer)
        {
            _inputSerializer = inputSerializer;
            _outputSerializer = outputSerializer;
        }

        public IBsonSerializer<TInput> InputSerializer => _inputSerializer;
        public IBsonSerializer<TOutput> OutputSerializer => _outputSerializer;

        public MqlPipeline<TInput, TNewOutput> Append<TNewOutput>(MqlStage<TOutput, TNewOutput> stage)
        {
            throw new NotImplementedException();
        }

        public List<BsonDocument> Translate()
        {
            throw new NotImplementedException();
        }

        public static implicit operator PipelineDefinition<TInput, TOutput>(MqlPipeline<TInput, TOutput> pipeline)
        {
            var stages = pipeline.Translate();
            return new BsonDocumentStagePipelineDefinition<TInput, TOutput>(stages, pipeline.OutputSerializer);
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

        public static MqlPipeline<TInput, BsonDocument> Group<TInput, TOutput, TId, TFields>(
            this MqlPipeline<TInput, TOutput> pipeline,
            Expression<Func<TOutput, TId>> id,
            Expression<Func<TId, TOutput, TFields>> fields)
        {
            return pipeline.Append(MqlStage.Group(id, fields));
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
