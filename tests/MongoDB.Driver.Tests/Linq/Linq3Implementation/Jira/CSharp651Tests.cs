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
using MongoDB.Driver.Linq;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3Implementation.Jira;

public class CSharp651Tests : LinqIntegrationTest<CSharp651Tests.ClassFixture>
{
    public CSharp651Tests(ClassFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public void Test1()
    {
        var collection = Fixture.Collection;
        var hash = new byte[] { 1, 2, 3 };

        var queryable = collection.AsQueryable()
            .Where(p => p.Hash == hash)
            .Select(p => (int?)p.Id);

        var result = queryable.SingleOrDefault();
        result.Should().Be(1);

        var stages = queryable.GetLoggedStages();
        AssertStages(
            stages,
            "{ $match : { Hash : HexData(0, '010203') } }",
            "{ $project : { _v : '$_id', _id : 0 } }",
            "{ $limit : 2 }");
    }

    public class C
    {
        public int Id { get; set; }
        public byte[] Hash { get; set; }
    }

    public sealed class ClassFixture : MongoCollectionFixture<C>
    {
        protected override IEnumerable<C> InitialData =>
        [
            new C { Id = 1, Hash = [1, 2, 3] },
            new C { Id = 2, Hash = [4, 5, 6] }
        ];
    }
}
