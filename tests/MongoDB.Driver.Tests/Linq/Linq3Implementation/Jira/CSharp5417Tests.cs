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

using System.Runtime.Serialization;
using FluentAssertions;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3Implementation.Jira
{
    public class CSharp5417Tests : Linq3IntegrationTest
    {
        [Fact]
        public void Filter_Eq_should_work()
        {
            var collection = GetCollection();
            var filter = Builders<Project>.Filter.Eq(s => s.ProjectStatus, Project.ProjectStatuses.CLOSED);

            var renderedFilter = TranslateFilter<Project>(collection, filter);
            renderedFilter.Should().Be("{ ProjectStatus : 1 }");

            var result = collection.CountDocuments(filter);
            result.Should().Be(0); // count is 0 because I tested against an empty collection
        }

        [Fact]
        public void Filter_Nin_should_work()
        {
            var collection = GetCollection();
            var filter = Builders<Project>.Filter.Nin(s => s.ProjectStatus, [Project.ProjectStatuses.CLOSED]);

            var renderedFilter = TranslateFilter<Project>(collection, filter);
            renderedFilter.Should().Be("{ ProjectStatus : { $nin : [1] } }");

            var result = collection.CountDocuments(filter);
            result.Should().Be(0); // count is 0 because I tested against an empty collection
        }

        [Fact]
        public void Filter_In_should_work()
        {
            var collection = GetCollection();
            var filter = Builders<Project>.Filter.In(s => s.ProjectStatus, [Project.ProjectStatuses.CLOSED]);

            var renderedFilter = TranslateFilter<Project>(collection, filter);
            renderedFilter.Should().Be("{ ProjectStatus : { $in : [1] } }");

            var result = collection.CountDocuments(filter);
            result.Should().Be(0); // count is 0 because I tested against an empty collection
        }

        private IMongoCollection<Project> GetCollection()
        {
            var collection = GetCollection<Project>("test");
            // CreateCollection(
            //     collection,
            //     new C { Id = 1, D = 1000.0M });
            return collection;
        }

        public class Project
        {
#pragma warning disable CA1717
            public enum ProjectStatuses
#pragma warning restore CA1717
            {
                [EnumMember(Value = "On hold")]
                ONHOLD,
                [EnumMember(Value = "Closed")]
                CLOSED,
            };

            public int Id { get; set; }
            public ProjectStatuses? ProjectStatus { get; set; }
        }
    }
}
