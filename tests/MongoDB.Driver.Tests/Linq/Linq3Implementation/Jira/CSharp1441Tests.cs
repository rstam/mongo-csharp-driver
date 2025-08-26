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

public class CSharp1441Tests : LinqIntegrationTest<CSharp1441Tests.ClassFixture>
{
    public CSharp1441Tests(ClassFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public void SelectMany_with_collection_selector_with_a_transparent_identifier()
    {
        var collection = Fixture.Collection;

        var queryable =
            from x in collection.AsQueryable()
            from y in x.G
            orderby x.Id
            select new { x.B, y };

        var stages = Translate(collection, queryable);
        AssertStages(
            stages,
            "{ $project : { _v : { $map : { input : '$G', as : 'y', in : { x : '$$ROOT', y : '$$y' } } }, _id : 0 } }",
            "{ $unwind : '$_v' }",
            "{ $sort : { '_v.x._id' : 1 } }",
            "{ $project : { B : '$_v.x.B', y : '$_v.y', _id : 0 } }");

        var results = queryable.ToList();
        results.Select(x => x.B).Should().Equal(11, 11, 22, 22, 22);
        results.Select(x => x.y).Should().Equal(13, 14, 23, 24, 25);
    }

    public class C
    {
        public int Id { get; set; }
        public int B { get; set; }
        public int[] G { get; set; }
    }

    public sealed class ClassFixture : MongoCollectionFixture<C>
    {
        protected override IEnumerable<C> InitialData =>
        [
            new C { Id = 2, B = 22, G = [23, 24, 25] },
            new C { Id = 1, B = 11, G = [13, 14] },
        ];
    }
}
