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
using FluentAssertions;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3Implementation.Jira
{
    public class InvalidCastToIBsonDocumentSerializerTests : Linq3IntegrationTest
    {
        static InvalidCastToIBsonDocumentSerializerTests()
        {
            var objectSerializer = new ObjectSerializer((Type _) => true);
            BsonSerializer.RegisterSerializer(objectSerializer);
        }

        [Fact]
        public void Find_CountDocuments_should_work()
        {
            var collection = GetCollection();
            var find = collection.Find(_ => true);

            var result = find.CountDocuments();

            result.Should().Be(1);
        }

        private IMongoCollection<dynamic> GetCollection()
        {
            var collection = GetCollection<dynamic>("test");
            CreateCollection(
                collection,
                new C { Id = 1, X = 1 });
            return collection;
        }

        private class C
        {
            public int Id { get; set; }
            public int X { get; set; }
        }
    }
}
