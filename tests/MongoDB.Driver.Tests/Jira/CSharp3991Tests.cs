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
using MongoDB.Bson.TestHelpers.XunitExtensions;
using Xunit;

namespace MongoDB.Driver.Tests.Jira
{
    public class CSharp3991Tests
    {
        // test all combinations of:
        // filter: FilterDefinition, Expression, lambda expression, BsonDocument, string
        // options: omitted, null, without projection, with projection
        // session: omitted, present
        // async: no, yes

        [Theory]
        [ParameterAttributeData]
        public void Find_with_filter_definition_and_no_options_should_work(
            [Values(false, true)] bool withSession,
            [Values(false, true)] bool async)
        {
            var (client, collection) = Setup();
            var filter = Builders<C>.Filter.Eq("Id", 1);
            var session = withSession ? client.StartSession() : null;

            using var cursor = (withSession, async) switch
            {
                (false, false) => collection.FindSync(filter),
                (false, true) => collection.FindAsync(filter).GetAwaiter().GetResult(),
                (true, false) => collection.FindSync(session, filter),
                (true, true) => collection.FindAsync(session, filter).GetAwaiter().GetResult()
            };

            var results = cursor.ToList();
            results.Count.Should().Be(1);
            results[0].Id.Should().Be(1);
            results[0].X.Should().Be(2);
        }

        [Theory]
        [ParameterAttributeData]
        public void Find_with_filter_definition_and_options_null_should_work(
            [Values(false, true)] bool withSession,
            [Values(false, true)] bool async)
        {
            var (client, collection) = Setup();
            var filter = Builders<C>.Filter.Eq("Id", 1);
            var session = withSession ? client.StartSession() : null;

            using var cursor = (withSession, async) switch
            {
                (false, false) => collection.FindSync(filter, null),
                (false, true) => collection.FindAsync(filter, null).GetAwaiter().GetResult(),
                (true, false) => collection.FindSync(session, filter, null),
                (true, true) => collection.FindAsync(session, filter, null).GetAwaiter().GetResult()
            };

            var results = cursor.ToList();
            results.Count.Should().Be(1);
            results[0].Id.Should().Be(1);
            results[0].X.Should().Be(2);
        }

        [Theory]
        [ParameterAttributeData]
        public void Find_with_filter_definition_and_options_without_projection_should_work(
            [Values(false, true)] bool withSession,
            [Values(false, true)] bool async)
        {
            var (client, collection) = Setup();
            var filter = Builders<C>.Filter.Eq("Id", 1);
            var options = new FindOptions<C>();
            var session = withSession ? client.StartSession() : null;

            using var cursor = (withSession, async) switch
            {
                (false, false) => collection.FindSync(filter, options),
                (false, true) => collection.FindAsync(filter, options).GetAwaiter().GetResult(),
                (true, false) => collection.FindSync(session, filter, options),
                (true, true) => collection.FindAsync(session, filter, options).GetAwaiter().GetResult()
            };

            var results = cursor.ToList();
            results.Count.Should().Be(1);
            results[0].Id.Should().Be(1);
            results[0].X.Should().Be(2);
        }

        [Theory]
        [ParameterAttributeData]
        public void Find_with_filter_definition_and_options_with_projection_should_work(
            [Values(false, true)] bool withSession,
            [Values(false, true)] bool async)
        {
            var (client, collection) = Setup();
            var filter = Builders<C>.Filter.Eq("Id", 1);
            var options = new FindOptions<C, P>()
            {
                Projection = Builders<C>.Projection.Include("X").Exclude("Id")
            };
            var session = withSession ? client.StartSession() : null;

            using var cursor = (withSession, async) switch
            {
                (false, false) => collection.FindSync(filter, options),
                (false, true) => collection.FindAsync(filter, options).GetAwaiter().GetResult(),
                (true, false) => collection.FindSync(session, filter, options),
                (true, true) => collection.FindAsync(session, filter, options).GetAwaiter().GetResult()
            };

            var results = cursor.ToList();
            results.Count.Should().Be(1);
            results[0].X.Should().Be(2);
        }

