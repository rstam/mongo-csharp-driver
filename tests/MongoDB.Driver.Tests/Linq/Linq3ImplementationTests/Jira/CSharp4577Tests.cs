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
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq;
using MongoDB.TestHelpers.XunitExtensions;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3ImplementationTests.Jira
{
    public class CSharp4577Tests : Linq3IntegrationTest
    {
        [Theory]
        [ParameterAttributeData]
        public void Update_set_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = CreateCollection(linqProvider);
            var id = 1;
            var key = "key";
            var updateDefinition = Builders<Model>.Update.Set(x => x.Keys[key], "new value");

            var renderedUpdate = RenderUpdate(collection, updateDefinition, linqProvider);
            renderedUpdate.Should().Be("{ $set : { 'Keys.key' : 'new value' } }");

            collection.UpdateOne(
                Builders<Model>.Filter.Eq(o => o.Id, id),
                updateDefinition);

            var documents = collection.FindSync("{}").ToList();
            documents.Should().HaveCount(1);
            documents[0].Keys["key"].Should().Be("new value");
        }

        [Theory]
        [ParameterAttributeData]
        public void Update_unset_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = CreateCollection(linqProvider);
            var id = 1;
            var key = "key";
            var updateDefinition = Builders<Model>.Update.Unset(x => x.Keys[key]);

            var renderedUpdate = RenderUpdate(collection, updateDefinition, linqProvider);
            renderedUpdate.Should().Be("{ $unset : { 'Keys.key' : 1 } }");

            collection.UpdateOne(
                Builders<Model>.Filter.Eq(o => o.Id, id),
                updateDefinition);

            var documents = collection.FindSync("{}").ToList();
            documents.Should().HaveCount(1);
            documents[0].Keys.Should().NotContainKey("key");
        }

        private IMongoCollection<Model> CreateCollection(LinqProvider linqProvider)
        {
            var collection = GetCollection<Model>("test", linqProvider);
            CreateCollection(
                collection,
                new Model(1, new Dictionary<string, string> { ["key"] = "value" }));
            return collection;
        }

        private BsonDocument RenderUpdate(IMongoCollection<Model> collection, UpdateDefinition<Model> updateDefinition, LinqProvider linqProvider)
        {
            var documentSerializer = collection.DocumentSerializer;
            var serializerRegistry = BsonSerializer.SerializerRegistry;
            return (BsonDocument)updateDefinition.Render(documentSerializer, serializerRegistry, linqProvider);
        }

        private class Model
        {
            public Model(int id, Dictionary<string, string> keys)
            {
                Id = id;
                Keys = keys;
            }

            public int Id { get; }
            public Dictionary<string, string> Keys { get; }
        }
    }
}
