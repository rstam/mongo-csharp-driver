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
using System.Linq.Expressions;
using MongoDB.Bson;

namespace MongoDB.Driver.MqlBuilder
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public abstract class MqlStage
    {
        #region static
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

        public static MqlStage<TInput, TResult> Group<TInput, TResult>(
            Expression<Func<TInput, TResult>> fields)
        {
            return new MqlGroupStage<TInput, TResult>(fields);
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
            return new MqlMatchStage<TInput>(predicate);
        }

        public static MqlStage<TInput, NoPipelineOutput> Out<TInput>(string databaseName, string collectionName)
        {
            throw new NotImplementedException();
        }

        public static MqlStage<TInput, TOutput> Project<TInput, TOutput>(Expression<Func<TInput, TOutput>> projection)
        {
            return new MqlProjectStage<TInput, TOutput>(projection);
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

        public static MqlStage<TInput, TInput> Search<TInput>()
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

        public static MqlStage<TInput, TOutput> Unwind<TInput, TField, TOutput>(
            Expression<Func<TInput, TField>> field,
            TOutput prototype, // for type inference
            string includeArrayIndex = null,
            bool preserverNullAndEmptyArrays = false)
        {
            throw new NotImplementedException();
        }

        public static MqlStage<TInput, TInput> Unset<TInput>(
            params Expression<Func<TInput, object>>[] fields)
        {
            throw new NotImplementedException();
        }
        #endregion

        public abstract MqlStageType StageType { get; }
    }

    public abstract class MqlStage<TInput, TOutput> : MqlStage
    {
    }

    public enum MqlStageType
    {
        Group,
        Match,
        Project
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
