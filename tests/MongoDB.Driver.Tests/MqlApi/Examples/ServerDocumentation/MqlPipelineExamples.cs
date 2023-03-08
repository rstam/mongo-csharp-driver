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
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.MqlApi;
using Xunit;

namespace MongoDB.Driver.Tests.MqlApi.Examples.ServerDocumentation
{
    public class MqlPipelineExamples : MqlIntegrationTest
    {
        [Fact]
        public void Pipeline_examples()
        {
            var collection = CreateCollection();
            var database = collection.Database;
            var foreignCollection = database.GetCollection<C>("foreignCollection");

            // https://www.mongodb.com/docs/manual/reference/operator/aggregation-pipeline/
            _ = Mql.Pipeline(collection).AddFields(x => new { X = 1 });
            // TODO: $bucket
            // TODO: $bucketAuto
            _ = Mql.Pipeline(collection).ChangeStream(new ChangeStreamArgs { ShowExpandedEvents = true }); // [{ $changeStream : { showExpandedEvents : true } }]
            // TODO: $collStats
            _ = Mql.Pipeline(collection).Count(() => new { X = 0 }); // [{ $count : 'X' }]
            // TODO: $currentOp
            // TODO: $densify
            _ = Mql.Pipeline(database).Documents(new D { X = 1 }, new D { X = 2 }); // [{ $documents : [{ X : 1 }, { X : 2 }] }]
            // TODO: $facet
            // TODO: $fill
            // TODO: $geoNear
            // TODO: $graphLookup
            // TODO: $group
            // TODO: $indexStats
            _ = Mql.Pipeline(collection).Limit(1); // [{ $limit : 1 }]
            // TODO: $listLocalSessions
            // TODO: $listSessions
            _ = Mql.Pipeline(collection).Lookup(foreignCollection, x => x.X, x => x.Y, "output"); // [{ $lookup : { from : 'foreignCollection', localField : 'X', foreignField : 'Y', as : 'output' } }]
            _ = Mql.Pipeline(collection).Match(x => x.X == 1); // [{ $match : { X : { $eq : 1 } } }]
            // TODO: $merge
            _ = Mql.Pipeline(collection).Out("database", "collection"); // [{ $out : { db : 'database', coll : 'collection' } }]
            // TODO: $planCacheStats
            _ = Mql.Pipeline(collection).Project(x => new { V = x.X }); // [{ $project : { V : '$X', _id : 0 } }]
            // TODO: $redact
            _ = Mql.Pipeline(collection).ReplaceRoot(x => new { V = x.X }); // [{ $replaceRoot : { newRoot : { V : '$X' } } }]
            _ = Mql.Pipeline(collection).ReplaceWith(x => new { V = x.X }); // [{ $replaceWith : { V : '$X' } }]
            _ = Mql.Pipeline(collection).Sample(100); // [{ $sample : { size : 100 } }]
            // TODO: $search
            // TODO: $searchMeta
            _ = Mql.Pipeline(collection).Set(x => new { X = 1 });
            // TODO: $setWindowFields
            // TODO: $sartedDataDistribution
            _ = Mql.Pipeline(collection).Skip(1); // [{ $skip : 1 }]
            _ = Mql.Pipeline(collection).Sort(x => Mql.Ascending(x.X), x => Mql.Descending(x.Y)); // [{ $sort : { X : 1, Y : -1 } }]
            _ = Mql.Pipeline(collection).SortByCount(x => x.X); // [{ $sortByCount : '$X' }]
            _ = Mql.Pipeline(collection).UnionWith(foreignCollection); // [{ $unionWith : 'foreignCollection' }]
            _ = Mql.Pipeline(collection).UnionWith(foreignCollection, Mql.Pipeline(foreignCollection).Limit(1)); // [{ $unionWith : { coll : 'foreignCollection', pipeline : [{ $limit : 1 }] } }]
            _ = Mql.Pipeline(collection).Unset(x => x.X); // [{ $unset : 'X' }]
            _ = Mql.Pipeline(collection).Unset(x => x.X, x => x.Y); // [{ $unset : ['X', 'Y'] }]
            // TODO: $unwind
        }

        private IMongoCollection<C> CreateCollection()
        {
            var collection = GetCollection<C>();

            CreateCollection(
                collection,
                new C { Id = 1, X = 1, Y = 1 },
                new C { Id = 2, X = 2, Y = 2 });

            return collection;
        }

        public class C
        {
            public int Id { get; set; }
            public int[] A { get; set; }
            public DateTime D { get; set; }
            public string S { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
        }

        public class D
        {
            public int X { get; set; }
        }
    }
}
