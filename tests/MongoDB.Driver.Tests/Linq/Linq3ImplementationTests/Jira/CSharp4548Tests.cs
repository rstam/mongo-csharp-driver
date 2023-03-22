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
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.TestHelpers.XunitExtensions;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3ImplementationTests.Jira
{
    public class CSharp4548Tests : Linq3IntegrationTest
    {
        [Fact]
        public void Find_BsonDocument_with_projection_definition_and_client_side_projection_should_work()
        {
            var collection = CreateBsonDocumentCollection();

            var find = collection
                .Find(_ => true)
                .Project(Builders<BsonDocument>.Projection.Include("key").Include("prop").Exclude("_id"));

            var projection = TranslateFindProjection(collection, find);
            projection.Should().Be("{ key : 1, prop : 1, _id : 0  }");

            var results = find
                .ToEnumerable()
                .Select(x => MyClientSideProjection(x["key"], x["prop"]))
                .ToList();

            results.Should().Equal(new Result("k", "p"));
        }

        [Fact]
        public void Find_BsonDocument_with_bson_document_projection_and_client_side_projection_should_work()
        {
            var collection = CreateBsonDocumentCollection();

            var find = collection
                .Find(_ => true)
                .Project(new BsonDocument { { "key", "$key" }, { "prop", "$prop" }, { "_id", 0 } });

            var projection = TranslateFindProjection(collection, find);
            projection.Should().Be("{ key : '$key', prop : '$prop', _id : 0  }");

            var results = find
                .ToEnumerable()
                .Select(x => MyClientSideProjection(x["key"], x["prop"]))
                .ToList();

            results.Should().Equal(new Result("k", "p"));
        }

        [Fact]
        public void Find_BsonDocument_with_anonymous_class_projection_and_client_side_projection_should_work()
        {
            RequireServer.Check().Supports(Feature.FindProjectionExpressions);
            var collection = CreateBsonDocumentCollection();

            var find = collection
                .Find(_ => true)
                .Project(x => new { Key = x["key"], Prop = x["prop"] });

            var projection = TranslateFindProjection(collection, find);
            projection.Should().Be("{ Key : '$key', Prop : '$prop', _id : 0  }");

            var results = find
                .ToEnumerable()
                .Select(x => MyClientSideProjection(x.Key, x.Prop))
                .ToList();

            results.Should().Equal(new Result("k", "p"));
        }

        [Fact]
        public void Find_BsonDocument_with_dto_class_using_constructor_projection_and_client_side_projection_should_work()
        {
            RequireServer.Check().Supports(Feature.FindProjectionExpressions);
            var collection = CreateBsonDocumentCollection();

            var find = collection
                .Find(_ => true)
                .Project(x => new Dto1(x["key"], x["prop"]));

            var projection = TranslateFindProjection(collection, find);
            projection.Should().Be("{ Key : '$key', Prop : '$prop', _id : 0  }");

            var results = find
                .ToEnumerable()
                .Select(x => MyClientSideProjection(x.Key, x.Prop))
                .ToList();

            results.Should().Equal(new Result("k", "p"));
        }

        [Fact]
        public void Find_BsonDocument_with_dto_class_using_initializers_projection_and_client_side_projection_should_work()
        {
            RequireServer.Check().Supports(Feature.FindProjectionExpressions);
            var collection = CreateBsonDocumentCollection();

            var find = collection
                .Find(_ => true)
                .Project(x => new Dto1 { Key = x["key"], Prop = x["prop"] });

            var projection = TranslateFindProjection(collection, find);
            projection.Should().Be("{ Key : '$key', Prop : '$prop', _id : 0  }");

            var results = find
                .ToEnumerable()
                .Select(x => MyClientSideProjection(x.Key, x.Prop))
                .ToList();

            results.Should().Equal(new Result("k", "p"));
        }

        [Fact]
        public void Find_BsonDocument_with_tuple_using_constructor_projection_and_client_side_projection_should_work()
        {
            RequireServer.Check().Supports(Feature.FindProjectionExpressions);
            var collection = CreateBsonDocumentCollection();

            var find = collection
                .Find(_ => true)
                .Project(x => new Tuple<BsonValue, BsonValue>(x["key"], x["prop"]));

            var projection = TranslateFindProjection(collection, find);
            projection.Should().Be("{ _v : ['$key', '$prop'], _id : 0  }");

            var results = find
                .ToEnumerable()
                .Select(x => MyClientSideProjection(x.Item1, x.Item2))
                .ToList();

            results.Should().Equal(new Result("k", "p"));
        }

        [Fact]
        public void Find_BsonDocument_with_tuple_using_create_method_projection_and_client_side_projection_should_work()
        {
            RequireServer.Check().Supports(Feature.FindProjectionExpressions);
            var collection = CreateBsonDocumentCollection();

            var find = collection
                .Find(_ => true)
                .Project(x => Tuple.Create(x["key"], x["prop"]));

            var projection = TranslateFindProjection(collection, find);
            projection.Should().Be("{ _v : ['$key', '$prop'], _id : 0  }");

            var results = find
                .ToEnumerable()
                .Select(x => MyClientSideProjection(x.Item1, x.Item2))
                .ToList();

            results.Should().Equal(new Result("k", "p"));
        }

        [Fact]
        public void Find_BsonDocument_with_value_tuple_using_constructor_projection_and_client_side_projection_should_work()
        {
            RequireServer.Check().Supports(Feature.FindProjectionExpressions);
            var collection = CreateBsonDocumentCollection();

            var find = collection
                .Find(_ => true)
                .Project(x => new ValueTuple<BsonValue, BsonValue>(x["key"], x["prop"]));

            var projection = TranslateFindProjection(collection, find);
            projection.Should().Be("{ _v : ['$key', '$prop'], _id : 0  }");

            var results = find
                .ToEnumerable()
                .Select(x => MyClientSideProjection(x.Item1, x.Item2))
                .ToList();

            results.Should().Equal(new Result("k", "p"));
        }

        [Fact]
        public void Find_BsonDocument_with_value_tuple_using_create_method_projection_and_client_side_projection_should_work()
        {
            RequireServer.Check().Supports(Feature.FindProjectionExpressions);
            var collection = CreateBsonDocumentCollection();

            var find = collection
                .Find(_ => true)
                .Project(x => ValueTuple.Create(x["key"], x["prop"]));

            var projection = TranslateFindProjection(collection, find);
            projection.Should().Be("{ _v : ['$key', '$prop'], _id : 0  }");

            var results = find
                .ToEnumerable()
                .Select(x => MyClientSideProjection(x.Item1, x.Item2))
                .ToList();

            results.Should().Equal(new Result("k", "p"));
        }

        [Fact]
        public void Find_document_with_projection_definition_and_client_side_projection_should_work()
        {
            var collection = CreateDocumentCollection();

            var find = collection
                .Find(_ => true)
                .Project(Builders<Document>.Projection.Include(x => x.Key).Include(x => x.Prop).Exclude("_id"));

            var projection = TranslateFindProjection(collection, find);
            projection.Should().Be("{ Key : 1, Prop : 1, _id : 0  }");

            var results = find
                .ToEnumerable()
                .Select(x => MyClientSideProjection(x["Key"], x["Prop"]))
                .ToList();

            results.Should().Equal(new Result("k", "p"));
        }

        [Fact]
        public void Find_document_with_bson_document_projection_and_client_side_projection_should_work()
        {
            var collection = CreateDocumentCollection();

            var find = collection
                .Find(_ => true)
                .Project(new BsonDocument { { "Key", "$Key" }, { "Prop", "$Prop" }, { "_id", 0 } });

            var projection = TranslateFindProjection(collection, find);
            projection.Should().Be("{ Key : '$Key', Prop : '$Prop', _id : 0  }");

            var results = find
                .ToEnumerable()
                .Select(x => MyClientSideProjection(x["Key"], x["Prop"]))
                .ToList();

            results.Should().Equal(new Result("k", "p"));
        }

        [Fact]
        public void Find_document_with_anonymous_class_projection_and_client_side_projection_should_work()
        {
            RequireServer.Check().Supports(Feature.FindProjectionExpressions);
            var collection = CreateDocumentCollection();

            var find = collection
                .Find(_ => true)
                .Project(x => new { Key = x.Key, Prop = x.Prop });

            var projection = TranslateFindProjection(collection, find);
            projection.Should().Be("{ Key : '$Key', Prop : '$Prop', _id : 0  }");

            var results = find
                .ToEnumerable()
                .Select(x => MyClientSideProjection(x.Key, x.Prop))
                .ToList();

            results.Should().Equal(new Result("k", "p"));
        }

        [Fact]
        public void Find_document_with_dto_class_using_constructor_projection_and_client_side_projection_should_work()
        {
            RequireServer.Check().Supports(Feature.FindProjectionExpressions);
            var collection = CreateDocumentCollection();

            var find = collection
                .Find(_ => true)
                .Project(x => new Dto2(x.Key, x.Prop));

            var projection = TranslateFindProjection(collection, find);
            projection.Should().Be("{ Key : '$Key', Prop : '$Prop', _id : 0  }");

            var results = find
                .ToEnumerable()
                .Select(x => MyClientSideProjection(x.Key, x.Prop))
                .ToList();

            results.Should().Equal(new Result("k", "p"));
        }

        [Fact]
        public void Find_document_with_dto_class_using_initializers_projection_and_client_side_projection_should_work()
        {
            RequireServer.Check().Supports(Feature.FindProjectionExpressions);
            var collection = CreateDocumentCollection();

            var find = collection
                .Find(_ => true)
                .Project(x => new Dto2 { Key = x.Key, Prop = x.Prop });

            var projection = TranslateFindProjection(collection, find);
            projection.Should().Be("{ Key : '$Key', Prop : '$Prop', _id : 0  }");

            var results = find
                .ToEnumerable()
                .Select(x => MyClientSideProjection(x.Key, x.Prop))
                .ToList();

            results.Should().Equal(new Result("k", "p"));
        }

        [Fact]
        public void Find_document_with_tuple_using_constructor_projection_and_client_side_projection_should_work()
        {
            RequireServer.Check().Supports(Feature.FindProjectionExpressions);
            var collection = CreateDocumentCollection();

            var find = collection
                .Find(_ => true)
                .Project(x => new Tuple<string, string>(x.Key, x.Prop));

            var projection = TranslateFindProjection(collection, find);
            projection.Should().Be("{ _v : ['$Key', '$Prop'], _id : 0  }");

            var results = find
                .ToEnumerable()
                .Select(x => MyClientSideProjection(x.Item1, x.Item2))
                .ToList();

            results.Should().Equal(new Result("k", "p"));
        }

        [Fact]
        public void Find_document_with_tuple_using_create_method_projection_and_client_side_projection_should_work()
        {
            RequireServer.Check().Supports(Feature.FindProjectionExpressions);
            var collection = CreateDocumentCollection();

            var find = collection
                .Find(_ => true)
                .Project(x => Tuple.Create(x.Key, x.Prop));

            var projection = TranslateFindProjection(collection, find);
            projection.Should().Be("{ _v : ['$Key', '$Prop'], _id : 0  }");

            var results = find
                .ToEnumerable()
                .Select(x => MyClientSideProjection(x.Item1, x.Item2))
                .ToList();

            results.Should().Equal(new Result("k", "p"));
        }

        [Fact]
        public void Find_document_with_value_tuple_using_constructor_projection_and_client_side_projection_should_work()
        {
            RequireServer.Check().Supports(Feature.FindProjectionExpressions);
            var collection = CreateDocumentCollection();

            var find = collection
                .Find(_ => true)
                .Project(x => new ValueTuple<string, string>(x.Key, x.Prop));

            var projection = TranslateFindProjection(collection, find);
            projection.Should().Be("{ _v : ['$Key', '$Prop'], _id : 0  }");

            var results = find
                .ToEnumerable()
                .Select(x => MyClientSideProjection(x.Item1, x.Item2))
                .ToList();

            results.Should().Equal(new Result("k", "p"));
        }

        [Fact]
        public void Find_document_with_value_tuple_using_create_method_projection_and_client_side_projection_should_work()
        {
            RequireServer.Check().Supports(Feature.FindProjectionExpressions);
            var collection = CreateDocumentCollection();

            var find = collection
                .Find(_ => true)
                .Project(x => ValueTuple.Create(x.Key, x.Prop));

            var projection = TranslateFindProjection(collection, find);
            projection.Should().Be("{ _v : ['$Key', '$Prop'], _id : 0  }");

            var results = find
                .ToEnumerable()
                .Select(x => MyClientSideProjection(x.Item1, x.Item2))
                .ToList();

            results.Should().Equal(new Result("k", "p"));
        }

        private IMongoCollection<BsonDocument> CreateBsonDocumentCollection()
        {
            var collection = GetCollection<BsonDocument>("test");

            CreateCollection(
                collection,
                new BsonDocument { { "_id", 1 }, { "key", "k" }, { "prop", "p" } });

            return collection;
        }

        private IMongoCollection<Document> CreateDocumentCollection()
        {
            var collection = GetCollection<Document>("test");

            CreateCollection(
                collection,
                new Document { Id = 1, Key = "k", Prop = "p" });

            return collection;
        }

        private static Result MyClientSideProjection(BsonValue key, BsonValue prop)
        {
            return new Result(key.AsString, prop.AsString);
        }

        private static Result MyClientSideProjection(string key, string prop)
        {
            return new Result(key, prop);
        }

        public class Document
        {
            public int Id { get; set; }
            public string Key { get; set; }
            public string Prop { get; set; }
        }

        public class Dto1
        {
            public Dto1()
            {
            }

            public Dto1(BsonValue key, BsonValue prop)
            {
                Key = key;
                Prop = prop;
            }

            public BsonValue Key { get; set; }
            public BsonValue Prop { get; set; }
        }

        public class Dto2
        {
            public Dto2()
            {
            }

            public Dto2(string key, string prop)
            {
                Key = key;
                Prop = prop;
            }

            public string Key { get; set; }
            public string Prop { get; set; }
        }

        private class Result
        {
            public Result(string key, string prop)
            {
                Key = key;
                Prop = prop;
            }

            public string Key { get; set; }
            public string Prop { get; set; }

            public override bool Equals(object obj)
            {
                return
                    obj is Result other &&
                    Key == other.Key &&
                    Prop == other.Prop;
            }

            public override int GetHashCode() => 0;
        }
    }
}