        [Theory]
        [ParameterAttributeData]
        public void Find_with_filter_expression_and_no_options_should_work(
            [Values(false, true)] bool withSession,
            [Values(false, true)] bool async)
        {
            var (client, collection) = Setup();
            var filter = (Expression<Func<C, bool>>)(c => c.Id == 1);

            var session = withSession ? client.StartSession() : null;

            using var cursor = (withSession, async) switch
            {
                (false, false) => collection.FindSync(filter),
                (false, true) => collection.FindAsync(filter).GetAwaiter().GetResult(),
                (true, false) => collection.FindSync(session, filter),
                (true, true) => collection.FindAsync(session, filter).GetAwaiter().GetResult()
            };

            var results = cursor.ToList();
            results.Count.Should().Be(1);
            results[0].Id.Should().Be(1);
            results[0].X.Should().Be(2);
        }

        [Theory]
        [ParameterAttributeData]
        public void Find_with_filter_expression_and_options_null_should_work(
            [Values(false, true)] bool withSession,
            [Values(false, true)] bool async)
        {
            var (client, collection) = Setup();
            var filter = (Expression<Func<C, bool>>)(c => c.Id == 1);
            var session = withSession ? client.StartSession() : null;

            using var cursor = (withSession, async) switch
            {
                (false, false) => collection.FindSync(filter, null),
                (false, true) => collection.FindAsync(filter, null).GetAwaiter().GetResult(),
                (true, false) => collection.FindSync(session, filter, null),
                (true, true) => collection.FindAsync(session, filter, null).GetAwaiter().GetResult()
            };

            var results = cursor.ToList();
            results.Count.Should().Be(1);
            results[0].Id.Should().Be(1);
            results[0].X.Should().Be(2);
        }

        [Theory]
        [ParameterAttributeData]
        public void Find_with_filter_expression_and_options_without_projection_should_work(
            [Values(false, true)] bool withSession,
            [Values(false, true)] bool async)
        {
            var (client, collection) = Setup();
            var filter = (Expression<Func<C, bool>>)(c => c.Id == 1);
            var options = new FindOptions<C>();
            var session = withSession ? client.StartSession() : null;

            using var cursor = (withSession, async) switch
            {
                (false, false) => collection.FindSync(filter, options),
                (false, true) => collection.FindAsync(filter, options).GetAwaiter().GetResult(),
                (true, false) => collection.FindSync(session, filter, options),
                (true, true) => collection.FindAsync(session, filter, options).GetAwaiter().GetResult()
            };

            var results = cursor.ToList();
            results.Count.Should().Be(1);
            results[0].Id.Should().Be(1);
            results[0].X.Should().Be(2);
        }

        [Theory]
        [ParameterAttributeData]
        public void Find_with_filter_expression_and_options_with_projection_should_work(
            [Values(false, true)] bool withSession,
            [Values(false, true)] bool async)
        {
            var (client, collection) = Setup();
            var filter = (Expression<Func<C, bool>>)(c => c.Id == 1);
            var options = new FindOptions<C, P>()
            {
                Projection = Builders<C>.Projection.Include("X").Exclude("Id")
            };
            var session = withSession ? client.StartSession() : null;

            using var cursor = (withSession, async) switch
            {
                (false, false) => collection.FindSync(filter, options),
                (false, true) => collection.FindAsync(filter, options).GetAwaiter().GetResult(),
                (true, false) => collection.FindSync(session, filter, options),
                (true, true) => collection.FindAsync(session, filter, options).GetAwaiter().GetResult()
            };

            var results = cursor.ToList();
            results.Count.Should().Be(1);
            results[0].X.Should().Be(2);
        }

