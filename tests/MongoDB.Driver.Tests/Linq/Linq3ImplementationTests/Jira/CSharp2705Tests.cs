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
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentAssertions;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Driver.Linq;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3ImplementationTests.Jira
{
    public class CSharp2705Tests : Linq3IntegrationTest
    {
        [Fact]
        public void Week_should_work()
        {
            var collection = CreateCollection();
            var invitees = new[] { "Bob", "John", "Tim", "Ryan" };
            var roles = new[] { "party_planning", "food_preparation" };

            var queryable = collection
                .AsQueryable()
                .Where(
                    x =>
                        x.Invitees.Organisers.Any(a => invitees.Contains(a)) ||
                        (
                            x.Invitees.Roles.Any(y => invitees.Contains(y.Key) && roles.Any(r => y.Value.Contains(r))) &&
                            !(x.Invitees.BlacklistRoles.Any(y => invitees.Contains(y.Key) && roles.Any(r => y.Value.Contains(r))))
                        ));

            var stages = Translate(collection, queryable);
            AssertStages(
                stages,
                @"
                {
                    $match : {
                        $or: [
                            {
                                'Invitees.Organisers': {
                                    $in: [
                                        'Bob',
                                        'John',
                                        'Tim',
                                        'Ryan'
                                    ]
                                }
                            },
                            {
                                'Invitees.Roles': {
                                    $elemMatch: {
                                        k: {
                                            $in: [
                                                'Bob',
                                                'John',
                                                'Tim',
                                                'Ryan'
                                            ]
                                        },
                                        v: {
                                            $in: [
                                                'party_planning', 'food_preparation'
                                            ]
                                        }
                                    }
                                },
                                'Invitees.BlacklistRoles': {
                                    $not: {
                                        $elemMatch: {
                                            k: {
                                                $in: [
                                                    'Bob',
                                                    'John',
                                                    'Tim',
                                                    'Ryan'
                                                ]
                                            },
                                            v: {
                                                $in: [
                                                    'party_planning', 'food_preparation'
                                                ]
                                            }
                                        }
                                    }
                                }
                            }
                        ]
                    }
                }"
                );

            var results = queryable.ToList();
            results.Should().Equal(0, 0);
        }

        private IMongoCollection<C> CreateCollection()
        {
            var collection = GetCollection<C>("C");

            CreateCollection(
                collection,
                new C { Id = 1 },
                new C { Id = 2 });

            return collection;
        }

        private class C
        {
            public int Id { get; set; }
            public Invitee Invitees { get; set; }
        }

        private class Invitee
        {
            public string[] Organisers { get; set; }
            [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)] public Dictionary<string, string[]> Roles { get; set; }
            [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)] public Dictionary<string, string[]> BlacklistRoles { get; set; }
        }
    }
}
