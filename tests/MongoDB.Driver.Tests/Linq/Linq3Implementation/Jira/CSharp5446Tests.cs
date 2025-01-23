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

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3Implementation.Jira
{
    public class CSharp5446Tests : Linq3IntegrationTest
    {
        [Fact]
        public void Aggregate_Group_Distinct_should_work()
        {
            var collection = GetCollection();

            var request = collection
                .Aggregate()
                .Group(x => x.P1, g => new { P1 = g.Key, P2 = g.Select(i => i.P2).Distinct() });

            var stages = Translate(collection, request);
            AssertStages(stages,
                "{ $group: { _id: '$P1', __agg0: { $addToSet: '$P2' } } }",
                "{ $project: { P1: '$_id', P2: '$__agg0', _id: 0 } }");
        }

        [Fact]
        public void Aggregate_Group_new_HashSet_should_work()
        {
            var collection = GetCollection();

            var request = collection
                .Aggregate()
                .Group(x => x.P1, g => new { P1 = g.Key, P2 = new HashSet<string>(g.Select(i => i.P2)) });

            var stages = Translate(collection, request);
            AssertStages(stages,
                "{ $group: { _id: '$P1', __agg0: { $addToSet: '$P2' } } }",
                "{ $project: { P1: '$_id', P2: '$__agg0', _id: 0 } }");
        }

        [Fact]
        public void Queryable_GroupBy_Distinct_should_work()
        {
            var collection = GetCollection();

            var request = collection
                .AsQueryable()
                .GroupBy(
                    x => x.P1,
                    (key, elements) => new { P1 = key, P2 = elements.Select(i => i.P2).Distinct() });

            var stages = Translate(collection, request);
            AssertStages(stages,
                "{ $group: { _id: '$P1', __agg0: { $addToSet: '$P2' } } }",
                "{ $project: { P1: '$_id', P2: '$__agg0', _id: 0 } }");
        }

        [Fact]
        public void Queryable_GroupBy_new_HashSet_should_work()
        {
            var collection = GetCollection();

            var request = collection
                .AsQueryable()
                .GroupBy(
                    x => x.P1,
                    (key, elements) => new { P1 = key, P2 = new HashSet<string>(elements.Select(i => i.P2)) });

            var stages = Translate(collection, request);
            AssertStages(stages,
                "{ $group: { _id: '$P1', __agg0: { $addToSet: '$P2' } } }",
                "{ $project: { P1: '$_id', P2: '$__agg0', _id: 0 } }");
        }

        [Fact]
        public void Queryable_GroupBy_Select_Distinct_should_work()
        {
            var collection = GetCollection();

            var request = collection
                .AsQueryable()
                .GroupBy(x => x.P1)
                .Select(g => new { P1 = g.Key, P2 = g.Select(i => i.P2).Distinct() });

            var stages = Translate(collection, request);
            AssertStages(stages,
                "{ $group: { _id: '$P1', __agg0: { $addToSet: '$P2' } } }",
                "{ $project: { P1: '$_id', P2: '$__agg0', _id: 0 } }");
        }

        [Fact]
        public void Queryable_GroupBy_Select_new_HashSet_should_work()
        {
            var collection = GetCollection();

            var request = collection
                .AsQueryable()
                .GroupBy(x => x.P1)
                .Select(g => new { P1 = g.Key, P2 = new HashSet<string>(g.Select(i => i.P2)) });

            var stages = Translate(collection, request);
            AssertStages(stages,
                "{ $group: { _id: '$P1', __agg0: { $addToSet: '$P2' } } }",
                "{ $project: { P1: '$_id', P2: '$__agg0', _id: 0 } }");
        }

        private IMongoCollection<SourceData> GetCollection()
        {
            var collection = GetCollection<SourceData>("test");
            CreateCollection(
                collection,
                new SourceData { P1 = 1, P2 = "A" },
                new SourceData { P1 = 1, P2 = "B" },
                new SourceData { P1 = 2, P2 = "C" });
            return collection;
        }

        private class SourceData
        {
            public int P1 { get; set; }
            public string P2 { get; set; }
        }
    }
}
