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
using System.Linq.Expressions;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3ImplementationTests.Jira
{
    public class CSharp4519Tests : Linq3IntegrationTest
    {
        [Fact]
        public void Example_should_work()
        {
            var collection = CreateCollection();

            var updateDefinition = GetUpdateFieldDefinition((t => t.Type, "super-fast"), (t => t.Price, 100m));

            var serializerRegistry = BsonSerializer.SerializerRegistry;
            var carSerializer = serializerRegistry.GetSerializer<Car>();
            var rendered = (BsonDocument)updateDefinition.Render(carSerializer, serializerRegistry, LinqProvider.V3);
            rendered.Should().Be("{ $set : { Type : 'super-fast', Price : '100' } }");
        }

        private IMongoCollection<Car> CreateCollection()
        {
            var collection = GetCollection<Car>("cars");
            return collection;
        }

        static UpdateDefinition<Car> GetUpdateFieldDefinition(params (Expression<Func<Car, object>> fieldSelector, object value)[] fieldsToUpdate)
        {
            UpdateDefinition<Car> updateDefinition = null;
            foreach (var (fieldSelector, value) in fieldsToUpdate)
            {
                updateDefinition = updateDefinition?.Set(fieldSelector, value) ?? Builders<Car>.Update.Set(fieldSelector, value);
            }
            return updateDefinition;
        }

        public class Car
        {
            public string Type { get; set; }
            public decimal Price { get; set; }
        }
    }
}
