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
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.Linq;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3Implementation.Jira
{
    public class CSharp3886RobinTests : Linq3IntegrationTest
    {
        [Fact]
        public void Where_example_1_with_Type1_should_work()
        {
            var collection = GetCollection();

            var aggregate = collection
                .Aggregate()
                .Project(d => new
                {
                    Id = d.Id,
                    FirstCsOfA = d.Cs.Where(x => x.Type1 == Es.A, 1).ToList(),
                });

            var stages = Translate(collection, aggregate);
            AssertStages(stages, "{ $project : { _id : '$_id', FirstCsOfA : { $filter : { input : '$Cs', as : 'x', cond : { $eq : ['$$x.Type1', 0] }, limit : 1 } } } }");

            var result = aggregate.Single();
            result.FirstCsOfA.Select(x => x.Value).Should().Equal(2);
        }

        [Fact]
        public void Where_example_1_with_Type2_should_work()
        {
            var collection = GetCollection();

            var aggregate = collection
                .Aggregate()
                .Project(d => new
                {
                    Id = d.Id,
                    FirstCsOfA = d.Cs.Where(x => x.Type2 == Es.A, 1).ToList(),
                });

            var stages = Translate(collection, aggregate);
            AssertStages(stages, "{ $project : { _id : '$_id', FirstCsOfA : { $filter : { input : '$Cs', as : 'x', cond : { $eq : ['$$x.Type2', 'A'] }, limit : 1 } } } }");

            var result = aggregate.Single();
            result.FirstCsOfA.Select(x => x.Value).Should().Equal(2);
        }

        [Fact]
        public void Where_example_2_with_Type1_should_work()
        {
            var collection = GetCollection();

            var aggregate = collection
                .Aggregate()
                .Project(d => new
                {
                    Id = d.Id,
                    FirstCsOfA = d.Cs.Where(x => x.Type1.ToString() == "0", 1).ToList(), // note: Es.A.ToString evaluates differently client-side than server-side
                });

            var stages = Translate(collection, aggregate);
            AssertStages(stages, "{ $project : { _id : '$_id', FirstCsOfA : { $filter : { input : '$Cs', as : 'x', cond : { $eq : [{ $toString : '$$x.Type1' }, '0'] }, limit : 1 } } } }");

            var result = aggregate.Single();
            result.FirstCsOfA.Select(x => x.Value).Should().Equal(2);
        }

        [Fact]
        public void Where_example_2_with_Type2_should_work()
        {
            var collection = GetCollection();

            var aggregate = collection
                .Aggregate()
                .Project(d => new
                {
                    Id = d.Id,
                    FirstCsOfA = d.Cs.Where(x => x.Type2.ToString() == Es.A.ToString(), 1).ToList(),
                });

            var stages = Translate(collection, aggregate);
            AssertStages(stages, "{ $project : { _id : '$_id', FirstCsOfA : { $filter : { input : '$Cs', as : 'x', cond : { $eq : [{ $toString : '$$x.Type2' }, 'A'] }, limit : 1 } } } }");

            var result = aggregate.Single();
            result.FirstCsOfA.Select(x => x.Value).Should().Equal(2);
        }

        [Fact]
        public void Where_example_3_with_Type1_should_work()
        {
            var collection = GetCollection();

            var aggregate = collection
                .Aggregate()
                .Project(d => new
                {
                    Id = d.Id,
                    FirstCsOfA = d.Cs.Where(x => x.Type1 == Es.A).Take(1).ToList(),
                });

            var stages = Translate(collection, aggregate);
            AssertStages(stages, "{ $project : { _id : '$_id', FirstCsOfA : { $slice : [{ $filter : { input : '$Cs', as : 'x', cond : { $eq : ['$$x.Type1', 0] } } }, 1] } } }");

            var result = aggregate.Single();
            result.FirstCsOfA.Select(x => x.Value).Should().Equal(2);
        }

        [Fact]
        public void Where_example_3_with_Type2_should_work()
        {
            var collection = GetCollection();

            var aggregate = collection
                .Aggregate()
                .Project(d => new
                {
                    Id = d.Id,
                    FirstCsOfA = d.Cs.Where(x => x.Type2 == Es.A).Take(1).ToList(),
                });

            var stages = Translate(collection, aggregate);
            AssertStages(stages, "{ $project : { _id : '$_id', FirstCsOfA : { $slice : [{ $filter : { input : '$Cs', as : 'x', cond : { $eq : ['$$x.Type2', 'A'] } } }, 1] } } }");

            var result = aggregate.Single();
            result.FirstCsOfA.Select(x => x.Value).Should().Equal(2);
        }

        private IMongoCollection<D> GetCollection()
        {
            var collection = GetCollection<D>("test");
            CreateCollection(
                collection,
                new D
                {
                    Id = 1,
                    Cs =
                        [
                            new C { Type1 = Es.B, Type2 = Es.B, Value = 1},
                            new C { Type1 = Es.A, Type2 = Es.A, Value = 2},
                            new C { Type1 = Es.A, Type2 = Es.A, Value = 3},
                        ]
                });
            return collection;
        }

        enum Es { A, B }

        class C
        {
            public Es Type1 { get; set; }
            [BsonRepresentation(BsonType.String)] public Es Type2 { get; set; }
            public int Value { get; set; }
        }

        class D
        {
            public int Id { get; set; }
            public List<C> Cs { get; set; }
        }
    }
}
