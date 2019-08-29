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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FluentAssertions;
using MongoDB.Bson.TestHelpers;
using Moq;
using Xunit;

namespace MongoDB.Driver.Tests
{
    public class EncryptionClientsTests
    {
        [Theory]
        [InlineData(null, "mongodb://localhost:27020")]
        [InlineData("mongodb://test:27021", "mongodb://test:27021")]
        public void CreateMongoCryptDConnectionStringTest(string mongocryptdURI, string expectedConnectionString)
        {
            var extraOptions = GetExtraOptions("mongocryptdURI", mongocryptdURI);
            var subject = CreateSubject();
            var result = subject.CreateMongoCryptDConnectionString(extraOptions);
            result.Should().Be(expectedConnectionString);
        }

        private IEncryptionClients CreateSubject()
        {
            return EncryptionClients.CreateEncryptionClientsIfNecessary(
                new MongoClientSettings
                {
                    AutoEncryptionOptions = new AutoEncryptionOptions(
                        CollectionNamespace.FromFullName("test.test"),
                        Mock.Of<IReadOnlyDictionary<string, IReadOnlyDictionary<string, object>>>())
                });
        }

        private IReadOnlyDictionary<string, object> GetExtraOptions(string key, string value)
        {
            if (value == null)
            {
                return new ReadOnlyDictionary<string, object>(new Dictionary<string, object>());
            }
            else
            {
                var dict = new Dictionary<string, object>()
                {
                    { key, value }
                };
                return new ReadOnlyDictionary<string, object>(dict);
            }
        }
    }

    internal static class EncryptionClientsReflector
    {
        internal static string CreateMongoCryptDConnectionString(this IEncryptionClients encryptionClients, IReadOnlyDictionary<string, object> extraOptions)
        {
            return (string)Reflector.Invoke(encryptionClients, nameof(CreateMongoCryptDConnectionString), extraOptions);
        }
    }
}
