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
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3Implementation.Jira
{
    public class CSharp5240Tests : Linq3IntegrationTest
    {
        [Fact]
        public void Set_should_work()
        {
            var collection = GetCollection();
            var pipeline = new EmptyPipelineDefinition<Document>()
                .Set(x => new Document()
                {
                    SubDocuments = x.SubDocuments
                        .Where(y => y.PartnerId == "123").ToList(),
                });

            var stages = Translate(pipeline, collection.DocumentSerializer, translationOptions: null);
            AssertStages(stages, "{ $set : { SubDocuments : { $filter : { input : '$SubDocuments', as : 'y', cond : { $eq : ['$$y.PartnerId', '123'] } } } } }");

            var result = collection.Aggregate(pipeline).Single();
            result.SubDocuments.Select(x => x.Id).Should().Equal("a", "b");
        }

        private IMongoCollection<Document> GetCollection()
        {
            var collection = GetCollection<Document>("test");
            var subDocuments = new List<SubDocument>()
            {
                new SubDocument { Id = "a", PartnerId = "123" },
                new SubDocument { Id = "b", PartnerId = "123" },
                new SubDocument { Id = "c", PartnerId = "456" }

            };
            CreateCollection(
                collection,
                new Document { Id = 1, SubDocuments = subDocuments });
            return collection;
        }

        public class Document
        {
            public int Id { get; set; }
            public List<SubDocument> SubDocuments { get; set; } = new();
        }

        public class SubDocument
        {
            public string Id { get; set; } = null!;
            public string PartnerId { get; set; } = null!;
        }
    }
}
