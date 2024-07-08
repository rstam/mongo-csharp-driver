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
using System;
using System.Linq;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.Linq;
using MongoDB.TestHelpers.XunitExtensions;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3Implementation.Jira
{
    public class CSharp30953Tests : Linq3IntegrationTest
    {
        [Theory]
        [ParameterAttributeData]
        public void Any_Key_equals_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = GetCollection(linqProvider);

            var findFluent = collection.Find(s => s.RelatedObjects.Any(o => o.Key == "Hello"));

            if (linqProvider == LinqProvider.V2)
            {
                var exception = Record.Exception(() => TranslateFindFilter(collection, findFluent));
                exception.Should().BeOfType<NotSupportedException>();
            }
            else
            {
                var translatedFilter = TranslateFindFilter(collection, findFluent);
                translatedFilter.Should().Be("{ }");

                var result = findFluent.ToList();
                result.Select(r => r.Id).Should().Equal(1);
            }
        }

        [Theory]
        [ParameterAttributeData]
        public void Any_Value_equals_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = GetCollection(linqProvider);

            var findFluent = collection.Find(s => s.RelatedObjects.Any(o => o.Value == 1));

            if (linqProvider == LinqProvider.V2)
            {
                var exception = Record.Exception(() => TranslateFindFilter(collection, findFluent));
                exception.Should().BeOfType<NotSupportedException>();
            }
            else
            {
                var translatedFilter = TranslateFindFilter(collection, findFluent);
                translatedFilter.Should().Be("{ }");

                var result = findFluent.ToList();
                result.Select(r => r.Id).Should().Equal(1);
            }
        }


        [Theory]
        [ParameterAttributeData]
        public void Keys_Any_equals_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = GetCollection(linqProvider);

            var findFluent = collection.Find(s => s.RelatedObjects.Keys.Any(o => o == "a"));

            if (linqProvider == LinqProvider.V2)
            {
                var exception = Record.Exception(() => TranslateFindFilter(collection, findFluent));
                exception.Should().BeOfType<ArgumentException>();
            }
            else
            {
                var translatedFilter = TranslateFindFilter(collection, findFluent);
                translatedFilter.Should().Be("{ }");

                var result = findFluent.ToList();
                result.Select(r => r.Id).Should().Equal(1);
            }
        }

        [Theory]
        [ParameterAttributeData]
        public void Values_Any_equals_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = GetCollection(linqProvider);

            var findFluent = collection.Find(s => s.RelatedObjects.Values.Any(o => o == 1));

            if (linqProvider == LinqProvider.V2)
            {
                var exception = Record.Exception(() => TranslateFindFilter(collection, findFluent));
                exception.Should().BeOfType<ArgumentException>();
            }
            else
            {
                var translatedFilter = TranslateFindFilter(collection, findFluent);
                translatedFilter.Should().Be("{ }");

                var result = findFluent.ToList();
                result.Select(r => r.Id).Should().Equal(1);
            }
        }

        private IMongoCollection<ClassWithDictionary> GetCollection(LinqProvider linqProvider)
        {
            var collection = GetCollection<ClassWithDictionary>("test", linqProvider);
            CreateCollection(
                collection,
                new ClassWithDictionary { Id = 1, RelatedObjects = new Dictionary<string, int> { { "a", 1 } } });
            return collection;
        }

        private class ClassWithDictionary
        {
            public int Id { get; set; }
            public IDictionary<string, int> RelatedObjects { get; set; }
        }
    }
}
