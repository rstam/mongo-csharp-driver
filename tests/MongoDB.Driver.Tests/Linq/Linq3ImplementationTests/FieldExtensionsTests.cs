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
using MongoDB.Driver.Linq;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3ImplementationTests
{
    public class FieldExtensionsTests
    {
        [Fact]
        public void Exists_in_filter_should_work()
        {
            var collection = GetCollection();
            var subject = collection.AsQueryable();

            var queryable = subject.Where(x => x.F.Exists());

            var stages = Linq3TestHelpers.Translate(collection, queryable);
            var expectedStages = new[]
            {
                "{ $match : { F : { $exists : true } } }"
            };
            Linq3TestHelpers.AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Exists_in_expression_should_work()
        {
            var collection = GetCollection();
            var subject = collection.AsQueryable();

            var queryable = subject.Select(x => x.F.Exists());

            var stages = Linq3TestHelpers.Translate(collection, queryable);
            var expectedStages = new[]
            {
                "{ $project : { _v : { $ne : ['$F', undefined] }, _id : 0 } }"
            };
            Linq3TestHelpers.AssertStages(stages, expectedStages);
        }

        private IMongoCollection<C> GetCollection()
        {
            var client = DriverTestConfiguration.Linq3Client;
            var database = client.GetDatabase(DriverTestConfiguration.DatabaseNamespace.DatabaseName);
            return database.GetCollection<C>(DriverTestConfiguration.CollectionNamespace.CollectionName);
        }

        private class C
        {
            public string F { get; set; }
        }
    }
}
