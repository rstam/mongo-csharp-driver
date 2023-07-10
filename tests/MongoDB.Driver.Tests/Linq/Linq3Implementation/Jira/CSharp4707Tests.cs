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
using System;
using FluentAssertions;
using MongoDB.Driver.Linq;
using MongoDB.TestHelpers.XunitExtensions;
using Xunit;
using System.Linq;

namespace MongoDB.Driver.Tests.Linq.Linq3Implementation.Jira
{
    public class CSharp4707Tests : Linq3IntegrationTest
    {
        [Theory]
        [ParameterAttributeData]
        public void Project_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = GetCollection(linqProvider);
            var projection = Builders<TaskEntity>.Projection
                .Expression(e => new TaskHeader
                {
                    Id = e.Id,
                    Name = e.Name,
                    LinkCount = e.Links != null ? e.Links.Length : 0
                });

            var aggregate = collection.Aggregate()
                .Project(projection);

            var stages = Translate(collection, aggregate);
            if (linqProvider == LinqProvider.V2)
            {
                AssertStages(stages, "{ $project : { Id : '$_id', Name : '$Name', LinkCount : { $cond : [{ $ne : ['$Links', null] }, { $size : '$Links' }, 0] }, _id : 0 } }");
            }
            else
            {
                AssertStages(stages, "{ $project : { _id : '$_id', Name : '$Name', LinkCount : { $cond : { if : { $ne : ['$Links', null] }, then : { $size : '$Links' }, else : 0 } } } }");
            }

            var results = aggregate.ToList().OrderBy(x => x.Id).ToList();
            results.Count.Should().Be(4);
            results.Select(x => x.Id).Should().Equal(1, 2, 3, 4);
            results.Select(x => x.Name).Should().Equal("Task 1", "Task 2", "Task 3", "Task 4");
            results.Select(x => x.LinkCount).Should().Equal(2, 4, 0, 0);
        }

        [Theory]
        [ParameterAttributeData]
        public void Project_in_facet_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = GetCollection(linqProvider);
            var projection = Builders<TaskEntity>.Projection
                .Expression(x => new TaskHeader
                {
                    Id = x.Id,
                    Name = x.Name,
                    LinkCount = x.Links != null ? x.Links.Length : 0
                });

            var countFacet = AggregateFacet.Create(
                "count",
                PipelineDefinition<TaskEntity, AggregateCountResult>.Create(
                    new[]
                    {
                        PipelineStageDefinitionBuilder.Count<TaskEntity>()
                    }));
            var dataFacet = AggregateFacet.Create(
                "data",
                PipelineDefinition<TaskEntity, TaskHeader>.Create(
                    new[]
                    {
                        PipelineStageDefinitionBuilder.Project(projection)
                    }));

            var aggregate = collection.Aggregate().Facet(countFacet, dataFacet);

            var stages = Translate(collection, aggregate);
            if (linqProvider == LinqProvider.V2)
            {
                var expectedStage =
                    "{" +
                    "    $facet : " +
                    "    {" +
                    "        count : [{ $count : 'count' }], " +
                    "        data : [{ $project : { Id : '$_id', Name : '$Name', LinkCount : { $cond : [{ $ne : ['$Links', null] }, { $size : '$Links' }, 0] }, _id : 0 } }] " +
                    "    }" +
                    "}";
                AssertStages(stages, expectedStage);

                var exception = Record.Exception(() => aggregate.ToList());
                exception.Should().BeOfType<FormatException>();
            }
            else
            {
                var expectedStage =
                    "{" +
                    "    $facet : " +
                    "    {" +
                    "        count : [{ $count : 'count' }], " +
                    "        data : [{ $project : { _id : '$_id', Name : '$Name', LinkCount : { $cond : { if : { $ne : ['$Links', null] }, then : { $size : '$Links' }, else : 0 } } } }] " +
                    "    }" +
                    "}";
                AssertStages(stages, expectedStage);

                var results = aggregate.ToList();
            }
        }

        private IMongoCollection<TaskEntity> GetCollection(LinqProvider linqProvider)
        {
            var collection = GetCollection<TaskEntity>("test", linqProvider);
            var tasksToInsert = new List<TaskEntity>
            {
                new TaskEntity { Id = 1, Name = "Task 1", Links = new string[2] {"link1", "link2"} },
                new TaskEntity { Id = 2, Name = "Task 2", Links = new string[4] {"link3", "link4", "link5", "link6"} },
                new TaskEntity { Id = 3, Name = "Task 3", Links = Array.Empty<string>() },
                new TaskEntity { Id = 4, Name = "Task 4", Links = null }
            };
            CreateCollection(collection, tasksToInsert);
            return collection;
        }

        private class TaskEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string[] Links { get; set; }
        }

        private class TaskHeader
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int LinkCount { get; set; }
        }
    }
}
