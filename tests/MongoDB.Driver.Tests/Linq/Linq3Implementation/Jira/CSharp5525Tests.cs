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

public class CSharp5525Tests : LinqIntegrationTest<CSharp5525Tests.ClassFixture>
{
    public CSharp5525Tests(ClassFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public void Take_minus_two_should_return_empty_array()
    {
        var collection = Fixture.Collection;

        var queryable = collection.AsQueryable()
            .Select(x => x.A.Take(-2));

        var stages = Translate(collection, queryable);
        AssertStages(stages, "{ $project : { _v : { $slice : ['$A', 0] }, _id : 0} }");

        var result = queryable.Single();
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0, new int[] { })]
    [InlineData(1, new int[] { 3 })]
    [InlineData(2, new int[] { 2, 3 })]
    [InlineData(3, new int[] { 1, 2, 3 })]
    [InlineData(4, new int[] { 1, 2, 3 })]
    public void Simulated_Take_Last_should_return_expected_result(int n, int[] expectedResult)
    {
        var collection = Fixture.Collection;

        var queryable = collection.AsQueryable()
            .Select(x => x.A.Skip(x.A.Length - n));

        var stages = Translate(collection, queryable);
        var skip = n == 0 ? "{ $size : '$A' }" : $$"""{ $subtract : [{ $size : '$A' }, {{n}}] }""";
        var expectedStage = $$"""{ $project : { _v : { $slice : ['$A', { $max : [{{skip}}, 0] }, 2147483647] }, _id : 0 } }""";
        AssertStages(stages, expectedStage);

        var result = queryable.Single();
        result.Should().Equal(expectedResult);
    }

    public class C
    {
        public int Id { get; set; }
        public int[] A { get; set; }
    }

    public sealed class ClassFixture : MongoCollectionFixture<C>
    {
        protected override IEnumerable<C> InitialData =>
        [
            new C { Id = 1, A = [1, 2, 3] }
        ];
    }
}
