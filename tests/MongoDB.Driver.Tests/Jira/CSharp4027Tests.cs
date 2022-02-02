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
using System.Linq;
using System.Text;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Xunit;

namespace MongoDB.Driver.Tests.Jira
{
    public class CSharp4027Tests
    {
        [Fact]
        public void Insert_should_add_discriminator()
        {
            var collection = GetCollection();
            var c = new C { Id = 1, X = 2 };

            collection.InsertOne(c);
            var result = GetBsonDocumentResult(collection);

            result.Should().Be("{ _id : 1, _t : 'C', X : 2 }");
        }

        [Fact]
        public void Upsert_should_add_discriminator()
        {
            var collection = GetCollection();
            var c = new C { Id = 1, X = 2 };

            var filter = Builders<C>.Filter.Eq(x => x.Id, 1);
            var update = Builders<C>.Update
                .SetOnInsert("_t", "C")
                .SetOnInsert(x => x.X, 2);
            var options = new FindOneAndUpdateOptions<C> { IsUpsert = true };

            collection.FindOneAndUpdate(filter, update, options);
            var result = GetBsonDocumentResult(collection);

            result.Should().Be("{ _id : 1, X : 2, _t : 'C' }"); // server created the fields in this order
        }

        private IMongoCollection<C> GetCollection()
        {
            var client = DriverTestConfiguration.Client;
            var database = client.GetDatabase(DriverTestConfiguration.DatabaseNamespace.DatabaseName);
            var collection = database.GetCollection<C>(DriverTestConfiguration.CollectionNamespace.CollectionName);
            database.DropCollection(collection.CollectionNamespace.CollectionName);
            return collection;
        }

        private BsonDocument GetBsonDocumentResult(IMongoCollection<C> collection)
        {
            var database = collection.Database;
            var bsonDocumentCollection = database.GetCollection<BsonDocument>(collection.CollectionNamespace.CollectionName);
            return bsonDocumentCollection.Find("{}").ToList().Single();
        }
    }

    [BsonDiscriminator(Required = true, RootClass = true)]
    public class C
    {
        public int Id { get; set; }
        public int X { get; set; }
    }
}
