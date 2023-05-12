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
using MongoDB.Driver.Linq;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3ImplementationTests.Jira
{
    public class CSharp4535Tests : Linq3IntegrationTest
    {
        [Fact]
        public void Where_should_work()
        {
            var collection = CreateCollection();

            var queryable1 = collection.AsQueryable();
            var queryable2 = (IQueryable<Entity>)queryable1; // queryable1 and queryable2 reference the same object but have different compile time types
            var queryable3 = queryable2.Where(x => x.Value == 1);

            var stages = Translate(collection, queryable3);
            AssertStages(stages, "{ $match : { Value : 1 } }");

            var results = queryable3.ToList();
            results.Select(x => x.Id).Should().Equal(1);
        }

        private IMongoCollection<MongoEntity> CreateCollection()
        {
            var collection = GetCollection<MongoEntity>("C");

            CreateCollection(
                collection,
                new MongoEntity { Id = 1, Value = 1, Internal = 1 },
                new MongoEntity { Id = 2, Value = 2, Internal = 2 });

            return collection;
        }

        public class Entity
        {
            public int Id { get; set; }
            public int Value { get; set; }
        }

        public class MongoEntity : Entity
        {
            public int Internal { get; set; }
        }
    }
}
