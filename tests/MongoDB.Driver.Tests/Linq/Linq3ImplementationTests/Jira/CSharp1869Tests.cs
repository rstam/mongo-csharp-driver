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
using System.Globalization;
using System.Linq;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Driver.Linq;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3ImplementationTests.Jira
{
    public class CSharp1869Tests : Linq3IntegrationTest
    {
        [Fact]
        public void Aggregate_with_sum_of_double_should_work()
        {
            var collection = CreateCollection();
            var minimumDate = DateTime.Parse("2022-01-01T00:00:00Z", null, DateTimeStyles.AdjustToUniversal);

            var queryable = collection
                .Aggregate()
                .Match(x => x.Date > minimumDate)
                .Group(
                    x => x.UserId,
                    group => new
                    {
                        id = group.Key,
                        Count = group.Count(),
                        Price = group.Sum(x => x.Price)
                    });

            var stages = Translate(collection, queryable);
            AssertStages(
                stages,
                "{ $match : { Date : { $gt : ISODate('2022-01-01T00:00:00Z') } } }",
                "{ $group : { _id : '$UserId', __agg0 : { $sum : 1 }, __agg1 : { $sum : '$Price' } } }",
                "{ $project : { id : '$_id', Count : '$__agg0', Price : '$__agg1', _id : 0 } }");

            var result = queryable.Single();
            result.id.Should().Be(2);
            result.Count.Should().Be(1);
            result.Price.Should().Be(2.0);
        }

        [Fact]
        public void Aggregate_with_sum_of_TimeSpan_should_work()
        {
            var collection = CreateCollection();
            var minimumDate = DateTime.Parse("2022-01-01T00:00:00Z", null, DateTimeStyles.AdjustToUniversal);

            var queryable = collection
                .Aggregate()
                .Match(x => x.Date > minimumDate)
                .Group(
                    x => x.UserId,
                    group => new
                    {
                        id = group.Key,
                        Count = group.Count(),
                        Duration = TimeSpan.FromTicks(group.Sum(x => x.Duration.Ticks))
                    });

            var stages = Translate(collection, queryable);
            AssertStages(
                stages,
                "{ $match : { Date : { $gt : ISODate('2022-01-01T00:00:00Z') } } }",
                "{ $group : { _id : '$UserId', __agg0 : { $sum : 1 }, __agg1 : { $sum : '$Price' } } }",
                "{ $project : { id : '$_id', Count : '$__agg0', Price : '$__agg1', _id : 0 } }");

            var result = queryable.Single();
            result.id.Should().Be(2);
            result.Count.Should().Be(1);
            result.Duration.TotalSeconds.Should().Be(2.0);
        }

        private IMongoCollection<C> CreateCollection()
        {
            var collection = GetCollection<C>("C");

            CreateCollection(
                collection,
                new C { Id = 1, Date = DateTime.Parse("2022-01-01T00:00:00Z", null, DateTimeStyles.AdjustToUniversal), UserId = 1, Price = 1.00, Duration = TimeSpan.FromSeconds(1) },
                new C { Id = 2, Date = DateTime.Parse("2022-01-02T00:00:00Z", null, DateTimeStyles.AdjustToUniversal), UserId = 2, Price = 2.00, Duration = TimeSpan.FromSeconds(2) });

            return collection;
        }

        private class C
        {
            public int Id { get; set; }
            public DateTime Date { get; set; }
            public int UserId { get; set; }
            public double Price { get; set; }
            [BsonTimeSpanOptions(BsonType.Int64, TimeSpanUnits.Ticks)] public TimeSpan Duration { get; set; } // note: default representation is string
        }
    }
}
