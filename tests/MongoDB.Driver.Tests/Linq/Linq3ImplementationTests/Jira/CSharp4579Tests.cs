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
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.Linq;
using MongoDB.TestHelpers.XunitExtensions;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3ImplementationTests.Jira
{
    public class CSharp4579Tests : Linq3IntegrationTest
    {
        [Theory]
        [ParameterAttributeData]
        public void Project_id_only_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = CreateCollection(linqProvider);

            var pipeline = new EmptyPipelineDefinition<Derived>()
                .Project(x => new Derived
                {
                    Id = x.Id,
                });

            var stages = Translate(collection, pipeline);
            AssertStages(stages, "{ $project : { Id : '$Id', _id : 0 } }");

            var results = collection.Aggregate(pipeline).ToList();
            results.Should().HaveCount(1);
            results[0].UniqueId.Should().Be(0); // value is 0 because it was not projected
            results[0].Id.Should().Be(2);
        }

        [Theory]
        [ParameterAttributeData]
        public void Project_all_properties_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = CreateCollection(linqProvider);

            var pipeline = new EmptyPipelineDefinition<Derived>()
                .Project(x => new Derived
                {
                    UniqueId = x.UniqueId,
                    Id = x.Id,
                });

            if (linqProvider == LinqProvider.V2)
            {
                var stages = Translate(collection, pipeline);
                AssertStages(stages, "{ $project : { UniqueId : '$_id', Id : '$Id', _id : 0 } }");

                var exception = Record.Exception(() => collection.Aggregate(pipeline).ToList());
                exception.Should().BeOfType<FormatException>(); // this scenario is not supported in LINQ2
                exception.Message.Should().Contain("Element 'UniqueId' does not match any field or property");
            }
            else
            {
                var stages = Translate(collection, pipeline);
                AssertStages(stages, "{ $project : { _id : '$_id', Id : '$Id' } }");

                var results = collection.Aggregate(pipeline).ToList();
                results.Should().HaveCount(1);
                results[0].UniqueId.Should().Be(1);
                results[0].Id.Should().Be(2);
            }
        }

        private IMongoCollection<Derived> CreateCollection(LinqProvider linqProvider)
        {
            var collection = GetCollection<Derived>("test", linqProvider);
            CreateCollection(
                collection,
                new Derived { UniqueId = 1, Id = 2 });
            return collection;
        }

        public abstract class Base
        {
            [BsonId]
            [BsonElement("_id")]
            public int UniqueId { get; set; }
        }

        [BsonNoId]
        public class Derived : Base
        {
            public int Id { get; set; }
        }
    }
}
