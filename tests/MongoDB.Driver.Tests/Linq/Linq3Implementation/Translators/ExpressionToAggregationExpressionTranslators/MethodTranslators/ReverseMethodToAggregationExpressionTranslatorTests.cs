﻿/* Copyright 2010-present MongoDB Inc.
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
using MongoDB.Driver.TestHelpers;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3Implementation.Translators.ExpressionToAggregationExpressionTranslators.MethodTranslators
{
    public class ReverseMethodToAggregationExpressionTranslatorTests : LinqIntegrationTest<ReverseMethodToAggregationExpressionTranslatorTests.ClassFixture>
    {
        public ReverseMethodToAggregationExpressionTranslatorTests(ClassFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public void Enumerable_Reverse_should_work()
        {
            var collection = Fixture.Collection;

            var queryable = collection.AsQueryable().Select(x => x.A.Reverse());

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { _v : { $reverseArray : '$A' }, _id : 0 } }");

            var result = queryable.Single();
            result.Should().Equal(3, 2, 1);
        }

        [Fact]
        public void Queryable_Reverse_should_work()
        {
            var collection = Fixture.Collection;

            var queryable = collection.AsQueryable().Select(x => x.A.AsQueryable().Reverse());

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { _v : { $reverseArray : '$A' }, _id : 0 } }");

            var result = queryable.Single();
            result.Should().Equal(3, 2, 1);
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
                new C { Id = 1, A = new int[] { 1, 2, 3 } }
            ];
        }
    }
}
