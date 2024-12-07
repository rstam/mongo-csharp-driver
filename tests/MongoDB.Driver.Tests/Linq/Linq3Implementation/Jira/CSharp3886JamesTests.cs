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
using System.Linq.Expressions;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3Implementation.Jira
{
    public class CSharp3886JamesTests : Linq3IntegrationTest
    {
        [Fact]
        public void Where_predicate_should_work()
        {
            var collection = GetCollection();
            Expression<Func<C, bool>> predicate = x => x.P == (x.Es == E.A ? 42 : 144);

            var queryable = collection.AsQueryable().Where(predicate);

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $match : { $expr : { $eq : ['$P', { $cond : { if : { $eq : ['$Es', 'A'] }, then : 42, else : 144 } }] } } }");

            var results = queryable.ToList();
            results.Select(x => x.Id).Should().Equal(1);
        }

        private IMongoCollection<C> GetCollection()
        {
            var collection = GetCollection<C>("test");
            CreateCollection(
                collection,
                new C { Id = 1, Es = E.A, P = 42 },
                new C { Id = 2, Es = E.B, P = 42 });
            return collection;
        }

        public enum E { A, B }

        private class C
        {
            public int Id { get; set; }
            [BsonRepresentation(BsonType.String)] public E Es { get; set; }
            public int P { get; set; }
        }
    }
}
