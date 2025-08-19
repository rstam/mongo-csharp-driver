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
using MongoDB.Driver.TestHelpers;
using FluentAssertions;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3Implementation.Jira;

public class CSharp5707Tests : LinqIntegrationTest<CSharp5707Tests.ClassFixture>
{
    public CSharp5707Tests(ClassFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public void Test1()
    {
        var collection = Fixture.Collection;

        var queryable = collection.AsQueryable()
            .OrderBy(c => c.Id);

        var stages = Translate(collection, queryable);
        AssertStages(
            stages,
            "{ $project : { _id : 0, _document : '$$ROOT', _key1 : '$$ROOT' } }",
            "{ $sort : { _key1 : 1 } }",
            "{ $replaceRoot : { newRoot : '$_document' } }");

        var results = queryable.ToList();
        results.Select(c => c.Id.X).Should().Equal(1, 2);
    }

    public class Customer
    {
        public CompoundKey Id { get; set; }
        public string Name { get; set; }
    }

    public class CompoundKey
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public sealed class ClassFixture : MongoCollectionFixture<Customer>
    {
        protected override IEnumerable<Customer> InitialData =>
        [
            new Customer { Id = new CompoundKey { X = 2, Y = 1 }, Name = "Zachary" },
            new Customer { Id = new CompoundKey { X = 1, Y = 2 }, Name = "Adam" }
        ];
    }
}
