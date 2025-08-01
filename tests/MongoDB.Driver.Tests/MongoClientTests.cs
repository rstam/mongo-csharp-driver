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
using System.Threading;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Operations;
using MongoDB.TestHelpers.XunitExtensions;
using Moq;
using Xunit;

namespace MongoDB.Driver.Tests
{
    [Trait("Category", "Integration")]
    public class MongoClientTests
    {
        [Fact]
        public void constructor_with_settings_should_throw_when_settings_is_null()
        {
            var exception = Record.Exception(() => new MongoClient((MongoClientSettings)null));

            var argumentNullException = exception.Should().BeOfType<ArgumentNullException>().Subject;
            argumentNullException.ParamName.Should().Be("settings");
        }

        [Fact]
        public void UsesSameClusterForIdenticalSettings()
        {
            var client1 = new MongoClient("mongodb://localhost");
            var cluster1 = client1.Cluster;

            var client2 = new MongoClient("mongodb://localhost");
            var cluster2 = client2.Cluster;

            Assert.Same(cluster1, cluster2);
        }

        [Fact]
        public void UsesSameClusterWhenReadPreferenceTagsAreTheSame()
        {
            var client1 = new MongoClient("mongodb://localhost/?readPreference=secondary;readPreferenceTags=dc:ny");
            var cluster1 = client1.Cluster;

            var client2 = new MongoClient("mongodb://localhost/?readPreference=secondary;readPreferenceTags=dc:ny");
            var cluster2 = client2.Cluster;

            Assert.Same(cluster1, cluster2);
        }

        [Fact]
        public void DefaultMongoClient_should_not_dispose_cluster()
        {
            var client = new MongoClient(new MongoClientSettings());
            var clusterId = client.Cluster.ClusterId;
            client.Dispose();

            ClusterRegistry.Instance._registry().Values.Should().Contain(c => c.ClusterId == clusterId);
        }

        [Fact]
        public void Dispose_should_return_cluster()
        {
            var cluster = new Mock<IClusterInternal>();
            var clusterSource = new Mock<IClusterSource>();
            clusterSource.Setup(c => c.Get(It.IsAny<ClusterKey>())).Returns(cluster.Object);

            var settings = new MongoClientSettings()
            {
                ClusterSource = clusterSource.Object
            };

            var client = new MongoClient(settings);
            client.Dispose();

            clusterSource.Verify(c => c.Return(cluster.Object));
        }

        [Fact]
        public void Dispose_twice_should_return_cluster_only_once()
        {
            var cluster = new Mock<IClusterInternal>();
            var clusterSource = new Mock<IClusterSource>();
            clusterSource.Setup(c => c.Get(It.IsAny<ClusterKey>())).Returns(cluster.Object);

            var settings = new MongoClientSettings()
            {
                ClusterSource = clusterSource.Object
            };

            var client = new MongoClient(settings);
            client.Dispose();
            client.Dispose();

            clusterSource.Verify(c => c.Return(cluster.Object), Times.Once);
        }

        [Fact]
        public void Disposed_client_should_throw_on_member_access()
        {
            var client = new MongoClient(new MongoClientSettings());
            client.Dispose();

            var exception = Record.Exception(() => client.Cluster);
            exception.Should().BeOfType<ObjectDisposedException>();
        }

        [Theory]
        [ParameterAttributeData]
        public void DropDatabase_should_invoke_the_correct_operation(
            [Values(false, true)] bool usingSession,
            [Values(false, true)] bool async)
        {
            var operationExecutor = new MockOperationExecutor();
            var writeConcern = new WriteConcern(1);
            var subject = new MongoClient(DriverTestConfiguration.GetClientSettings(), _ => operationExecutor).WithWriteConcern(writeConcern);
            var session = CreateClientSession();
            using var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            if (usingSession)
            {
                if (async)
                {
                    subject.DropDatabaseAsync(session, "awesome", cancellationToken).GetAwaiter().GetResult();
                }
                else
                {
                    subject.DropDatabase(session, "awesome", cancellationToken);
                }
            }
            else
            {
                if (async)
                {
                    subject.DropDatabaseAsync("awesome", cancellationToken).GetAwaiter().GetResult();
                }
                else
                {
                    subject.DropDatabase("awesome", cancellationToken);
                }
            }

            var call = operationExecutor.GetWriteCall<BsonDocument>();
            if (usingSession)
            {
                call.SessionId.Should().BeSameAs(session.ServerSession.Id);
            }
            else
            {
                call.UsedImplicitSession.Should().BeTrue();
            }
            call.CancellationToken.Should().Be(cancellationToken);

            var dropDatabaseOperation = call.Operation.Should().BeOfType<DropDatabaseOperation>().Subject;
            dropDatabaseOperation.DatabaseNamespace.Should().Be(new DatabaseNamespace("awesome"));
            dropDatabaseOperation.WriteConcern.Should().BeSameAs(writeConcern);
        }

