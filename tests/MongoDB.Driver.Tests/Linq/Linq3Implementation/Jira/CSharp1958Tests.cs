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
using MongoDB.Driver.Linq;
using MongoDB.TestHelpers.XunitExtensions;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3Implementation.Jira
{
    public class CSharp1958Tests : Linq3IntegrationTest
    {
        [Theory]
        [ParameterAttributeData]
        public void Any_should_not_return_whole_document(
            [Values(0, 1, 2)] int n,
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = GetCollection(n, linqProvider);
            var queryable = collection.AsQueryable();

            if (linqProvider == LinqProvider.V2)
            {
                var result = true;
                var exception = Record.Exception(() => result = queryable.Any());

                if (n == 0)
                {
                    result.Should().BeFalse();
                }
                else
                {
                    exception.Should().BeOfType<FormatException>(); // attempted to deserialize invalid document
                }
            }
            else
            {
                var (stages, result) = ExecuteQueryCapturingStages(
                    queryable,
                    queryable => queryable.Any());

                AssertStages(
                    stages,
                    "{ $limit : 1 }",
                    "{ $project : { _id : 0, _v : null } }");

                var expectedResult = n > 0;
                result.Should().Be(expectedResult);
            }
        }

        private IMongoCollection<C> GetCollection(int n, LinqProvider linqProvider)
        {
            var collection = GetCollection<C>("test", linqProvider);
            var bsonDocumentCollection = collection.Database.GetCollection<BsonDocument>("test");
            var invalidDocuments = Enumerable.Range(1, n).Select(id => new BsonDocument("_id", new string((char)('a' + id), 1)));
            CreateCollection(bsonDocumentCollection, invalidDocuments);
            return collection;
        }

        private class C
        {
            public int Id { get; set; }
            public int X { get; set; }
        }
    }
}
