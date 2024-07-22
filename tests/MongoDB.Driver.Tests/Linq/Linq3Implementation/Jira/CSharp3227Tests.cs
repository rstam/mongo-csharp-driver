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
using FluentAssertions;
using MongoDB.Driver.Linq;
using MongoDB.TestHelpers.XunitExtensions;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3Implementation.Jira
{
    public class CSharp3227Tests : Linq3IntegrationTest
    {
        [Theory]
        [ParameterAttributeData]
        public void Find_DummyContainer_with_Projection_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = GetCollection1(linqProvider);

            var find = collection.Find("{}")
                .Project(
                    Builders<DummyContainer>.Projection.Expression(x =>
                        new DummyContainerWithDummyNames
                        {
                            ContainerId = x.Id,
                            DummyNames = x.Dummies.Select(d => d.Name)
                        }));

            var renderedProjection = TranslateFindProjection(collection, find);
            if (linqProvider == LinqProvider.V2)
            {
                renderedProjection.Should().Be("{ _id : 1, 'Dummies.Name' : 1 }"); // client-side projection
            }
            else
            {
                renderedProjection.Should().Be("{ ContainerId : '$_id', DummyNames : '$Dummies.Name', _id : 0 }"); // server-side projection
            }

            var result = find.Single();
            result.ContainerId.Should().Be(1);
            result.DummyNames.Should().Equal("Two", "Three");
        }

        [Theory]
        [ParameterAttributeData]
        public void Select_DummyContainer_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = GetCollection1(linqProvider);

            var queryable = collection.AsQueryable()
                .Select(
                    x => new DummyContainerWithDummyNames
                    {
                        ContainerId = x.Id,
                        DummyNames = x.Dummies.Select(d => d.Name)
                    });

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { ContainerId : '$_id', DummyNames : '$Dummies.Name', _id : 0 } }");

            var result = queryable.Single();
            result.ContainerId.Should().Be(1);
            result.DummyNames.Should().Equal("Two", "Three");
        }

        [Theory]
        [ParameterAttributeData]
        public void Find_DummyContainerWrapper_with_Projection_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = GetCollection2(linqProvider);

            var find = collection.Find("{}")
                .Project(
                    Builders<DummyContainerWrapper>.Projection.Expression(x =>
                        new DummyContainerWithDummyNames
                        {
                            ContainerId = x.Id,
                            DummyNames = x.Container.Dummies.Select(d => d.Name)
                        }));

            var renderedProjection = TranslateFindProjection(collection, find);
            if (linqProvider == LinqProvider.V2)
            {
                renderedProjection.Should().Be("{ _id : 1, 'Container.Dummies' : 1, 'Dummies.Name' : 1 }"); // LINQ2 projection is wrong
            }
            else
            {
                renderedProjection.Should().Be("{ ContainerId : '$_id', DummyNames : '$Container.Dummies.Name', _id : 0 }"); // server-side projection
            }

            var result = find.Single();
            result.ContainerId.Should().Be(1);
            if (linqProvider == LinqProvider.V2)
            {
                result.DummyNames.Should().Equal(null, null); // LINQ2 result is wrong
            }
            else
            {
                result.DummyNames.Should().Equal("Two", "Three");
            }
        }

        [Theory]
        [ParameterAttributeData]
        public void Select_DummyContainerWrapper_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = GetCollection2(linqProvider);

            var queryable = collection.AsQueryable()
                .Select(
                    x => new DummyContainerWithDummyNames
                    {
                        ContainerId = x.Id,
                        DummyNames = x.Container.Dummies.Select(d => d.Name)
                    });

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { ContainerId : '$_id', DummyNames : '$Container.Dummies.Name', _id : 0 } }");

            var result = queryable.Single();
            result.ContainerId.Should().Be(1);
            result.DummyNames.Should().Equal("Two", "Three");
        }

        private IMongoCollection<DummyContainer> GetCollection1(LinqProvider linqProvider)
        {
            var collection = GetCollection<DummyContainer>("test", "test", linqProvider);
            CreateCollection(
                collection,
                new DummyContainer
                {
                    Id = 1,
                    Dummies = new[]
                    {
                        new Dummy { Id = 2, Name = "Two" },
                        new Dummy { Id = 3, Name = "Three" }
                    }
                });
            return collection;
        }

        private IMongoCollection<DummyContainerWrapper> GetCollection2(LinqProvider linqProvider)
        {
            var collection = GetCollection<DummyContainerWrapper>("test", "test", linqProvider);
            CreateCollection(
                collection,
                new DummyContainerWrapper
                {
                    Id = 1,
                    Container = new DummyContainer
                    {
                        Id = 1,
                        Dummies = new[]
                        {
                            new Dummy { Id = 2, Name = "Two" },
                            new Dummy { Id = 3, Name = "Three" }
                        }
                    }
                });
            return collection;
        }

        private class DummyContainer
        {
            public int Id { get; set; }
            public IList<Dummy> Dummies { get; set; }
        }

        public class Dummy
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class DummyContainerWithDummyNames
        {
            public int ContainerId { get; set; }
            public IEnumerable<string> DummyNames { get; set; }
        }

        private class DummyContainerWrapper
        {
            public int Id { get; set; }
            public DummyContainer Container { get; set; }
        }
    }
}
