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
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.TestHelpers;
using MongoDB.Driver.Core.TestHelpers.XunitExtensions;
using MongoDB.TestHelpers.XunitExtensions;
using Xunit;

namespace MongoDB.Driver.Core.Operations
{
    public class AggregateOperationTests : OperationTestBase
    {
        private static BsonDocument[] __pipeline = new[] { BsonDocument.Parse("{ $match : { x : 'x' } }") };
        private static IBsonSerializer<BsonDocument> __resultSerializer = BsonDocumentSerializer.Instance;

        [Fact]
        public void Constructor_with_database_should_create_a_valid_instance()
        {
            var subject = new AggregateOperation<BsonDocument>(_databaseNamespace, __pipeline, __resultSerializer, _messageEncoderSettings);

            subject.CollectionNamespace.Should().BeNull();
            subject.DatabaseNamespace.Should().BeSameAs(_databaseNamespace);
            subject.Pipeline.Should().Equal(__pipeline);
            subject.ResultSerializer.Should().BeSameAs(__resultSerializer);
            subject.MessageEncoderSettings.Should().BeSameAs(_messageEncoderSettings);

            subject.AllowDiskUse.Should().NotHaveValue();
            subject.BatchSize.Should().NotHaveValue();
            subject.Collation.Should().BeNull();
            subject.MaxAwaitTime.Should().NotHaveValue();
            subject.MaxTime.Should().NotHaveValue();
            subject.ReadConcern.IsServerDefault.Should().BeTrue();
#pragma warning disable 618
            subject.UseCursor.Should().NotHaveValue();
#pragma warning restore 618
            subject.RetryRequested.Should().BeFalse();
        }

        [Fact]
        public void Constructor_with_database_should_throw_when_databaseNamespace_is_null()
        {
            var exception = Record.Exception(() => new AggregateOperation<BsonDocument>((DatabaseNamespace)null, __pipeline, __resultSerializer, _messageEncoderSettings));

            var argumentNullException = exception.Should().BeOfType<ArgumentNullException>().Subject;
            argumentNullException.ParamName.Should().Be("databaseNamespace");
        }

        [Fact]
        public void Constructor_with_database_should_throw_when_pipeline_is_null()
        {
            var exception = Record.Exception(() => new AggregateOperation<BsonDocument>(_databaseNamespace, null, __resultSerializer, _messageEncoderSettings));

            var argumentNullException = exception.Should().BeOfType<ArgumentNullException>().Subject;
            argumentNullException.ParamName.Should().Be("pipeline");
        }

        [Fact]
        public void Constructor_with_database_should_throw_when_resultSerializer_is_null()
        {
            var exception = Record.Exception(() => new AggregateOperation<BsonDocument>(_databaseNamespace, __pipeline, null, _messageEncoderSettings));

            var argumentNullException = exception.Should().BeOfType<ArgumentNullException>().Subject;
            argumentNullException.ParamName.Should().Be("resultSerializer");
        }

        [Fact]
        public void Constructor_with_database_should_throw_when_messageEncoderSettings_is_null()
        {
            var exception = Record.Exception(() => new AggregateOperation<BsonDocument>(_databaseNamespace, __pipeline, __resultSerializer, null));

            var argumentNullException = exception.Should().BeOfType<ArgumentNullException>().Subject;
            argumentNullException.ParamName.Should().Be("messageEncoderSettings");
        }

        [Fact]
        public void Constructor_with_collection_should_create_a_valid_instance()
        {
            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, __pipeline, __resultSerializer, _messageEncoderSettings);

            subject.CollectionNamespace.Should().Be(_collectionNamespace);
            subject.DatabaseNamespace.Should().BeNull();
            subject.Pipeline.Should().Equal(__pipeline);
            subject.ResultSerializer.Should().BeSameAs(__resultSerializer);
            subject.MessageEncoderSettings.Should().BeSameAs(_messageEncoderSettings);

            subject.AllowDiskUse.Should().NotHaveValue();
            subject.BatchSize.Should().NotHaveValue();
            subject.Collation.Should().BeNull();
            subject.MaxAwaitTime.Should().NotHaveValue();
            subject.MaxTime.Should().NotHaveValue();
            subject.ReadConcern.IsServerDefault.Should().BeTrue();
#pragma warning disable 618
            subject.UseCursor.Should().NotHaveValue();
#pragma warning restore 618
            subject.RetryRequested.Should().BeFalse();
        }

