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
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3Implementation.Jira
{
    public class CSharp5431Tests : Linq3IntegrationTest
    {
        [Fact]
        public void Select_decimal_divide_should_work()
        {
            var collection = GetCollection();

            var queryable = collection.AsQueryable()
                .Where(x => x.Status != "invalid" && x.Id != "5f0fd5e708262bdc7e2d01fc" && string.IsNullOrWhiteSpace(x.FileId) == false);

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $match : { Status : { $ne : 'invalid' }, _id : { $ne : '5f0fd5e708262bdc7e2d01fc' }, FileId : { $nin : [null, { $regularExpression : { pattern : '^\\\\s*$', options : '' } }] } } }");

            var results = queryable.ToList();
            results.Select(x => x.Id).Should().Equal("444444444444444444444444");
        }

        private IMongoCollection<MongoFileDto> GetCollection()
        {
            var collection = GetCollection<MongoFileDto>("test");
            CreateCollection(
                collection,
                new MongoFileDto { Id = "5f0fd5e708262bdc7e2d01fc", Status = "invalid", FileId = null },
                new MongoFileDto { Id = "111111111111111111111111", Status = "invalid", FileId = null },
                new MongoFileDto { Id = "222222222222222222222222", Status = "valid", FileId = null },
                new MongoFileDto { Id = "333333333333333333333333", Status = "valid", FileId = " " },
                new MongoFileDto { Id = "444444444444444444444444", Status = "valid", FileId = "abc" });
            return collection;
        }

        private class MongoFileDto
        {
            public string Id { get; set; }
            public string Status { get; set; }
            public string FileId { get; set; }
        }
    }
}