        [Theory]
        [ParameterAttributeData]
        public void ListDatabaseNames_should_invoke_the_correct_operation(
            [Values(false, true)] bool usingSession,
            [Values(false, true)] bool async)
        {
            var operationExecutor = new MockOperationExecutor();
            var subject = new MongoClient(DriverTestConfiguration.GetClientSettings(), _ => operationExecutor);
            var session = CreateClientSession();
            using var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            var listDatabaseNamesResult = @"
            {
            	""databases"" : [
            		{
            			""name"" : ""admin"",
            			""sizeOnDisk"" : 131072,
            			""empty"" : false
            		},
            		{
            			""name"" : ""blog"",
            			""sizeOnDisk"" : 11669504,
            			""empty"" : false
            		},
            		{
            			""name"" : ""test-chambers"",
            			""sizeOnDisk"" : 222883840,
            			""empty"" : false
            		},
            		{
            			""name"" : ""recipes"",
            			""sizeOnDisk"" : 73728,
            			""empty"" : false
            		},
            		{
            			""name"" : ""employees"",
            			""sizeOnDisk"" : 225280,
            			""empty"" : false
            		}
            	],
            	""totalSize"" : 252534784,
            	""ok"" : 1
            }";
            var operationResult = BsonDocument.Parse(listDatabaseNamesResult);
            operationExecutor.EnqueueResult(CreateListDatabasesOperationCursor(operationResult));

            IList<string> databaseNames;
            if (async)
            {
                if (usingSession)
                {
                    databaseNames = subject.ListDatabaseNamesAsync(session, cancellationToken).GetAwaiter().GetResult().ToList();
                }
                else
                {
                    databaseNames = subject.ListDatabaseNamesAsync(cancellationToken).GetAwaiter().GetResult().ToList();
                }
            }
            else
            {
                if (usingSession)
                {
                    databaseNames = subject.ListDatabaseNames(session, cancellationToken).ToList();
                }
                else
                {
                    databaseNames = subject.ListDatabaseNames(cancellationToken).ToList();
                }
            }

            var call = operationExecutor.GetReadCall<IAsyncCursor<BsonDocument>>();

            if (usingSession)
            {
                call.SessionId.Should().BeSameAs(session.ServerSession.Id);
            }
            else
            {
                call.UsedImplicitSession.Should().BeTrue();
            }

            call.CancellationToken.Should().Be(cancellationToken);

            var operation = call.Operation.Should().BeOfType<ListDatabasesOperation>().Subject;
            operation.NameOnly.Should().Be(true);
            databaseNames.Should().Equal(operationResult["databases"].AsBsonArray.Select(record => record["name"].AsString));
        }

        private IAsyncCursor<BsonDocument> CreateListDatabasesOperationCursor(BsonDocument reply)
        {
            var databases = reply["databases"].AsBsonArray.OfType<BsonDocument>();
            return new SingleBatchAsyncCursor<BsonDocument>(databases.ToList());
        }

        [Theory]
        [ParameterAttributeData]
        public void ListDatabases_should_invoke_the_correct_operation(
            [Values(false, true)] bool usingSession,
            [Values(false, true)] bool async)
        {
            var operationExecutor = new MockOperationExecutor();
            var subject = new MongoClient(DriverTestConfiguration.GetClientSettings(), _ => operationExecutor);
            var session = CreateClientSession();
            using var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            var filterDocument = BsonDocument.Parse("{ name : \"awesome\" }");
            var filterDefinition = (FilterDefinition<BsonDocument>)filterDocument;
            var nameOnly = true;
            var options = new ListDatabasesOptions
            {
                Filter = filterDefinition,
                NameOnly = nameOnly
            };

            if (usingSession)
            {
                if (async)
                {
                    subject.ListDatabasesAsync(session, options, cancellationToken).GetAwaiter().GetResult();
                }
                else
                {
                    subject.ListDatabases(session, options, cancellationToken);
                }
            }
            else
            {
                if (async)
                {
                    subject.ListDatabasesAsync(options, cancellationToken).GetAwaiter().GetResult();
                }
                else
                {
                    subject.ListDatabases(options, cancellationToken);
                }
            }

            var call = operationExecutor.GetReadCall<IAsyncCursor<BsonDocument>>();
            if (usingSession)
            {
                call.SessionId.Should().BeSameAs(session.ServerSession.Id);
            }
            else
            {
                call.UsedImplicitSession.Should().BeTrue();
            }
            call.CancellationToken.Should().Be(cancellationToken);

            var operation = call.Operation.Should().BeOfType<ListDatabasesOperation>().Subject;
            operation.Filter.Should().Be(filterDocument);
            operation.NameOnly.Should().Be(nameOnly);
        }