        [Theory]
        [ParameterAttributeData]
        public void Find_with_filter_lambda_expression_and_no_options_should_work(
            [Values(false, true)] bool withSession,
            [Values(false, true)] bool async)
        {
            var (client, collection) = Setup();

            var session = withSession ? client.StartSession() : null;

            using var cursor = (withSession, async) switch
            {
                (false, false) => collection.FindSync(c => c.Id == 1),
                (false, true) => collection.FindAsync(c => c.Id == 1).GetAwaiter().GetResult(),
                (true, false) => collection.FindSync(session, c => c.Id == 1),
                (true, true) => collection.FindAsync(session, c => c.Id == 1).GetAwaiter().GetResult()
            };

            var results = cursor.ToList();
            results.Count.Should().Be(1);
            results[0].Id.Should().Be(1);
            results[0].X.Should().Be(2);
        }

        [Theory]
        [ParameterAttributeData]
        public void Find_with_filter_lambda_expression_and_options_null_should_work(
            [Values(false, true)] bool withSession,
            [Values(false, true)] bool async)
        {
            var (client, collection) = Setup();
            var session = withSession ? client.StartSession() : null;

            using var cursor = (withSession, async) switch
            {
                (false, false) => collection.FindSync(c => c.Id == 1, null),
                (false, true) => collection.FindAsync(c => c.Id == 1, null).GetAwaiter().GetResult(),
                (true, false) => collection.FindSync(session, c => c.Id == 1, null),
                (true, true) => collection.FindAsync(session, c => c.Id == 1, null).GetAwaiter().GetResult()
            };

            var results = cursor.ToList();
            results.Count.Should().Be(1);
            results[0].Id.Should().Be(1);
            results[0].X.Should().Be(2);
        }

        [Theory]
        [ParameterAttributeData]
        public void Find_with_filter_lambda_expression_and_options_without_projection_should_work(
            [Values(false, true)] bool withSession,
            [Values(false, true)] bool async)
        {
            var (client, collection) = Setup();
            var options = new FindOptions<C>();
            var session = withSession ? client.StartSession() : null;

            using var cursor = (withSession, async) switch
            {
                (false, false) => collection.FindSync(c => c.Id == 1, options),
                (false, true) => collection.FindAsync(c => c.Id == 1, options).GetAwaiter().GetResult(),
                (true, false) => collection.FindSync(session, c => c.Id == 1, options),
                (true, true) => collection.FindAsync(session, c => c.Id == 1, options).GetAwaiter().GetResult()
            };

            var results = cursor.ToList();
            results.Count.Should().Be(1);
            results[0].Id.Should().Be(1);
            results[0].X.Should().Be(2);
        }

        [Theory]
        [ParameterAttributeData]
        public void Find_with_filter_lambda_expression_and_options_with_projection_should_work(
            [Values(false, true)] bool withSession,
            [Values(false, true)] bool async)
        {
            var (client, collection) = Setup();
            var options = new FindOptions<C, P>()
            {
                Projection = Builders<C>.Projection.Include("X").Exclude("Id")
            };
            var session = withSession ? client.StartSession() : null;

            using var cursor = (withSession, async) switch
            {
                (false, false) => collection.FindSync(c => c.Id == 1, options),
                (false, true) => collection.FindAsync(c => c.Id == 1, options).GetAwaiter().GetResult(),
                (true, false) => collection.FindSync(session, c => c.Id == 1, options),
                (true, true) => collection.FindAsync(session, c => c.Id == 1, options).GetAwaiter().GetResult()
            };

            var results = cursor.ToList();
            results.Count.Should().Be(1);
            results[0].X.Should().Be(2);
        }

