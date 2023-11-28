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

using System.Linq;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using MongoDB.TestHelpers.XunitExtensions;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3Implementation.Jira
{
    public class CSharp4852Tests : Linq3IntegrationTest
    {
        [Theory]
        [ParameterAttributeData]
        public void Find_with_client_side_Projection_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = GetCollection(linqProvider);
            var filter = Builders<MainClass>.Filter.Empty;

            var find = collection.Find(filter);

            var results = find
                .ToEnumerable()
                .Select(main => new ProjectedMainClassWithoutSecretData()
                {
                    Id = main.Id,
                    Name = main.Name,
                    OptionalData = main.OptionalData != null ?
                        new ProjectedOptionalClassWithoutSecretData()
                        {
                            PublicData = main.OptionalData.PublicData
                        } :
                        null,
                    HasOptionalPrivateData = !string.IsNullOrEmpty(main.OptionalData?.PrivateData)
                })
                .ToList();

            results.Select(x => x.Id).Should().Equal(1, 2, 3, 4, 5, 6);
            results.Select(x => x.Name).Should().Equal("A", "B", "C", "D", "E", "F");
            results.Where(x => x.OptionalData == null).Select(x => x.Id).Should().Equal(1, 2);
            results.Where(x => x.OptionalData != null).Select(x => x.OptionalData.PublicData).Should().Equal("C", "D", "E", "F");
            results.Select(x => x.HasOptionalPrivateData).Should().Equal(false, false, false, false, false, true);
        }

        [Fact]
        public void Find_with_server_side_Projection_using_LINQ3_should_work()
        {
            var collection = GetCollection(LinqProvider.V3);
            var filter = Builders<MainClass>.Filter.Empty;
            var projection = Builders<MainClass>.Projection.Expression(main => new ProjectedMainClassWithoutSecretData()
            {
                Id = main.Id,
                Name = main.Name,
                OptionalData = main.OptionalData != null ?
                    new ProjectedOptionalClassWithoutSecretData()
                    {
                        PublicData = main.OptionalData.PublicData
                    } :
                    null,
                HasOptionalPrivateData = main.OptionalData != null ?
                    !string.IsNullOrEmpty(main.OptionalData.PrivateData) :
                    false
            });

            var find = collection.Find(filter).Project(projection);

            var translatedProjection = TranslateFindProjection(collection, find);
            var results = find.ToList();

            translatedProjection.Should().Be(
                @"{
                    _id : 1,
                    Name : 1,
                    OptionalData : { $cond : {
                        if : { $ne : ['$OptionalData', null] },
                        then : { PublicData : '$OptionalData.PublicData' },
                        else : null
                    }},
                    HasOptionalPrivateData : { $cond : {
                        if : { $ne : ['$OptionalData', null] },
                        then : { $not : { $in : ['$OptionalData.PrivateData', [null, '']] } },
                        else : false
                    }}
                }");

            // missing OptionalData or missing PrivateData gives unexpected results
            results.Select(x => x.Id).Should().Equal(1, 2, 3, 4, 5, 6);
            results.Select(x => x.Name).Should().Equal("A", "B", "C", "D", "E", "F");
            results.Where(x => x.OptionalData == null).Select(x => x.Id).Should().Equal(2);
            results.Where(x => x.OptionalData != null).Select(x => x.OptionalData.PublicData).Should().Equal(null, "C", "D", "E", "F");
            results.Select(x => x.HasOptionalPrivateData).Should().Equal(true, false, true, false, false, true);
        }

        private IMongoCollection<MainClass> GetCollection(LinqProvider linqProvider)
        {
            var collection = GetCollection<MainClass>("test", linqProvider);
            CreateCollection(
                GetCollection<BsonDocument>("test"),
                BsonDocument.Parse("{ _id : 1, Name : 'A' }"),
                BsonDocument.Parse("{ _id : 2, Name : 'B', OptionalData : null }"),
                BsonDocument.Parse("{ _id : 3, Name : 'C', OptionalData : { PublicData : 'C' } }"),
                BsonDocument.Parse("{ _id : 4, Name : 'D', OptionalData : { PublicData : 'D', PrivateData : null } }"),
                BsonDocument.Parse("{ _id : 5, Name : 'E', OptionalData : { PublicData : 'E', PrivateData : '' } }"),
                BsonDocument.Parse("{ _id : 6, Name : 'F', OptionalData : { PublicData : 'F', PrivateData : 'F' } }")
                );
            return collection;
        }

        public class MainClass
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public OptionalClass OptionalData { get; set; }
        }

        public class OptionalClass
        {
            public string PublicData { get; set; }
            public string PrivateData { get; set; }
        }

        public class ProjectedMainClassWithoutSecretData
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public ProjectedOptionalClassWithoutSecretData OptionalData { get; set; }
            public bool HasOptionalPrivateData { get; set; }
        }

        public class ProjectedOptionalClassWithoutSecretData
        {
            public string PublicData { get; set; }
        }
    }
}
