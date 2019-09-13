/* Copyright 2019-present MongoDB Inc.
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
using MongoDB.Bson;
using MongoDB.Bson.TestHelpers;
using MongoDB.Driver.Encryption;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xunit;

namespace MongoDB.Driver.Tests
{
    public class MongocryptdFactoryTests
    {
        [Theory]
        [InlineData("mongocryptdURI1")]
        [InlineData("test")]
        public void Constructor_should_throw_when_an_invalid_extra_option_key(string key)
        {
            var extraOptions = new Dictionary<string, object> { { key, new object() } };
            var exception = Record.Exception(() => new MongocryptdFactory(extraOptions));
            var e = exception.Should().BeOfType<ArgumentException>().Subject;
            e.Message.Should().Be($"Invalid extra option key: {key}.");
        }

        [Theory]
        [InlineData("mongocryptdURI", "mongodb://localhost:11111", "mongodb://localhost:11111")]
        [InlineData(null, null, "mongodb://localhost:27020")]
        public void CreateMongocryptdConnectionString_should_create_expeced_connection_string(string optionKey, string optionValue, string expectedConnectionString)
        {
            var extraOptions = new Dictionary<string, object>();
            if (optionKey != null)
            {
                extraOptions.Add(optionKey, optionValue);
            }
            var subject = new MongocryptdFactory(extraOptions);
            var connectionString = subject.CreateMongocryptdConnectionString();
            connectionString.Should().Be(expectedConnectionString);
        }

        [Fact]
        public void CreateMongocryptdConnectionString_should_throw_when_argument_not_string()
        {
            var extraOptions = new Dictionary<string, object>();
            extraOptions.Add("mongocryptdURI", true);
            var subject = new MongocryptdFactory(extraOptions);
            var exception = Record.Exception(() => subject.CreateMongocryptdConnectionString());
            var e = exception.Should().BeOfType<InvalidCastException>().Subject;
            e.Message.Should().Be("Unable to cast object of type 'System.Boolean' to type 'System.String'.");
        }

        [SkippableTheory]
        [InlineData("{ mongocryptdBypassSpawn : true }", null, null, false)]
        [InlineData(null, "mongocryptd.exe", "--idleShutdownTimeoutSecs 60", true)]
        [InlineData("{ mongocryptdBypassSpawn : false }", "mongocryptd.exe", "--idleShutdownTimeoutSecs 60", true)]
        [InlineData("{ mongocryptdBypassSpawn : false }", "mongocryptd.exe", "--idleShutdownTimeoutSecs 60", true)]
        [InlineData("{ mongocryptdBypassSpawn : false, mongocryptdSpawnPath : 'c:/' }", "c:/mongocryptd.exe", "--idleShutdownTimeoutSecs 60", true)]
        [InlineData("{ mongocryptdBypassSpawn : false, mongocryptdSpawnPath : 'c:/mgcr.exe' }", "c:/mgcr.exe", "--idleShutdownTimeoutSecs 60", true)]
        [InlineData("{ mongocryptdBypassSpawn : false, mongocryptdSpawnPath : 'c:/mgcr.exe' }", "c:/mgcr.exe", "--idleShutdownTimeoutSecs 60", true)]
        // args string
        [InlineData("{ mongocryptdSpawnArgs : '--arg1 A --arg2 B' }", "mongocryptd.exe", "--arg1 A --arg2 B --idleShutdownTimeoutSecs 60", true)]
        [InlineData("{ mongocryptdSpawnArgs : '--arg1 A --arg2 B --idleShutdownTimeoutSecs 50' }", "mongocryptd.exe", "--arg1 A --arg2 B --idleShutdownTimeoutSecs 50", true)]
        // args list
        [InlineData("{ mongocryptdSpawnArgs : [ 'arg1 A', 'arg2 B'] }", "mongocryptd.exe", "--arg1 A --arg2 B --idleShutdownTimeoutSecs 60", true)]
        [InlineData("{ mongocryptdSpawnArgs : [ 'arg1 A', 'arg2 B', 'idleShutdownTimeoutSecs 50'] }", "mongocryptd.exe", "--arg1 A --arg2 B --idleShutdownTimeoutSecs 50", true)]
        [InlineData("{ mongocryptdSpawnArgs : [ '--arg1 A', '--arg2 B', '--idleShutdownTimeoutSecs 50'] }", "mongocryptd.exe", "--arg1 A --arg2 B --idleShutdownTimeoutSecs 50", true)]

        [InlineData("{ mongocryptdBypassSpawn : false, mongocryptdSpawnArgs : [ '--arg1 A', '--arg2 B', '--idleShutdownTimeoutSecs 50'] }", "mongocryptd.exe", "--arg1 A --arg2 B --idleShutdownTimeoutSecs 50", true)]
        public void Mongocryptd_should_be_spawned_with_correct_extra_arguments(
            string stringExtraOptions,
            string expectedPath,
            string expectedArgs,
            bool shouldBeSpawned)
        {
            var bsonDocumentExtraOptions =
                stringExtraOptions != null
                 ? BsonDocument.Parse(stringExtraOptions)
                 : new BsonDocument();

            object CreateTypedExtraOptions(BsonValue value)
            {
                if (value.IsBsonArray)
                {
                    return value.AsBsonArray.Select(c => c); // IEnumerable
                }
                else if (value.IsBoolean)
                {
                    return (bool)value; // bool
                }
                else
                {
                    return (string)value; // string
                }
            }

            var extraOptions = bsonDocumentExtraOptions
                .Elements
                .ToDictionary(k => k.Name, v => CreateTypedExtraOptions(v.Value));

            var subject = new MongocryptdFactory(extraOptions);

            var result = subject.ShouldMongocryptdBeSpawned(out var path, out var args);
            result.Should().Be(shouldBeSpawned);
            path.Should().Be(expectedPath);
            args.Should().Be(expectedArgs);
        }

        [Theory]
        [InlineData("mongocryptdBypassSpawn", 1)]
        [InlineData("mongocryptdSpawnArgs", 1)]
        public void Mongocryptd_should_throw_when_argument_has_unexpected_type(string key, object value)
        {
            var extraOptions = new Dictionary<string, object>();
            extraOptions.Add(key, value);
            var subject = new MongocryptdFactory(extraOptions);
            var exception = Record.Exception(() => subject.ShouldMongocryptdBeSpawned(out _, out _));
            var e = exception.Should().BeOfType<InvalidCastException>().Subject;
            e.Message.Should().Be($"Invalid type: {value.GetType().Name} of {key} option.");
        }
    }

    internal static class MongocryptdFactoryReflector
    {
        public static string CreateMongocryptdConnectionString(this MongocryptdFactory mongocryptdHelper)
        {
            return (string)Reflector.Invoke(mongocryptdHelper, nameof(CreateMongocryptdConnectionString));
        }

        public static bool ShouldMongocryptdBeSpawned(this MongocryptdFactory mongocryptdHelper, out string path, out string args)
        {
            return (bool)Reflector.Invoke(mongocryptdHelper, nameof(ShouldMongocryptdBeSpawned), out path, out args);
        }
    }
}