        [Theory]
        [ParameterAttributeData]
        public void Find_with_filter_document_and_no_options_should_work(
            [Values(false, true)] bool withSession,
            [Values(false, true)] bool async)
        {
            var (client, collection) = Setup();
            var filter = new BsonDocument("_id", 1);
            var session = withSession ? client.StartSession() : null;

            using var cursor = (withSession, async) switch
            {
                (false, false) => collection.FindSync(filter),
                (false, true) => collection.FindAsync(filter).GetAwaiter().GetResult(),
                (true, false) => collection.FindSync(session, filter),
                (true, true) => collection.FindAsync(session, filter).GetAwaiter().GetResult()
            };

            var results = cursor.ToList();
            results.Count.Should().Be(1);
            results[0].Id.Should().Be(1);
            results[0].X.Should().Be(2);
        }

        [Theory]
        [ParameterAttributeData]
        public void Find_with_filter_document_and_options_null_should_work(
            [Values(false, true)] bool withSession,
            [Values(false, true)] bool async)
        {
            var (client, collection) = Setup();
            var filter = new BsonDocument("_id", 1);
            var session = withSession ? client.StartSession() : null;

            using var cursor = (withSession, async) switch
            {
                (false, false) => collection.FindSync(filter, null),
                (false, true) => collection.FindAsync(filter, null).GetAwaiter().GetResult(),
                (true, false) => collection.FindSync(session, filter, null),
                (true, true) => collection.FindAsync(session, filter, null).GetAwaiter().GetResult()
            };

            var results = cursor.ToList();
            results.Count.Should().Be(1);
            results[0].Id.Should().Be(1);
            results[0].X.Should().Be(2);
        }

        [Theory]
        [ParameterAttributeData]
        public void Find_with_filter_document_and_options_without_projection_should_work(
            [Values(false, true)] bool withSession,
            [Values(false, true)] bool async)
        {
            var (client, collection) = Setup();
            var filter = new BsonDocument("_id", 1);
            var options = new FindOptions<C>();
            var session = withSession ? client.StartSession() : null;

            using var cursor = (withSession, async) switch
            {
                (false, false) => collection.FindSync(filter, options),
                (false, true) => collection.FindAsync(filter, options).GetAwaiter().GetResult(),
                (true, false) => collection.FindSync(session, filter, options),
                (true, true) => collection.FindAsync(session, filter, options).GetAwaiter().GetResult()
            };

            var results = cursor.ToList();
            results.Count.Should().Be(1);
            results[0].Id.Should().Be(1);
            results[0].X.Should().Be(2);
        }

        [Theory]
        [ParameterAttributeData]
        public void Find_with_filter_document_and_options_with_projection_should_work(
            [Values(false, true)] bool withSession,
            [Values(false, true)] bool async)
        {
            var (client, collection) = Setup();
            var filter = new BsonDocument("_id", 1);
            var options = new FindOptions<C, P>()
            {
                Projection = Builders<C>.Projection.Include("X").Exclude("Id")
            };
            var session = withSession ? client.StartSession() : null;

            using var cursor = (withSession, async) switch
            {
                (false, false) => collection.FindSync(filter, options),
                (false, true) => collection.FindAsync(filter, options).GetAwaiter().GetResult(),
                (true, false) => collection.FindSync(session, filter, options),
                (true, true) => collection.FindAsync(session, filter, options).GetAwaiter().GetResult()
            };

            var results = cursor.ToList();
            results.Count.Should().Be(1);
            results[0].X.Should().Be(2);
        }

        [Theory]
        [ParameterAttributeData]
        public void Find_with_filter_json_and_no_options_should_work(
            [Values(false, true)] bool withSession,
            [Values(false, true)] bool async)
        {
            var (client, collection) = Setup();
            var filter = "{ _id : 1 }";
            var session = withSession ? client.StartSession() : null;

            using var cursor = (withSession, async) switch
            {
                (false, false) => collection.FindSync(filter),
                (false, true) => collection.FindAsync(filter).GetAwaiter().GetResult(),
                (true, false) => collection.FindSync(session, filter),
                (true, true) => collection.FindAsync(session, filter).GetAwaiter().GetResult()
            };

            var results = cursor.ToList();
            results.Count.Should().Be(1);
            results[0].Id.Should().Be(1);
            results[0].X.Should().Be(2);
        }

