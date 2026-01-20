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
using MongoDB.Driver.TestHelpers;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Linq.Linq3Implementation.Serializers;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3Implementation.Jira;

public class CSharpxxxxTests : LinqIntegrationTest<CSharpxxxxTests.ClassFixture>
{
    static CSharpxxxxTests()
    {
        BsonClassMap.RegisterClassMap<C>(cm =>
        {
            var intAsStringSerializer = new Int32Serializer(representation: BsonType.String);
            var dictionarySerializer = DictionarySerializer.Create(DictionaryRepresentation.Document, intAsStringSerializer, intAsStringSerializer);

            cm.AutoMap();
            cm.MapMember(x => x.D).SetSerializer(dictionarySerializer);
        });
    }

    public CSharpxxxxTests(ClassFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public void C_sould_serialize_as_expected()
    {
        var c = new C()
        {
            Id = 1,
            One = 1,
            A = new int[] { 1, 2, 3 },
            B = new BsonDocument { { "One", 1 }, { "Two", 2 }, { "Three", 3 } },
            D = new Dictionary<int, int>() { { 1, 1 }, { 2, 2 }, { 3, 3 } },
            L = new List<int> { 1, 2, 3 }
        };

        var json =  c.ToJson();
        json.Should().Be("""{ "_id" : 1, "One" : "1", "A" : ["1", "2", "3"], "B" : { "One" : 1, "Two" : 2, "Three" : 3 }, "D" : { "1" : "1", "2" : "2", "3" : "3" }, "L" : ["1", "2", "3"] }""");
    }

    [Fact]
    public void Array_collection_initializer_should_work()
    {
        var collection = Fixture.Collection;

        var queryable = collection.AsQueryable()
            .Select(x => new C { A = new[] { x.One, 2, 3 } });

        var stages = Translate(collection, queryable);
        AssertStages(stages, "{ $project : { A : ['$One', '2', '3'], _id : 0 } }");
    }

    [Fact]
    public void BsonDocument_collection_initializer_should_work()
    {
        var collection = Fixture.Collection;

        var queryable = collection.AsQueryable()
            .Select(x => new C { B = { { "One", x.One }, { "Two", 2 }, { "Three", 3} } });

        var stages = Translate(collection, queryable);
        AssertStages(stages, "{ $project : { B : { One : '$One', Two : 2, Three : 3 } } }");
    }

    [Fact]
    public void Dictionary_collection_initializer_should_work()
    {
        var collection = Fixture.Collection;

        var queryable = collection.AsQueryable()
            .Select(x => new C { D = { { 1, x.One }, { 2, 2 }, { 3, 3} } });

        var stages = Translate(collection, queryable);
        AssertStages(stages, "{ $project : { D : { '1' : '$One', '2' : '2', '3' : '3' } } }");
    }

    [Fact]
    public void List_collection_initializer_should_work()
    {
        var collection = Fixture.Collection;

        var queryable = collection.AsQueryable()
            .Select(x => new C { L = { x.One, 2, 3 } });

        var stages = Translate(collection, queryable);
        AssertStages(stages, "{ $project : { A : ['$One', '2', '3'], _id : 0 } }");
    }

    public class C
    {
        public int Id { get; set; }
        [BsonRepresentation(BsonType.String)] public int One { get; set; }
        [BsonRepresentation(BsonType.String)] public int[] A { get; set; }
        public BsonDocument B { get; set; }
        public Dictionary<int, int> D { get; set; }
        [BsonRepresentation(BsonType.String)] public List<int> L { get; set; }
    }

    public sealed class ClassFixture : MongoCollectionFixture<C>
    {
        protected override IEnumerable<C> InitialData =>
        [
            new C { }
        ];
    }
}
