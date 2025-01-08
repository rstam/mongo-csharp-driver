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
using FluentAssertions;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3Implementation.Jira
{
    public class CSharp5321Tests : Linq3IntegrationTest
    {
        [Fact]
        public void Client_side_projection_should_fetch_only_needed_fields()
        {
            var collection = GetCollection();
            var enableClientSideProjections = new ExpressionTranslationOptions { EnableClientSideProjections = true };

            var queryable = collection.AsQueryable(enableClientSideProjections)
                .Select(x => Add(x.X, 1));

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { _0 : '$X', _id : 0 } }");

            var result = queryable.Single();
            result.Should().Be(2);
        }

        [Fact]
        public void Client_side_projection_should_compute_sum_server_side()
        {
            var collection = GetCollection();
            var enableClientSideProjections = new ExpressionTranslationOptions { EnableClientSideProjections = true };

            var queryable = collection.AsQueryable(enableClientSideProjections)
                .Select(x => Add(x.A.Sum(), 4));

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { _0 : { $sum : '$A' }, _id : 0 } }");

            var result = queryable.Single();
            result.Should().Be(10);
        }

        private IMongoCollection<C> GetCollection()
        {
            var collection = GetCollection<C>("test");
            CreateCollection(
                collection,
                new C { Id = 1, X = 1, A = [1, 2, 3] });
            return collection;
        }

        private static int Add(int x, int y) => x + y;

        private class C
        {
            public int Id { get; set; }
            public int X { get; set; }
            public int[] A { get; set; }
        }
    }
}
