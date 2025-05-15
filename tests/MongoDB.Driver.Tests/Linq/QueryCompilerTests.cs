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

namespace MongoDB.Driver.Tests.Linq;

public class QueryCompilerTests : LinqIntegrationTest<QueryCompilerTests.ClassFixture>
{
    public QueryCompilerTests(ClassFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public void Query_with_zero_parameters_should_work()
    {
        var collection = Fixture.Collection;
        var compiledQuery = QueryCompiler.CompileQuery(
            () =>
                collection.AsQueryable(null)
                    .Take(1));

        var result = compiledQuery.Execute().Single();
        result.Id.Should().Be(1);
    }

    [Fact]
    public void Query_with_one_parameter_should_work()
    {
        var collection = Fixture.Collection;
        var compiledQuery = QueryCompiler.CompileQuery(
            (int id) =>
                collection.AsQueryable(null)
                    .Where(x => x.Id == id));

        var result = compiledQuery.Execute(1).Single();
        result.Id.Should().Be(1);
    }

    [Fact]
    public void Query_with_two_parameters_should_work()
    {
        var collection = Fixture.Collection;
        var compiledQuery = QueryCompiler.CompileQuery(
            (int id, int take) =>
                collection.AsQueryable(null)
                    .Where(x => x.Id == id)
                    .Take(take));

        var result = compiledQuery.Execute(1, 2).Single();
        result.Id.Should().Be(1);
    }

    [Fact]
    public void Scalar_query_with_one_parameter_should_work()
    {
        var collection = Fixture.Collection;
        var compiledQuery = QueryCompiler.CompileScalarQuery(
            (int id) =>
                collection.AsQueryable(null)
                    .Single(x => x.Id == id));

        var result = compiledQuery.Execute(1);
        result.Should().Be(1);
    }

    public class C
    {
        public int Id { get; set; }
    }

    public sealed class ClassFixture : MongoCollectionFixture<C>
    {
        protected override IEnumerable<C> InitialData =>
        [
            new C { Id = 1 }
        ];
    }
}
