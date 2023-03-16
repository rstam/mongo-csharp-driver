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
using MongoDB.Driver.Linq;
using MongoDB.TestHelpers.XunitExtensions;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3ImplementationTests.Jira
{
    public class CSharp4559Tests : Linq3IntegrationTest
    {
        [Theory]
        [ParameterAttributeData]
        public void Where_example_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = CreateCollection(linqProvider);
            var dto = new Dto { Client = "acme" };

            var queryable = collection
                .AsQueryable()
                .Where(x =>
                    x.ClientName.ToLower().Contains(dto.Client) ||
                    x.ClientEmail.ToLower().Contains(dto.Client) ||
                    x.AdditionalClients.Any(c =>
                        c.ClientName.ToLower().Contains(dto.Client) ||
                        c.ClientEmail.ToLower().Contains(dto.Client)));

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $match : { $or : [{ ClientName : /acme/is }, { ClientEmail : /acme/is }, { AdditionalClients : { $elemMatch : { $or : [{ ClientName : /acme/is }, { ClientEmail : /acme/is }] } } }] } }");

            var results = queryable.ToList();
            results.Select(x => x.Id).Should().Equal(1, 2, 3, 4);
        }

        private IMongoCollection<Client> CreateCollection(LinqProvider linqProvider)
        {
            var collection = GetCollection<Client>("test", linqProvider);

            CreateCollection(
                collection,
                new Client { Id = 1, ClientName = "Acme Inc" },
                new Client { Id = 2, ClientEmail = "Acme.com" },
                new Client { Id = 3, AdditionalClients = new[] { new Client { Id = 0, ClientName = "Acme Inc" } } },
                new Client { Id = 4, AdditionalClients = new[] { new Client { Id = 0, ClientEmail = "Acme.com" } } },
                new Client { Id = 5 });

            return collection;
        }

        private class Client
        {
            public int Id { get; set; }
            public string ClientName { get; set; }
            public string ClientEmail { get; set; }
            public Client[] AdditionalClients { get; set; }
        }

        private class Dto
        {
            public string Client { get; set; }
        }
    }
}
