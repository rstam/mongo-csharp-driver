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
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3Implementation.Jira
{
    public class CSharp4818Tests : Linq3IntegrationTest
    {
        [Fact]
        public void GroupJoin_should_work()
        {
            var peopleCollection = GetPeopleCollection();
            var organizationsCollection = GetOrganizationsCollection();

            var queryable = peopleCollection.AsQueryable()
                .SelectMany(x => x.OrganizationIds.Select(
                    organizationId => new PersonDtoUnwound
                    {
                        Id = x.Id,
                        Name = x.Name,
                        OrganizationId = organizationId
                    }))
                .Join(
                    organizationsCollection.AsQueryable(),
                    person => person.OrganizationId,
                    organization => organization.Id,
                    (person, organization) => new PersonUnwound
                    {
                        Id = person.Id,
                        Name = person.Name,
                        Organization = organization
                    })
                .GroupBy(
                    x => x.Id,
                    (key, unwound) => new Person
                    {
                        Id = key,
                        Name = unwound.First().Name,
                        Organizations = new List<Organization>(unwound.Select(p => p.Organization))
                    });

            var stages = Translate(peopleCollection, queryable);
            AssertStages(
                stages,
                "{ $project : { _v : { $map : { input : '$OrganizationIds', as : 'organizationId', in : { _id : '$_id', Name : '$Name', OrganizationId : '$$organizationId' } } }, _id : 0 } }",
                "{ $unwind : '$_v' }",
                "{ $project : { _outer : '$_v', _id : 0 } }",
                "{ $lookup : { from : 'organizations', localField : '_outer.OrganizationId', foreignField : '_id', as : '_inner' } }",
                "{ $unwind : '$_inner' }",
                "{ $project: { _id : '$_outer._id', Name : '$_outer.Name', Organization : '$_inner' } }",
                "{ $group : { _id : '$_id', __agg0 : { $first : '$$ROOT' }, __agg1 : { $push : '$Organization' } } }",
                "{ $project : { _id : '$_id', Name : '$__agg0.Name', Organizations : '$__agg1' } }");

            var results = queryable.ToList().OrderBy(x => x.Id).ToList();
            results.Should().HaveCount(2);

            results[0].Id.Should().Be(1);
            results[0].Name.Should().Be("Adam");
            results[0].Organizations.Should().HaveCount(1);
            results[0].Organizations[0].Id.Should().Be(1);
            results[0].Organizations[0].Name.Should().Be("Alfa");

            results[1].Id.Should().Be(2);
            results[1].Name.Should().Be("Beth");
            results[1].Organizations.Should().HaveCount(2);
            results[1].Organizations[0].Id.Should().Be(1);
            results[1].Organizations[0].Name.Should().Be("Alfa");
            results[1].Organizations[1].Id.Should().Be(2);
            results[1].Organizations[1].Name.Should().Be("Beta");
        }

        private IMongoCollection<PersonDto> GetPeopleCollection()
        {
            var collection = GetCollection<PersonDto>("people");
            CreateCollection(
                collection,
                new PersonDto { Id = 1, Name = "Adam", OrganizationIds = new() { 1 } },
                new PersonDto { Id = 2, Name = "Beth", OrganizationIds = new() { 1, 2 } });
            return collection;
        }

        private IMongoCollection<Organization> GetOrganizationsCollection()
        {
            var collection = GetCollection<Organization>("organizations");
            CreateCollection(
                collection,
                new Organization { Id = 1, Name = "Alfa" },
                new Organization { Id = 2, Name = "Beta" });
            return collection;
        }

        class PersonDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<int> OrganizationIds { get; set; }
        }

        class PersonDtoUnwound
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int OrganizationId { get; set; }
        }

        class Organization
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        class PersonUnwound
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public Organization Organization { get; set; }
        }

        class Person
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<Organization> Organizations { get; set; }
        }
    }
}
