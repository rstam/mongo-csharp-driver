﻿/* Copyright 2019-present MongoDB Inc.
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
using MongoDB.Bson.TestHelpers.JsonDrivenTests;
using MongoDB.Driver.Core;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Clusters.ServerSelectors;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.TestHelpers;
using MongoDB.Driver.Core.TestHelpers.JsonDrivenTests;
using MongoDB.Driver.Core.TestHelpers.XunitExtensions;
using MongoDB.Driver.TestHelpers;
using MongoDB.Driver.Tests.JsonDrivenTests;
using Xunit;

namespace MongoDB.Driver.Tests.Specifications.Runner
{
    public abstract class MongoClientJsonDrivenTestRunnerBase
    {
        // private fields
        private readonly string _collectionNameKey = "collection_name";
        private readonly string _databaseNameKey = "database_name";
        private readonly string _dataKey = "data";
        private readonly string _expectationsKey = "expectations";
        private readonly string _failPointKey = "failPoint";
        private readonly string _operationsKey = "operations";
        private readonly string _outcomeKey = "outcome";
        private readonly bool _shouldEventsBeChecked = true;
        private readonly string _skipReasonKey = "skipReason";
        private readonly HashSet<string> _defaultCommandsToNotCapture = new HashSet<string>
        {
            "isMaster",
            "buildInfo",
            "getLastError",
            "authenticate",
            "saslStart",
            "saslContinue",
            "getnonce"
        };

        private readonly string[] _expectedSharedColumns = { "_path", "database_name", "collection_name", "data", "tests", "minServerVersion" };

        private string DatabaseName { get; set; }
        private string CollectionName { get; set; }

        private IDictionary<string, object> _objectMap = null;

        // Protected
        // Virtual properties
        protected virtual HashSet<string> DefaultCommandsToNotCapture => _defaultCommandsToNotCapture;

        protected virtual string[] ExpectedSharedColumns => _expectedSharedColumns;

        protected virtual string DatabaseNameKey => _databaseNameKey;

        protected virtual string CollectionNameKey => _collectionNameKey;

        protected virtual string ExpectationsKey => _expectationsKey;

        protected abstract string[] ExpectedTestColumns { get; }

        protected virtual string OutcomeKey => _outcomeKey;

        protected virtual string FailPointKey => _failPointKey;

        protected virtual string DataKey => _dataKey;

        protected virtual string OperationsKey => _operationsKey;

        protected virtual string SkipReasonKey => _skipReasonKey;

        protected virtual bool ShouldEventsBeChecked => _shouldEventsBeChecked;

        protected IDictionary<string, object> ObjectMap => _objectMap;

        // Virtual methods
        protected virtual void AssertEvents(EventCapturer actualEvents, BsonDocument test)
        {
            AssertEvents(actualEvents, test, null);
        }

        protected virtual void AssertEvents(EventCapturer eventCapturer, BsonDocument test, Action<BsonDocument> prepareEventResult)
        {
            if (test.Contains(ExpectationsKey))
            {
                var expectedEvents = test[ExpectationsKey].AsBsonArray.Cast<BsonDocument>().ToList();
                var actualEvents = GetEvents(eventCapturer);

                var n = Math.Min(actualEvents.Count, expectedEvents.Count);
                for (var index = 0; index < n; index++)
                {
                    var actualEvent = actualEvents[index];

                    var expectedEvent = expectedEvents[index];
                    prepareEventResult?.Invoke(expectedEvent);
                    AssertEvent(actualEvent, expectedEvent);
                }
                if (actualEvents.Count < expectedEvents.Count)
                {
                    throw new Exception($"Missing command started event: {expectedEvents[n]}.");
                }

                if (actualEvents.Count > expectedEvents.Count)
                {
                    throw new Exception($"Unexpected command started event: {actualEvents[n].CommandName}.");
                }
            }
        }

        protected virtual void AssertOutcome(BsonDocument test)
        {
            if (test.TryGetValue(OutcomeKey, out var outcome))
            {
                foreach (var aspect in outcome.AsBsonDocument)
                {
                    switch (aspect.Name)
                    {
                        case "collection":
                            VerifyCollectionOutcome(aspect.Value.AsBsonDocument);
                            break;

                        default:
                            throw new FormatException($"Unexpected outcome aspect: {aspect.Name}.");
                    }
                }
            }
        }

        protected virtual void CheckServerRequirements(BsonDocument document)
        {
            if (document.Contains("minServerVersion"))
            {
                var minServerVersion = document["minServerVersion"].AsString;
                RequireServer.Check().VersionGreaterThanOrEqualTo(minServerVersion);
            }
        }

        protected virtual void ConfigureClientSettings(MongoClientSettings settings, BsonDocument test)
        {
            if (test.Contains("clientOptions"))
            {
                foreach (var option in test["clientOptions"].AsBsonDocument)
                {
                    switch (option.Name)
                    {
                        case "readConcernLevel":
                            var level = (ReadConcernLevel)Enum.Parse(typeof(ReadConcernLevel), option.Value.AsString, ignoreCase: true);
                            settings.ReadConcern = new ReadConcern(level);
                            break;

                        case "readPreference":
                            settings.ReadPreference = ReadPreferenceFromBsonValue(option.Value);
                            break;

                        case "retryWrites":
                            settings.RetryWrites = option.Value.ToBoolean();
                            break;

                        case "w":
                            if (option.Value.IsString)
                            {
                                settings.WriteConcern = new WriteConcern(option.Value.AsString);
                            }
                            else
                            {
                                settings.WriteConcern = new WriteConcern(option.Value.ToInt32());
                            }
                            break;

                        default:
                            throw new FormatException($"Unexpected client option: \"{option.Name}\".");
                    }
                }
            }
        }

        protected virtual void CustomDataValidation(BsonDocument shared, BsonDocument test)
        {
            // do nothing by default.
        }

        protected virtual void ExecuteOperations(IMongoClient client, Dictionary<string, object> objectMap, BsonDocument test)
        {
            _objectMap = objectMap;

            var factory = new JsonDrivenTestFactory(client, DatabaseName, CollectionName, bucketName: null, objectMap);

            foreach (var operation in test[OperationsKey].AsBsonArray.Cast<BsonDocument>())
            {
                var receiver = operation["object"].AsString;
                var name = operation["name"].AsString;
                var jsonDrivenTest = factory.CreateTest(receiver, name);

                jsonDrivenTest.Arrange(operation);
                if (test["async"].AsBoolean)
                {
                    jsonDrivenTest.ActAsync(CancellationToken.None).GetAwaiter().GetResult();
                }
                else
                {
                    jsonDrivenTest.Act(CancellationToken.None);
                }
                jsonDrivenTest.Assert();
            }
        }

        protected abstract void RunTest(BsonDocument shared, BsonDocument test, EventCapturer eventCapturer);

        protected virtual void TestInitialize()
        {
            // do nothing by default.
        }

        protected virtual void VerifyCollectionOutcome(BsonDocument outcome)
        {
            foreach (var aspect in outcome)
            {
                switch (aspect.Name)
                {
                    case "data":
                        VerifyCollectionData(aspect.Value.AsBsonArray.Cast<BsonDocument>());
                        break;

                    default:
                        throw new FormatException($"Unexpected collection outcome aspect: {aspect.Name}.");
                }
            }
        }

        protected FailPoint ConfigureFailPoint(BsonDocument test)
        {
            if (test.TryGetValue(FailPointKey, out var failPoint))
            {
                var cluster = DriverTestConfiguration.Client.Cluster;
                var server = cluster.SelectServer(WritableServerSelector.Instance, CancellationToken.None);
                var session = NoCoreSession.NewHandle();
                var command = failPoint.AsBsonDocument;
                return FailPoint.Configure(cluster, session, command);
            }

            return null;
        }

        protected void CreateCollection()
        {
            var client = DriverTestConfiguration.Client;
            var database = client.GetDatabase(DatabaseName).WithWriteConcern(WriteConcern.WMajority);
            database.CreateCollection(CollectionName);
        }

        protected DisposableMongoClient CreateDisposableClient(BsonDocument test, EventCapturer eventCapturer)
        {
            return DriverTestConfiguration.CreateDisposableClient(settings =>
            {
                ConfigureClientSettings(settings, test);
                if (eventCapturer != null)
                {
                    settings.ClusterConfigurator = c => c.Subscribe(eventCapturer);
                }
            });
        }

        protected void DropCollection()
        {
            var client = DriverTestConfiguration.Client;
            var database = client.GetDatabase(DatabaseName).WithWriteConcern(WriteConcern.WMajority);
            database.DropCollection(CollectionName);
        }

        protected void InsertData(BsonDocument shared)
        {
            if (shared.Contains(DataKey))
            {
                var documents = shared[DataKey].AsBsonArray.Cast<BsonDocument>().ToList();
                if (documents.Count > 0)
                {
                    var client = DriverTestConfiguration.Client;
                    var database = client.GetDatabase(DatabaseName);
                    var collection = database.GetCollection<BsonDocument>(CollectionName).WithWriteConcern(WriteConcern.WMajority);
                    collection.InsertMany(documents);
                }
            }
        }

        protected void SetupAndRunTest(JsonDrivenTestCase testCase)
        {
            CheckServerRequirements(testCase.Shared);
            SetupAndRunTest(testCase.Shared, testCase.Test);
        }

        protected void VerifyCollectionData(IEnumerable<BsonDocument> expectedDocuments)
        {
            var database = DriverTestConfiguration.Client.GetDatabase(DatabaseName);
            var collection = database.GetCollection<BsonDocument>(CollectionName);
            var actualDocuments = collection.Find("{}").ToList();
            actualDocuments.Should().BeEquivalentTo(expectedDocuments);
        }


        // private methods
        private void AssertEvent(object actualEvent, BsonDocument expectedEvent)
        {
            if (expectedEvent.ElementCount != 1)
            {
                throw new FormatException("Expected event must be a document with a single element with a name the specifies the type of the event.");
            }

            var eventType = expectedEvent.GetElement(0).Name;
            var eventAsserter = EventAsserterFactory.CreateAsserter(eventType);
            eventAsserter.AssertAspects(actualEvent, expectedEvent[0].AsBsonDocument);
        }

        private List<CommandStartedEvent> GetEvents(EventCapturer eventCapturer)
        {
            var events = new List<CommandStartedEvent>();

            while (eventCapturer.Any())
            {
                events.Add((CommandStartedEvent)eventCapturer.Next());
            }

            return events;
        }

        private ReadPreference ReadPreferenceFromBsonValue(BsonValue value)
        {
            if (value.BsonType == BsonType.String)
            {
                var mode = (ReadPreferenceMode)Enum.Parse(typeof(ReadPreferenceMode), value.AsString, ignoreCase: true);
                return new ReadPreference(mode);
            }

            return ReadPreference.FromBsonDocument(value.AsBsonDocument);
        }

        private void SetupAndRunTest(BsonDocument shared, BsonDocument test)
        {
            if (test.Contains(SkipReasonKey))
            {
                throw new SkipException(test[SkipReasonKey].AsString);
            }

            Ensure.IsNotNullOrEmpty(DatabaseNameKey, nameof(DatabaseNameKey));
            Ensure.IsNotNullOrEmpty(CollectionNameKey, nameof(CollectionNameKey));

            JsonDrivenHelper.EnsureAllFieldsAreValid(shared, ExpectedSharedColumns);
            JsonDrivenHelper.EnsureAllFieldsAreValid(test, ExpectedTestColumns);
            CustomDataValidation(shared, test);

            DatabaseName = shared[DatabaseNameKey].AsString;
            CollectionName = shared[CollectionNameKey].AsString;

            TestInitialize();
            DropCollection();
            CreateCollection();
            InsertData(shared);

            using (ConfigureFailPoint(test))
            {
                EventCapturer eventCapturer = null;
                if (ShouldEventsBeChecked)
                {
                    eventCapturer = new EventCapturer().Capture<CommandStartedEvent>(e => !DefaultCommandsToNotCapture.Contains(e.CommandName));
                }

                RunTest(shared, test, eventCapturer);
                if (ShouldEventsBeChecked)
                {
                    AssertEvents(eventCapturer, test);
                }

                AssertOutcome(test);
            }
        }
    }
}