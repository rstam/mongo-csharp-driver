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
using MongoDB.Driver.Linq.Linq3Implementation;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3ImplementationTests.Jira
{
    public class CSharp3911Tests
    {
        [Fact]
        public void OrderBy_only_works_on_fields()
        {
            var client = DriverTestConfiguration.Linq3Client;
            var database = client.GetDatabase(DriverTestConfiguration.DatabaseNamespace.DatabaseName);
            var collection = database.GetCollection<Entity>("csharp3911");
            var utcNow = DateTime.UtcNow;

            var queryable = collection.AsQueryable()
                .Where(n => !n.StartDateUtc.HasValue || n.StartDateUtc <= utcNow)
                .Where(n => !n.EndDateUtc.HasValue || n.EndDateUtc >= utcNow)
                .OrderByDescending(n => n.StartDateUtc ?? n.CreatedOnUtc);

            var exception = Record.Exception(() => Linq3TestHelpers.Translate(collection, queryable));

            exception.Should().BeOfType<ExpressionNotSupportedException>();
        }

        [Fact]
        public void OrderBy_using_temporary_field_to_order_by_expression()
        {
            var client = DriverTestConfiguration.Linq3Client;
            var database = client.GetDatabase(DriverTestConfiguration.DatabaseNamespace.DatabaseName);
            var collection = database.GetCollection<Entity>("csharp3911");
            var utcNow = DateTime.UtcNow;

            var queryable = collection.AsQueryable()
                .Where(n => !n.StartDateUtc.HasValue || n.StartDateUtc <= utcNow)
                .Where(n => !n.EndDateUtc.HasValue || n.EndDateUtc >= utcNow)
                .Select(n => new { Document = n, TempOrderByField = n.StartDateUtc ?? n.CreatedOnUtc })
                .OrderBy(n => n.TempOrderByField)
                .Select(n => n.Document);

            var stages = Linq3TestHelpers.Translate(collection, queryable);
        }

        public class Entity
        {
            public int Id { get; set; }
            public DateTime? CreatedOnUtc { get; set; }
            public DateTime? StartDateUtc { get; set; }
            public DateTime? EndDateUtc { get; set; }
        }
    }
}
