using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace MongoDB.Driver.MqlApi
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static class MqlStage
    {
        public static MqlStage<TInput, BsonDocument> AddFields<TInput, TFields>(Expression<Func<TInput, TFields>> fields)
        {
            throw new NotImplementedException();
        }

        public static MqlStage<TDocument, ChangeStreamDocument<TDocument>> ChangeStream<TDocument>(ChangeStreamArgs args)
        {
            throw new NotImplementedException();
        }

        public static MqlStage<TInput, TNewOutput> Count<TInput, TNewOutput>(Expression<Func<TNewOutput>> countField)
        {
            throw new NotImplementedException();
        }

        public static MqlStage<NoPipelineInput, TDocument> Documents<TDocument>(params TDocument[] documents)
        {
            throw new NotImplementedException();
        }

        public static MqlStage<TInput, BsonDocument> Group<TInput, TId, TFields>(
            Expression<Func<TInput, TId>> id,
            Expression<Func<TId, TInput, TFields>> fields)
        {
            throw new NotImplementedException();
        }

        public static MqlStage<TInput, TInput> Limit<TInput>(long limit)
        {
            throw new NotImplementedException();
        }

        public static MqlStage<TInput, BsonDocument> Lookup<TInput, TForeign, TField>(
           IMongoCollection<TForeign> foreignCollection,
           Expression<Func<TInput, TField>> localField,
           Expression<Func<TForeign, TField>> foreignField,
           string @as)
        {
            throw new NotImplementedException();
        }

        public static MqlStage<TInput, TInput> Match<TInput>(Expression<Func<TInput, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public static MqlStage<TInput, NoPipelineOutput> Out<TInput>(string databaseName, string collectionName)
        {
            throw new NotImplementedException();
        }

        public static MqlStage<TInput, TOutput> Project<TInput, TOutput>(Expression<Func<TInput, TOutput>> projection)
        {
            throw new NotImplementedException();
        }

        public static MqlStage<TInput, TOutput> ReplaceRoot<TInput, TOutput>(Expression<Func<TInput, TOutput>> replacement)
        {
            throw new NotImplementedException();
        }

        public static MqlStage<TInput, TOutput> ReplaceWith<TInput, TOutput>(Expression<Func<TInput, TOutput>> replacement)
        {
            throw new NotImplementedException();
        }

        public static MqlStage<TInput, TInput> Sample<TInput>(long size)
        {
            throw new NotImplementedException();
        }

        public static MqlStage<TInput, BsonDocument> Set<TInput, TFields>(Expression<Func<TInput, TFields>> fields)
        {
            throw new NotImplementedException();
        }

        public static MqlStage<TInput, TInput> Skip<TInput>(long size)
        {
            throw new NotImplementedException();
        }

        public static MqlStage<TInput, TInput> Sort<TInput>(params Expression<Func<TInput, MqlSortOrder>>[] fields)
        {
            throw new NotImplementedException();
        }

        public static MqlStage<TInput, SortByCountOutput<TId>> SortByCount<TInput, TId>(Expression<Func<TInput, TId>> id)
        {
            throw new NotImplementedException();
        }

        public static MqlStage<TInput, TInput> UnionWith<TInput>(
            IMongoCollection<TInput> foreignCollection)
        {
            throw new NotImplementedException();
        }

        public static MqlStage<TInput, TInput> UnionWith<TInput, TForeign>(
            IMongoCollection<TForeign> foreignCollection,
            MqlPipeline<TForeign, TInput> foreignPipeline)
        {
            throw new NotImplementedException();
        }

        public static MqlStage<TInput, TInput> Unset<TInput>(
            params Expression<Func<TInput, object>>[] fields)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class MqlStage<TInput, TOutput>
    {
        private readonly IBsonSerializer<TInput> _inputSerializer;
        private readonly IBsonSerializer<TOutput> _outputSerializer;

        public MqlStage(IBsonSerializer<TInput> inputSerializer, IBsonSerializer<TOutput> outputSerializer)
        {
            _inputSerializer = inputSerializer;
            _outputSerializer = outputSerializer;
        }

        public IBsonSerializer<TInput> InputSerializer => _inputSerializer;
        public IBsonSerializer<TOutput> OutputSerializer => _outputSerializer;

        public List<BsonDocument> Translate()
        {
            throw new NotImplementedException();
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