        [Fact]
        public void Constructor_with_collection_should_throw_when_collectionNamespace_is_null()
        {
            var exception = Record.Exception(() => new AggregateOperation<BsonDocument>((CollectionNamespace)null, __pipeline, __resultSerializer, _messageEncoderSettings));

            var argumentNullException = exception.Should().BeOfType<ArgumentNullException>().Subject;
            argumentNullException.ParamName.Should().Be("collectionNamespace");
        }

        [Fact]
        public void Constructor_with_collection_should_throw_when_pipeline_is_null()
        {
            var exception = Record.Exception(() => new AggregateOperation<BsonDocument>(_collectionNamespace, null, __resultSerializer, _messageEncoderSettings));

            var argumentNullException = exception.Should().BeOfType<ArgumentNullException>().Subject;
            argumentNullException.ParamName.Should().Be("pipeline");
        }

        [Fact]
        public void Constructor_with_collection_should_throw_when_resultSerializer_is_null()
        {
            var exception = Record.Exception(() => new AggregateOperation<BsonDocument>(_collectionNamespace, __pipeline, null, _messageEncoderSettings));

            var argumentNullException = exception.Should().BeOfType<ArgumentNullException>().Subject;
            argumentNullException.ParamName.Should().Be("resultSerializer");
        }

        [Fact]
        public void Constructor_with_collection_should_throw_when_messageEncoderSettings_is_null()
        {
            var exception = Record.Exception(() => new AggregateOperation<BsonDocument>(_collectionNamespace, __pipeline, __resultSerializer, null));

            var argumentNullException = exception.Should().BeOfType<ArgumentNullException>().Subject;
            argumentNullException.ParamName.Should().Be("messageEncoderSettings");
        }

        [Fact]
        public void AllowDiskUse_get_and_set_should_work()
        {
            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, __pipeline, __resultSerializer, _messageEncoderSettings);

            subject.AllowDiskUse = true;
            var result = subject.AllowDiskUse;

            result.Should().Be(true);
        }

        [Fact]
        public void BatchSize_get_and_set_should_work()
        {
            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, __pipeline, __resultSerializer, _messageEncoderSettings);

            subject.BatchSize = 23;
            var result = subject.BatchSize;

            result.Should().Be(23);
        }

        [Fact]
        public void Collation_get_and_set_should_work()
        {
            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, __pipeline, __resultSerializer, _messageEncoderSettings);
            var collation = new Collation("en_US");

            subject.Collation = collation;
            var result = subject.Collation;

            result.Should().BeSameAs(collation);
        }

        [Fact]
        public void Comment_get_and_set_should_work()
        {
            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, __pipeline, __resultSerializer, _messageEncoderSettings);
            var value = (BsonValue)"test";

            subject.Comment = value;
            var result = subject.Comment;

            result.Should().BeSameAs(value);
        }

        [Fact]
        public void Hint_get_and_set_should_work()
        {
            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, __pipeline, __resultSerializer, _messageEncoderSettings);
            var value = new BsonDocument("x", 1);

            subject.Hint = value;
            var result = subject.Hint;

            result.Should().BeSameAs(value);
        }

        [Fact]
        public void Let_get_and_set_should_work()
        {
            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, __pipeline, __resultSerializer, _messageEncoderSettings);
            var value = new BsonDocument("y", "z");

            subject.Let = value;
            var result = subject.Let;

            result.Should().BeSameAs(value);
        }

        [Fact]
        public void MaxAwaitTime_get_and_set_should_work()
        {
            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, __pipeline, __resultSerializer, _messageEncoderSettings);
            var value = TimeSpan.FromSeconds(2);

            subject.MaxAwaitTime = value;
            var result = subject.MaxAwaitTime;

            result.Should().Be(value);
        }

        [Theory]
        [ParameterAttributeData]
        public void MaxTime_get_and_set_should_work(
            [Values(-10000, 0, 1, 10000, 99999)] long maxTimeTicks)
        {
            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, __pipeline, __resultSerializer, _messageEncoderSettings);
            var value = TimeSpan.FromTicks(maxTimeTicks);

            subject.MaxTime = value;
            var result = subject.MaxTime;

            result.Should().Be(value);
        }

        [Theory]
        [ParameterAttributeData]
        public void MaxTime_set_should_throw_when_value_is_invalid(
            [Values(-10001, -9999, -1)] long maxTimeTicks)
        {
            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, __pipeline, __resultSerializer, _messageEncoderSettings);
            var value = TimeSpan.FromTicks(maxTimeTicks);

            var exception = Record.Exception(() => subject.MaxTime = value);

            var e = exception.Should().BeOfType<ArgumentOutOfRangeException>().Subject;
            e.ParamName.Should().Be("value");
        }

        [Fact]
        public void ReadConcern_get_and_set_should_work()
        {
            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, __pipeline, __resultSerializer, _messageEncoderSettings);
            var value = new ReadConcern(ReadConcernLevel.Linearizable);

            subject.ReadConcern = value;
            var result = subject.ReadConcern;

            result.Should().BeSameAs(value);
        }

        [Theory]
        [ParameterAttributeData]
        public void RetryRequested_get_and_set_should_work(
            [Values(false, true)] bool value)
        {
            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, __pipeline, __resultSerializer, _messageEncoderSettings);

            subject.RetryRequested = value;
            var result = subject.RetryRequested;

            result.Should().Be(value);
        }

        [Fact]
        public void UseCursor_get_and_set_should_work()
        {
            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, __pipeline, __resultSerializer, _messageEncoderSettings);