        [Theory]
        [ParameterAttributeData]
        public void Watch_should_invoke_the_correct_operation(
            [Values(false, true)] bool usingSession,
            [Values(false, true)] bool async)
        {
            var operationExecutor = new MockOperationExecutor();
            var clientSettings = DriverTestConfiguration.GetClientSettings();
            var subject = new MongoClient(clientSettings, _ => operationExecutor);
            var session = usingSession ? CreateClientSession() : null;
            var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<BsonDocument>>().Limit(1);
            var options = new ChangeStreamOptions
            {
                BatchSize = 123,
                Collation = new Collation("en-us"),
                FullDocument = ChangeStreamFullDocumentOption.UpdateLookup,
                MaxAwaitTime = TimeSpan.FromSeconds(123),
                ResumeAfter = new BsonDocument(),
                StartAfter = new BsonDocument(),
                StartAtOperationTime = new BsonTimestamp(1, 2)
            };
            using var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            var renderedPipeline = new[] { BsonDocument.Parse("{ $limit : 1 }") };

            if (usingSession)
            {
                if (async)
                {
                    subject.WatchAsync(session, pipeline, options, cancellationToken).GetAwaiter().GetResult();
                }
                else
                {
                    subject.Watch(session, pipeline, options, cancellationToken);
                }
            }
            else
            {
                if (async)
                {
                    subject.WatchAsync(pipeline, options, cancellationToken).GetAwaiter().GetResult();
                }
                else
                {
                    subject.Watch(pipeline, options, cancellationToken);
                }
            }

            var call = operationExecutor.GetReadCall<IChangeStreamCursor<ChangeStreamDocument<BsonDocument>>>();
            if (usingSession)
            {
                call.SessionId.Should().BeSameAs(session.ServerSession.Id);
            }
            else
            {
                call.UsedImplicitSession.Should().BeTrue();
            }
            call.CancellationToken.Should().Be(cancellationToken);

            var changeStreamOperation = call.Operation.Should().BeOfType<ChangeStreamOperation<ChangeStreamDocument<BsonDocument>>>().Subject;
            changeStreamOperation.BatchSize.Should().Be(options.BatchSize);
            changeStreamOperation.Collation.Should().BeSameAs(options.Collation);
            changeStreamOperation.CollectionNamespace.Should().BeNull();
            changeStreamOperation.DatabaseNamespace.Should().BeNull();
            changeStreamOperation.FullDocument.Should().Be(options.FullDocument);
            changeStreamOperation.MaxAwaitTime.Should().Be(options.MaxAwaitTime);
            changeStreamOperation.MessageEncoderSettings.Should().NotBeNull();
            changeStreamOperation.Pipeline.Should().Equal(renderedPipeline);
            changeStreamOperation.ReadConcern.Should().Be(clientSettings.ReadConcern);
            changeStreamOperation.ResultSerializer.Should().BeOfType<ChangeStreamDocumentSerializer<BsonDocument>>();
            changeStreamOperation.ResumeAfter.Should().Be(options.ResumeAfter);
            changeStreamOperation.StartAfter.Should().Be(options.StartAfter);
            changeStreamOperation.StartAtOperationTime.Should().Be(options.StartAtOperationTime);
        }

        [Fact]
        public void WithReadConcern_should_return_expected_result()
        {
            var originalReadConcern = new ReadConcern(ReadConcernLevel.Linearizable);
            var subject = new MongoClient().WithReadConcern(originalReadConcern);
            var newReadConcern = new ReadConcern(ReadConcernLevel.Majority);

            var result = subject.WithReadConcern(newReadConcern);

            subject.Settings.ReadConcern.Should().BeSameAs(originalReadConcern);
            result.Settings.ReadConcern.Should().BeSameAs(newReadConcern);
            result.WithReadConcern(originalReadConcern).Settings.Should().Be(subject.Settings);
        }

        [Fact]
        public void WithReadPreference_should_return_expected_result()
        {
            var originalReadPreference = new ReadPreference(ReadPreferenceMode.Secondary);
            var subject = new MongoClient().WithReadPreference(originalReadPreference);
            var newReadPreference = new ReadPreference(ReadPreferenceMode.SecondaryPreferred);

            var result = subject.WithReadPreference(newReadPreference);

            subject.Settings.ReadPreference.Should().BeSameAs(originalReadPreference);
            result.Settings.ReadPreference.Should().BeSameAs(newReadPreference);
            result.WithReadPreference(originalReadPreference).Settings.Should().Be(subject.Settings);
        }

        [Fact]
        public void WithWriteConcern_should_return_expected_result()
        {
            var originalWriteConcern = new WriteConcern(2);
            var subject = new MongoClient().WithWriteConcern(originalWriteConcern);
            var newWriteConcern = new WriteConcern(3);

            var result = subject.WithWriteConcern(newWriteConcern);

            subject.Settings.WriteConcern.Should().BeSameAs(originalWriteConcern);
            result.Settings.WriteConcern.Should().BeSameAs(newWriteConcern);
            result.WithWriteConcern(originalWriteConcern).Settings.Should().Be(subject.Settings);
        }

        // private methods
        private IClientSessionHandle CreateClientSession()
        {
            var client = new Mock<IMongoClient>().Object;
            var options = new ClientSessionOptions();
            var cluster = Mock.Of<IClusterInternal>();
            var coreServerSession = new CoreServerSession();
            var coreSession = new CoreSession(cluster, coreServerSession, options.ToCore());
            var coreSessionHandle = new CoreSessionHandle(coreSession);
            return new ClientSessionHandle(client, options, coreSessionHandle);
        }
    }
}