        [Theory]
        [ParameterAttributeData]
        public void Find_with_filter_json_and_options_null_should_work(
            [Values(false, true)] bool withSession,
            [Values(false, true)] bool async)
        {
            var (client, collection) = Setup();
            var filter = "{ _id : 1 }";
            var session = withSession ? client.StartSession() : null;

            using var cursor = (withSession, async) switch
            {
                (false, false) => collection.FindSync(filter, null),
                (false, true) => collection.FindAsync(filter, null).GetAwaiter().GetResult(),
                (true, false) => collection.FindSync(session, filter, null),
                (true, true) => collection.FindAsync(session, filter, null).GetAwaiter().GetResult()
            };

            var results = cursor.ToList();
            results.Count.Should().Be(1);
            results[0].Id.Should().Be(1);
            results[0].X.Should().Be(2);
        }

        [Theory]
        [ParameterAttributeData]
        public void Find_with_filter_json_and_options_without_projection_should_work(
            [Values(false, true)] bool withSession,
            [Values(false, true)] bool async)
        {
            var (client, collection) = Setup();
            var filter = "{ _id : 1 }";
            var options = new FindOptions<C>();
            var session = withSession ? client.StartSession() : null;

            using var cursor = (withSession, async) switch
            {
                (false, false) => collection.FindSync(filter, options),
                (false, true) => collection.FindAsync(filter, options).GetAwaiter().GetResult(),
                (true, false) => collection.FindSync(session, filter, options),
                (true, true) => collection.FindAsync(session, filter, options).GetAwaiter().GetResult()
            };

            var results = cursor.ToList();
            results.Count.Should().Be(1);
            results[0].Id.Should().Be(1);
            results[0].X.Should().Be(2);
        }

        [Theory]
        [ParameterAttributeData]
        public void Find_with_filter_json_and_options_with_projection_should_work(
            [Values(false, true)] bool withSession,
            [Values(false, true)] bool async)
        {
            var (client, collection) = Setup();
            var filter = "{ _id : 1 }";
            var options = new FindOptions<C, P>()
            {
                Projection = Builders<C>.Projection.Include("X").Exclude("Id")
            };
            var session = withSession ? client.StartSession() : null;

            using var cursor = (withSession, async) switch
            {
                (false, false) => collection.FindSync(filter, options),
                (false, true) => collection.FindAsync(filter, options).GetAwaiter().GetResult(),
                (true, false) => collection.FindSync(session, filter, options),
                (true, true) => collection.FindAsync(session, filter, options).GetAwaiter().GetResult()
            };

            var results = cursor.ToList();
            results.Count.Should().Be(1);
            results[0].X.Should().Be(2);
        }

        [Fact]
        public void Example_from_jira_ticket_should_work()
        {
            var (client, collection) = Setup();

            using var cursor =
                collection.FindSync(
                    x => x.Id == 1,
                    new FindOptions<C, P>()
                    {
                        Projection = Builders<C>.Projection.Include(x => x.X).Exclude(x => x.Id)
                    });

            var results = cursor.ToList();
            results.Count.Should().Be(1);
            results[0].X.Should().Be(2);
        }

        public (IMongoClient, IMongoCollection<C>) Setup()
        {
            var client = DriverTestConfiguration.Client;
            var collectionNamespace = DriverTestConfiguration.CollectionNamespace;
            var database = client.GetDatabase(collectionNamespace.DatabaseNamespace.DatabaseName);
            var collection = database.GetCollection<C>(collectionNamespace.CollectionName);

            database.DropCollection(collectionNamespace.CollectionName);
            collection.InsertOne(new C { Id = 1, X = 2 });

            return (client, collection);
        }

        public class C
        {
            public int Id { get; set; }
            public int X { get; set; }
        }

        public class P
        {
            public int X { get; set; }
        }
    }
}