#pragma warning disable 618
            subject.UseCursor = true;
            var result = subject.UseCursor;
#pragma warning restore 618

            result.Should().BeTrue();
        }

        [Fact]
        public void CreateCommand_should_return_the_expected_result()
        {
            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, __pipeline, __resultSerializer, _messageEncoderSettings);
            var connectionDescription = OperationTestHelper.CreateConnectionDescription();
            var session = OperationTestHelper.CreateSession();

            var result = subject.CreateCommand(OperationContext.NoTimeout, session, connectionDescription);

            var expectedResult = new BsonDocument
            {
                { "aggregate", _collectionNamespace.CollectionName },
                { "pipeline", new BsonArray(__pipeline) },
                { "cursor", new BsonDocument() }
            };
            result.Should().Be(expectedResult);
        }

        [Theory]
        [ParameterAttributeData]
        public void CreateCommand_should_return_the_expected_result_when_AllowDiskUse_is_set(
            [Values(null, false, true)]
            bool? allowDiskUse)
        {
            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, __pipeline, __resultSerializer, _messageEncoderSettings)
            {
                AllowDiskUse = allowDiskUse
            };

            var connectionDescription = OperationTestHelper.CreateConnectionDescription();

            var session = OperationTestHelper.CreateSession();

            var result = subject.CreateCommand(OperationContext.NoTimeout, session, connectionDescription);

            var expectedResult = new BsonDocument
            {
                { "aggregate", _collectionNamespace.CollectionName },
                { "pipeline", new BsonArray(__pipeline) },
                { "allowDiskUse", () => allowDiskUse.Value, allowDiskUse != null },
                { "cursor", new BsonDocument() }
            };
            result.Should().Be(expectedResult);
        }

        [Theory]
        [ParameterAttributeData]
        public void CreateCommand_should_return_the_expected_result_when_BatchSize_is_set(
            [Values(null, 1)]
            int? batchSize)
        {
            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, __pipeline, __resultSerializer, _messageEncoderSettings)
            {
                BatchSize = batchSize
            };
            var connectionDescription = OperationTestHelper.CreateConnectionDescription();
            var session = OperationTestHelper.CreateSession();

            var result = subject.CreateCommand(OperationContext.NoTimeout, session, connectionDescription);

            var cursor = new BsonDocument
            {
                {"batchSize", () => batchSize.Value, batchSize != null}
            };
            var expectedResult = new BsonDocument
            {
                { "aggregate", _collectionNamespace.CollectionName },
                { "pipeline", new BsonArray(__pipeline) },
                { "cursor", cursor }
            };
            result.Should().Be(expectedResult);
        }

        [Theory]
        [ParameterAttributeData]
        public void CreateCommand_should_return_the_expected_result_when_Collation_is_set(
            [Values(null, "en_US", "fr_CA")]
            string locale)
        {
            var collation = locale == null ? null : new Collation(locale);
            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, __pipeline, __resultSerializer, _messageEncoderSettings)
            {
                Collation = collation
            };

            var connectionDescription = OperationTestHelper.CreateConnectionDescription();
            var session = OperationTestHelper.CreateSession();

            var result = subject.CreateCommand(OperationContext.NoTimeout, session, connectionDescription);

            var expectedResult = new BsonDocument
            {
                { "aggregate", _collectionNamespace.CollectionName },
                { "pipeline", new BsonArray(__pipeline) },
                { "collation", () => new BsonDocument("locale", locale), collation != null },
                { "cursor", new BsonDocument() }
            };
            result.Should().Be(expectedResult);
        }

        [Theory]
        [ParameterAttributeData]
        public void CreateCommand_should_return_expected_result_when_Comment_is_set(
            [Values(null, "test")]
            string comment)
        {
            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, __pipeline, __resultSerializer, _messageEncoderSettings)
            {
                Comment = comment,
            };

            var connectionDescription = OperationTestHelper.CreateConnectionDescription();
            var session = OperationTestHelper.CreateSession();

            var result = subject.CreateCommand(OperationContext.NoTimeout, session, connectionDescription);

            var expectedResult = new BsonDocument
            {
                { "aggregate", _collectionNamespace.CollectionName },
                { "pipeline", new BsonArray(__pipeline) },
                { "comment", () => comment, comment != null },
                { "cursor", new BsonDocument() }
            };
            result.Should().Be(expectedResult);
        }

        [Theory]
        [ParameterAttributeData]
        public void CreateCommand_should_return_the_expected_result_when_Hint_is_set(
            [Values(null, "{x: 1}")]
            string hintJson)
        {
            var hint = hintJson == null ? null : BsonDocument.Parse(hintJson);
            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, __pipeline, __resultSerializer, _messageEncoderSettings)
            {
                Hint = hint
            };

            var connectionDescription = OperationTestHelper.CreateConnectionDescription();
            var session = OperationTestHelper.CreateSession();

            var result = subject.CreateCommand(OperationContext.NoTimeout, session, connectionDescription);

            var expectedResult = new BsonDocument
            {
                { "aggregate", _collectionNamespace.CollectionName },
                { "pipeline", new BsonArray(__pipeline) },
                { "hint", () => hint, hint != null },
                { "cursor", new BsonDocument() }
            };
            result.Should().Be(expectedResult);
        }

        [Theory]
        [ParameterAttributeData]
        public void CreateCommand_should_return_expected_result_when_Let_is_set(
            [Values(null, "{ y : 'z' }")]
            string letJson)
        {
            var let = letJson == null ? null : BsonDocument.Parse(letJson);
            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, __pipeline, __resultSerializer, _messageEncoderSettings)
            {
                Let = let
            };

            var connectionDescription = OperationTestHelper.CreateConnectionDescription(Feature.AggregateOptionsLet.FirstSupportedWireVersion);
            var session = OperationTestHelper.CreateSession();

            var result = subject.CreateCommand(OperationContext.NoTimeout, session, connectionDescription);

            var expectedResult = new BsonDocument
            {
                { "aggregate", _collectionNamespace.CollectionName },
                { "pipeline", new BsonArray(__pipeline) },
                { "let", () => let, let != null },
                { "cursor", new BsonDocument() }
            };
            result.Should().Be(expectedResult);
        }

        [Theory]
        [InlineData(-10000, 0)]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(9999, 1)]
        [InlineData(10000, 1)]
        [InlineData(10001, 2)]
        public void CreateCommand_should_return_expected_result_when_MaxTime_is_set(long maxTimeTicks, int expectedMaxTimeMS)
        {
            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, __pipeline, __resultSerializer, _messageEncoderSettings)
            {
                MaxTime = TimeSpan.FromTicks(maxTimeTicks)
            };
            var connectionDescription = OperationTestHelper.CreateConnectionDescription();
            var session = OperationTestHelper.CreateSession();

            var result = subject.CreateCommand(OperationContext.NoTimeout, session, connectionDescription);

            var expectedResult = new BsonDocument
            {
                { "aggregate", _collectionNamespace.CollectionName },
                { "pipeline", new BsonArray(__pipeline) },
                { "maxTimeMS", expectedMaxTimeMS },
                { "cursor", new BsonDocument() }
            };
            result.Should().Be(expectedResult);
            result["maxTimeMS"].BsonType.Should().Be(BsonType.Int32);
        }

        [Theory]
        [InlineData(42)]
        [InlineData(-1)]
        public void CreateCommand_should_ignore_maxtime_if_timeout_specified(int timeoutMs)
        {
            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, __pipeline, __resultSerializer, _messageEncoderSettings)
            {
                MaxTime = TimeSpan.FromTicks(10)
            };
            var connectionDescription = OperationTestHelper.CreateConnectionDescription();
            var session = OperationTestHelper.CreateSession();

            var operationContext = new OperationContext(TimeSpan.FromMilliseconds(timeoutMs), CancellationToken.None);
            var result = subject.CreateCommand(operationContext, session, connectionDescription);

            result.Should().NotContain("maxTimeMS");
        }

        [Theory]
        [ParameterAttributeData]
        public void CreateCommand_should_return_the_expected_result_when_ReadConcern_is_set(
            [Values(null, ReadConcernLevel.Linearizable)]
            ReadConcernLevel? level)
        {
            var readConcern = new ReadConcern(level);
            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, __pipeline, __resultSerializer, _messageEncoderSettings)
            {
                ReadConcern = readConcern
            };

            var connectionDescription = OperationTestHelper.CreateConnectionDescription();
            var session = OperationTestHelper.CreateSession();

            var result = subject.CreateCommand(OperationContext.NoTimeout, session, connectionDescription);

            var expectedResult = new BsonDocument
            {
                { "aggregate", _collectionNamespace.CollectionName },
                { "pipeline", new BsonArray(__pipeline) },
                { "readConcern", () => readConcern.ToBsonDocument(), level != null },
                { "cursor", new BsonDocument() }
            };
            result.Should().Be(expectedResult);
        }

        [Theory]
        [ParameterAttributeData]
        public void CreateCommand_should_return_the_expected_result_when_using_causal_consistency(
            [Values(null, ReadConcernLevel.Linearizable)]
            ReadConcernLevel? level)
        {
            var readConcern = new ReadConcern(level);
            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, __pipeline, __resultSerializer, _messageEncoderSettings)
            {
                ReadConcern = readConcern
            };

            var connectionDescription = OperationTestHelper.CreateConnectionDescription(supportsSessions: true);
            var session = OperationTestHelper.CreateSession(true, new BsonTimestamp(100));

            var result = subject.CreateCommand(OperationContext.NoTimeout, session, connectionDescription);

            var expectedReadConcernDocument = readConcern.ToBsonDocument();
            expectedReadConcernDocument["afterClusterTime"] = new BsonTimestamp(100);

            var expectedResult = new BsonDocument
            {
                { "aggregate", _collectionNamespace.CollectionName },
                { "pipeline", new BsonArray(__pipeline) },
                { "readConcern", expectedReadConcernDocument },
                { "cursor", new BsonDocument() }
            };
            result.Should().Be(expectedResult);
        }

        [Theory]
        [ParameterAttributeData]
        public void Execute_should_return_expected_result(
            [Values(false, true)]
            bool async)
        {
            RequireServer.Check();
            EnsureTestData();
            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, __pipeline, __resultSerializer, _messageEncoderSettings);

            var cursor = ExecuteOperation(subject, async);
            var result = ReadCursorToEnd(cursor, async);

            result.Should().NotBeNull();
            result.Should().HaveCount(1);
        }

        [Theory]
        [ParameterAttributeData]
        public void Execute_should_throw_when_binding_is_null(
            [Values(false, true)]
            bool async)
        {
            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, __pipeline, __resultSerializer, _messageEncoderSettings);

            var exception = Record.Exception(() => ExecuteOperation(subject, binding: null, async: async));
            var argumentNullException = exception.Should().BeOfType<ArgumentNullException>().Subject;
            argumentNullException.ParamName.Should().Be("binding");
        }

        [Theory]
        [ParameterAttributeData]
        public void Execute_should_throw_when_maxTime_is_exceeded(
            [Values(false, true)] bool async)
        {
            RequireServer.Check().ClusterTypes(ClusterType.Standalone, ClusterType.ReplicaSet);

            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, __pipeline, __resultSerializer, _messageEncoderSettings) { MaxTime = TimeSpan.FromSeconds(9001) };

            using (var failPoint = FailPoint.ConfigureAlwaysOn(_cluster, _session, FailPointName.MaxTimeAlwaysTimeout))
            {
                var exception = Record.Exception(() => ExecuteOperation(subject, failPoint.Binding, async));

                exception.Should().BeOfType<MongoExecutionTimeoutException>();
            }
        }

        [Theory]
        [ParameterAttributeData]
        public void Execute_should_throw_when_pipeline_ends_with_out_or_merge(
            [Values("$out", "$merge")]
            string operatorName,
            [Values(false, true)]
            bool async)
        {
            var pipeline = new[] { BsonDocument.Parse($"{{ {operatorName} : \"xyz\" }}") };
            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, pipeline, __resultSerializer, _messageEncoderSettings);

            var exception = Record.Exception(() => ExecuteOperation(subject, async));

            var argumentException = exception.Should().BeOfType<ArgumentException>().Subject;
            argumentException.ParamName.Should().Be("pipeline");
        }

        [Theory]
        [ParameterAttributeData]
        public void Execute_should_return_expected_result_when_AllowDiskUse_is_set(
            [Values(null, false, true)]
            bool? allowDiskUse,
            [Values(false, true)]
            bool async)
        {
            RequireServer.Check();
            EnsureTestData();
            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, __pipeline, __resultSerializer, _messageEncoderSettings)
            {
                AllowDiskUse = allowDiskUse
            };

            var cursor = ExecuteOperation(subject, async);
            var result = ReadCursorToEnd(cursor, async);

            result.Should().NotBeNull();
            result.Should().HaveCount(1);
        }

        [Theory]
        [ParameterAttributeData]
        public void Execute_should_return_expected_result_when_BatchSize_is_set(
            [Values(null, 1, 10)]
            int? batchSize,
            [Values(false, true)]
            bool async)
        {
            RequireServer.Check();
            EnsureTestData();
            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, __pipeline, __resultSerializer, _messageEncoderSettings)
            {
                BatchSize = batchSize
            };

            using (var cursor = ExecuteOperation(subject, async))
            {
                var result = ReadCursorToEnd(cursor, async);

                result.Should().NotBeNull();
                result.Should().HaveCount(1);
            }
        }

        [Theory]
        [ParameterAttributeData]
        public void Execute_should_return_expected_result_when_Collation_is_set(
            [Values(false, true)]
            bool caseSensitive,
            [Values(false, true)]
            bool async)
        {
            RequireServer.Check();
            EnsureTestData();
            var collation = new Collation("en_US", caseLevel: caseSensitive, strength: CollationStrength.Primary);
            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, __pipeline, __resultSerializer, _messageEncoderSettings)
            {
                Collation = collation
            };

            var cursor = ExecuteOperation(subject, async);
            var result = ReadCursorToEnd(cursor, async);

            result.Should().NotBeNull();
            result.Should().HaveCount(caseSensitive ? 1 : 2);
        }

        [Theory]
        [ParameterAttributeData]
        public void Execute_should_return_expected_result_when_Comment_is_set(
            [Values(false, true)]
            bool async)
        {
            RequireServer.Check().ClusterTypes(ClusterType.Standalone, ClusterType.ReplicaSet);
            EnsureTestData();

            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, __pipeline, __resultSerializer, _messageEncoderSettings)
            {
                Comment = "test"
            };

            using (var profile = Profile(_collectionNamespace.DatabaseNamespace))
            {
                var cursor = ExecuteOperation(subject, async);
                var result = ReadCursorToEnd(cursor, async);

                result.Should().NotBeNull();

                var profileEntries = profile.Find(new BsonDocument("command.aggregate", new BsonDocument("$exists", true)));
                profileEntries.Should().HaveCount(1);
                profileEntries[0]["command"]["comment"].Should().Be(subject.Comment);
            }
        }

        [Theory]
        [ParameterAttributeData]
        public void Execute_should_return_expected_result_when_Hint_is_set(
            [Values(false, true)]
            bool async)
        {
            RequireServer.Check();
            EnsureTestData();
            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, __pipeline, __resultSerializer, _messageEncoderSettings)
            {
                Hint = "_id_"
            };

            var cursor = ExecuteOperation(subject, async);
            var result = ReadCursorToEnd(cursor, async);

            result.Should().NotBeNull();
        }

        [Theory]
        [ParameterAttributeData]
        public void Execute_should_return_expected_result_when_Let_is_set_with_match_expression(
            [Values(false, true)]
            bool async)
        {
            RequireServer.Check().Supports(Feature.AggregateOptionsLet);
            EnsureTestData();
            var pipeline = new[] { BsonDocument.Parse("{ $match : { $expr : { $eq : [ '$x', '$$y'] } } }") };
            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, pipeline, __resultSerializer, _messageEncoderSettings)
            {
                Let = new BsonDocument("y", "x")
            };

            var cursor = ExecuteOperation(subject, async);
            var result = ReadCursorToEnd(cursor, async);

            result.Should().BeEquivalentTo(new[]
            {
                new BsonDocument { { "_id", 1 }, { "x", "x" } }
            });
        }

        [Theory]
        [ParameterAttributeData]
        public void Execute_should_return_expected_result_when_Let_is_set_with_project(
            [Values(false, true)]
            bool async)
        {
            RequireServer.Check().Supports(Feature.AggregateOptionsLet);
            EnsureTestData();
            var pipeline = new[] { BsonDocument.Parse("{ $project : { y : '$$z' } }") };
            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, pipeline, __resultSerializer, _messageEncoderSettings)
            {
                Let = new BsonDocument("z", "x")
            };

            var cursor = ExecuteOperation(subject, async);
            var result = ReadCursorToEnd(cursor, async);

            result.Should().BeEquivalentTo(new[]
            {
                new BsonDocument { { "_id", 1 }, { "y", "x" } },
                new BsonDocument { { "_id", 2 }, { "y", "x" } }
            });
        }

        [Theory]
        [ParameterAttributeData]
        public void Execute_should_return_expected_result_when_MaxAwaitTime_is_set(
            [Values(null, 1000)]
            int? milliseconds,
            [Values(false, true)]
            bool async)
        {
            RequireServer.Check();
            EnsureTestData();
            var maxAwaitTime = milliseconds == null ? (TimeSpan?)null : TimeSpan.FromMilliseconds(milliseconds.Value);
            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, __pipeline, __resultSerializer, _messageEncoderSettings)
            {
                MaxAwaitTime = maxAwaitTime
            };

            var cursor = ExecuteOperation(subject, async);

            cursor.Should().BeOfType<AsyncCursor<BsonDocument>>();
            var cursorMaxTimeInfo = typeof(AsyncCursor<BsonDocument>).GetField("_maxTime", BindingFlags.NonPublic | BindingFlags.Instance);
            var cursorMaxTime = (TimeSpan?)cursorMaxTimeInfo.GetValue(cursor);
            cursorMaxTime.Should().Be(maxAwaitTime);
        }

        [Theory]
        [ParameterAttributeData]
        public void Execute_should_return_expected_result_when_MaxTime_is_set(
            [Values(null, 1000)]
            int? milliseconds,
            [Values(false, true)]
            bool async)
        {
            RequireServer.Check();
            EnsureTestData();
            var maxTime = milliseconds == null ? (TimeSpan?)null : TimeSpan.FromMilliseconds(milliseconds.Value);
            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, __pipeline, __resultSerializer, _messageEncoderSettings)
            {
                MaxTime = maxTime
            };

            var cursor = ExecuteOperation(subject, async);
            var result = ReadCursorToEnd(cursor, async);

            result.Should().NotBeNull();
            result.Should().HaveCount(1);
        }

        [Theory]
        [ParameterAttributeData]
        public void Execute_should_return_expected_result_when_ReadConcern_is_set(
            [Values(null, ReadConcernLevel.Local)]
            ReadConcernLevel? level,
            [Values(false, true)]
            bool async)
        {
            RequireServer.Check();
            EnsureTestData();
            var readConcern = new ReadConcern(level);
            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, __pipeline, __resultSerializer, _messageEncoderSettings)
            {
                ReadConcern = readConcern
            };

            var cursor = ExecuteOperation(subject, async);
            var result = ReadCursorToEnd(cursor, async);

            result.Should().NotBeNull();
            result.Should().HaveCount(1);
        }

        [Theory]
        [ParameterAttributeData]
        public void Execute_should_return_expected_result_when_UseCursor_is_set(
            [Values(null, false, true)]
            bool? useCursor,
            [Values(false, true)]
            bool async)
        {
            RequireServer.Check();
            EnsureTestData();
            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, __pipeline, __resultSerializer, _messageEncoderSettings)
            {
#pragma warning disable 618
                UseCursor = useCursor
#pragma warning restore 618
            };

            var cursor = ExecuteOperation(subject, async);
            var result = ReadCursorToEnd(cursor, async);

            result.Should().NotBeNull();
            result.Should().HaveCount(1);
        }

        [Theory]
        [ParameterAttributeData]
        public void Execute_should_send_session_id_when_supported(
            [Values(false, true)] bool async)
        {
            RequireServer.Check();
            EnsureTestData();
            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, __pipeline, __resultSerializer, _messageEncoderSettings);

            VerifySessionIdWasSentWhenSupported(subject, "aggregate", async);
        }

        [Theory]
        [ParameterAttributeData]
        public async Task Execute_should_set_operation_name([Values(false, true)] bool async)
        {
            RequireServer.Check();
            EnsureTestData();
            var subject = new AggregateOperation<BsonDocument>(_collectionNamespace, __pipeline, __resultSerializer, _messageEncoderSettings);

            await VerifyOperationNameIsSet(subject, async, "aggregate");
        }

        private void EnsureTestData()
        {
            RunOncePerFixture(() =>
            {
                DropCollection();
                Insert(new BsonDocument { { "_id", 1 }, { "x", "x" } });
                Insert(new BsonDocument { { "_id", 2 }, { "x", "X" } });
            });
        }
    }

    public class AggregateOperation_AggregateResultDeserializerTests
    {
        [Fact]
        public void Equals_derived_should_return_false()
        {
            var x = new AggregateOperation<int>.AggregateResultDeserializer(Int32Serializer.Instance);
            var y = new DerivedFromAggregateResultDeserializer<int>(Int32Serializer.Instance);

            var result = x.Equals(y);

            result.Should().Be(false);
        }

        [Fact]
        public void Equals_null_should_return_false()
        {
            var x = new AggregateOperation<int>.AggregateResultDeserializer(Int32Serializer.Instance);

            var result = x.Equals(null);

            result.Should().Be(false);
        }

        [Fact]
        public void Equals_object_should_return_false()
        {
            var x = new AggregateOperation<int>.AggregateResultDeserializer(Int32Serializer.Instance);
            var y = new object();

            var result = x.Equals(y);

            result.Should().Be(false);
        }

        [Fact]
        public void Equals_self_should_return_true()
        {
            var x = new AggregateOperation<int>.AggregateResultDeserializer(Int32Serializer.Instance);

            var result = x.Equals(x);

            result.Should().Be(true);
        }

        [Fact]
        public void Equals_with_equal_fields_should_return_true()
        {
            var x = new AggregateOperation<int>.AggregateResultDeserializer(Int32Serializer.Instance);
            var y = new AggregateOperation<int>.AggregateResultDeserializer(Int32Serializer.Instance);

            var result = x.Equals(y);

            result.Should().Be(true);
        }

        [Fact]
        public void Equals_with_not_equal_field_should_return_false()
        {
            var resultSerializer1 = new Int32Serializer(BsonType.Int32);
            var resultSerializer2 = new Int32Serializer(BsonType.String);
            var x = new AggregateOperation<int>.AggregateResultDeserializer(resultSerializer1);
            var y = new AggregateOperation<int>.AggregateResultDeserializer(resultSerializer2);

            var result = x.Equals(y);

            result.Should().Be(false);
        }

        [Fact]
        public void GetHashCode_should_return_zero()
        {
            var x = new AggregateOperation<int>.AggregateResultDeserializer(Int32Serializer.Instance);

            var result = x.GetHashCode();

            result.Should().Be(0);
        }

        internal class DerivedFromAggregateResultDeserializer<TResult> : AggregateOperation<TResult>.AggregateResultDeserializer
        {
            public DerivedFromAggregateResultDeserializer(IBsonSerializer<TResult> resultSerializer) : base(resultSerializer)
            {
            }
        }
    }

    public class AggregateOperation_AggregateCursorDeserializerTests
    {
        [Fact]
        public void Equals_derived_should_return_false()
        {
            var x = new AggregateOperation<int>.CursorDeserializer(Int32Serializer.Instance);
            var y = new DerivedFromCursorDeserializer<int>(Int32Serializer.Instance);

            var result = x.Equals(y);

            result.Should().Be(false);
        }

        [Fact]
        public void Equals_null_should_return_false()
        {
            var x = new AggregateOperation<int>.CursorDeserializer(Int32Serializer.Instance);

            var result = x.Equals(null);

            result.Should().Be(false);
        }

        [Fact]
        public void Equals_object_should_return_false()
        {
            var x = new AggregateOperation<int>.CursorDeserializer(Int32Serializer.Instance);
            var y = new object();

            var result = x.Equals(y);

            result.Should().Be(false);
        }

        [Fact]
        public void Equals_self_should_return_true()
        {
            var x = new AggregateOperation<int>.CursorDeserializer(Int32Serializer.Instance);

            var result = x.Equals(x);

            result.Should().Be(true);
        }

        [Fact]
        public void Equals_with_equal_fields_should_return_true()
        {
            var x = new AggregateOperation<int>.CursorDeserializer(Int32Serializer.Instance);
            var y = new AggregateOperation<int>.CursorDeserializer(Int32Serializer.Instance);

            var result = x.Equals(y);

            result.Should().Be(true);
        }

        [Fact]
        public void Equals_with_not_equal_field_should_return_false()
        {
            var resultSerializer1 = new Int32Serializer(BsonType.Int32);
            var resultSerializer2 = new Int32Serializer(BsonType.String);
            var x = new AggregateOperation<int>.CursorDeserializer(resultSerializer1);
            var y = new AggregateOperation<int>.CursorDeserializer(resultSerializer2);

            var result = x.Equals(y);

            result.Should().Be(false);
        }

        [Fact]
        public void GetHashCode_should_return_zero()
        {
            var x = new AggregateOperation<int>.CursorDeserializer(Int32Serializer.Instance);

            var result = x.GetHashCode();

            result.Should().Be(0);
        }

        internal class DerivedFromCursorDeserializer<TResult> : AggregateOperation<TResult>.CursorDeserializer
        {
            public DerivedFromCursorDeserializer(IBsonSerializer<TResult> resultSerializer) : base(resultSerializer)
            {
            }
        }
    }
}
