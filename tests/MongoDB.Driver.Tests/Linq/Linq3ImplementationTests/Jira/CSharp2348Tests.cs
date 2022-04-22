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
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3ImplementationTests.Jira
{
    public class CSharp2348Tests : Linq3IntegrationTest
    {
        [Fact]
        public void Any_with_equals_should_work()
        {
            var collection = CreateCollection();
            var find = collection.Find(x => x.Roles.Any(r => r == Role.Admin));

            var renderedFilter = RenderFilter(collection, find);
            renderedFilter.Should().Be("{ Roles : 1 }");

            var results = find.ToList().OrderBy(x => x.Id).ToList();
            results.Select(x => x.Id).Should().Equal(1, 3);
        }

        [Fact]
        public void Any_with_or_of_equals_should_work()
        {
            var collection = CreateCollection();
            var find = collection.Find(x => x.Roles.Any(r => r == Role.Admin || r == Role.Editor));

            var renderedFilter = RenderFilter(collection, find);
            renderedFilter.Should().Be("{ Roles : { $elemMatch : { $or : [{ $eq : 1 }, { $eq : 2 }] } } }");

            var results = find.ToList().OrderBy(x => x.Id).ToList();
            results.Select(x => x.Id).Should().Equal(1, 2, 3);
        }

        private IMongoCollection<User> CreateCollection()
        {
            var collection = GetCollection<User>();
            CreateCollection(
                collection,
                new User { Id = 1, Roles = new[] { Role.Admin } },
                new User { Id = 2, Roles = new[] { Role.Editor } },
                new User { Id = 3, Roles = new[] { Role.Admin, Role.Editor } });
            return collection;
        }

        private BsonDocument RenderFilter(IMongoCollection<User> collection, IFindFluent<User, User> find)
        {
            var filter = find.Filter;
            var documentSerializer = collection.DocumentSerializer;
            var serializerRegistry = BsonSerializer.SerializerRegistry;
            return filter.Render(documentSerializer, serializerRegistry, LinqProvider.V3);
        }

        public class User
        {
            public int Id { get; set; }
            public Role[] Roles { get; set; }
        }

        public enum Role
        {
            Admin = 1,
            Editor = 2
        }
    }
}
