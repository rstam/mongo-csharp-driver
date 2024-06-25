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
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.Linq;
using MongoDB.TestHelpers.XunitExtensions;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3Implementation.Jira
{
    public class CSharp5132Tests : Linq3IntegrationTest
    {
        [Theory]
        [ParameterAttributeData]
        public void Filter_in_projection_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = GetCollection(linqProvider);
            var builder = Builders<ProvideFeedback>.Projection;
            var request = new FeedbackGetRequestDto
            {
                ReviewType = "MBRVW",
                StatusFilters = new string[] { "APPROVED", "CONTESTED" }
            };
            var projection =
                builder.Expression(
                    f => new ProvideFeedback
                    {
                        Id = f.Id,
                        SessionId = f.SessionId,
                        IsPriority = f.IsPriority,
                        DocumentCreatedOn = f.DocumentCreatedOn,
                        DocumentCreatedBy = f.DocumentCreatedBy,
                        Reviews = f.Reviews.Where(review => review.Type == "MBRVW" && request.StatusFilters.Contains(review.StatusCode)).ToArray()
                    });

            var aggregate = collection.Aggregate<ProvideFeedback>()
                .Project(projection);

            if (linqProvider == LinqProvider.V2)
            {
                var exception = Record.Exception(() => Translate(collection, aggregate));
                exception.Should().BeOfType<NotSupportedException>();
            }
            else
            {
                var stages = Translate(collection, aggregate);
                AssertStages(
                    stages,
                    """
                    {
                        $project :
                        {
                            _id : "$_id",
                            session_id : "$session_id",
                            is_priority : "$is_priority",
                            document_created_on : "$document_created_on",
                            document_created_by : "$document_created_by",
                            reviews :
                            {
                                $filter :
                                {
                                    input : "$reviews",
                                    as : "review",
                                    cond :
                                    {
                                        $and :
                                        [   
                                            { $eq : ["$$review.type", "MBRVW"] },
                                            { $in : ["$$review.status_code", ["APPROVED", "CONTESTED"]] }
                                        ]
                                    }
                                }
                            }
                        }
                    }
                    """);

                var result = aggregate.Single();
                result.Id.Should().Be(1);
                result.SessionId.Should().Be(2);
                result.IsPriority.Should().BeTrue();
                result.DocumentCreatedOn.Should().Be(new DateTime(2024, 6, 25, 1, 2, 3, DateTimeKind.Utc));
                result.DocumentCreatedBy.Should().Be("John Doe");
                result.Reviews.Should().Equal(
                    new Review[]
                    {
                        new Review { Type = "MBRVW", StatusCode = "APPROVED" },
                        new Review { Type = "MBRVW", StatusCode = "CONTESTED" }
                    },
                    (x, y) => x.Type == y.Type && x.StatusCode == y.StatusCode);
            }
        }

        private IMongoCollection<ProvideFeedback> GetCollection(LinqProvider linqProvider)
        {
            var collection = GetCollection<ProvideFeedback>("test", linqProvider);
            var document = new ProvideFeedback
            {
                Id = 1,
                SessionId = 2,
                IsPriority = true,
                DocumentCreatedOn = new DateTime(2024, 6, 25, 1, 2, 3, DateTimeKind.Utc),
                DocumentCreatedBy = "John Doe",
                Reviews = new Review[]
                {
                    new Review { Type = "ABCDE", StatusCode = "APPROVED" },
                    new Review { Type = "MBRVW", StatusCode = "APPROVED" },
                    new Review { Type = "MBRVW", StatusCode = "CONTESTED" },
                    new Review { Type = "MBRVW", StatusCode = "OTHER" },
                }
            };
            CreateCollection(collection, document);
            return collection;
        }

        private class ProvideFeedback
        {
            public int Id { get; set; }
            [BsonElement("session_id")] public int SessionId { get; set; }
            [BsonElement("is_priority")] public bool IsPriority { get; set; }
            [BsonElement("document_created_on")] public DateTime DocumentCreatedOn { get; set; }
            [BsonElement("document_created_by")] public string DocumentCreatedBy { get; set; }
            [BsonElement("reviews")] public Review[] Reviews { get; set; }
        }

        public class Review
        {
            [BsonElement("type")] public string Type { get; set; }
            [BsonElement("status_code")] public string StatusCode { get; set; }
        }

        public class FeedbackGetRequestDto
        {
            public string ReviewType { get; set; }
            public string[] StatusFilters { get; set; }
        }
    }
}
