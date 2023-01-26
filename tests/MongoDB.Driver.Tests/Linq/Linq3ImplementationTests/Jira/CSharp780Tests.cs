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

using System.Linq;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3ImplementationTests.Jira
{
    public class CSharp780Tests : Linq3IntegrationTest
    {
        [Fact]
        public void AppendStage_with_null_resultSerializer_should_work()
        {
            var collection = CreateCollection();
            var options = new AggregateOptions { Hint = new BsonDocument("$natural", -1) };

            var queryable =
                collection.AsQueryable(options)
                .Take(1);

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $limit : 1 }");

            var results = queryable.ToList();
            results.Select(x => x.Id).Should().Equal(2);
        }

        private IMongoCollection<C> CreateCollection()
        {
            var collection = GetCollection<C>("C");

            CreateCollection(
                collection,
                new C { Id = 1 },
                new C { Id = 2 });

            return collection;
        }

        private class C
        {
            public int Id { get; set; }
        }
    }
}
