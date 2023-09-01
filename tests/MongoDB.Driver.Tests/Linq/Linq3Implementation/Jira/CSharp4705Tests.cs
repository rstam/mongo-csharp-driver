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
using FluentAssertions;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.Linq;
using MongoDB.TestHelpers.XunitExtensions;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3Implementation.Jira
{
#if NET6_0_OR_GREATER
    public class CSharp4705Tests : Linq3IntegrationTest
    {
        [Theory]
        [ParameterAttributeData]
        public void OrderBy_Count_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = GetCollection(linqProvider);
            var profileId = 1;
            var categories = new[] { "abc" };
            var filter = Builders<UnlocakableModel>.Filter.Eq(model => model.profileId, profileId);
            var options = new FindOptions<UnlocakableModel>
            {
                Projection = Builders<UnlocakableModel>.Projection.Expression(p => new UnlocakableModel
                {
                    profileId = p.profileId,
                    entries = (p.entries != null) ? new(p.entries.Where(kvp => categories.Contains(kvp.Key))) : new()
                })
            };

            if (linqProvider == LinqProvider.V2)
            {
                var projectionTranslation = TranslateFindProjection(collection, options.Projection);
                projectionTranslation.Should().Be("{ _id : 1, entries : 1 }");
            }
            else
            {
                var exception = Record.Exception(() => TranslateFindProjection(collection, options.Projection));
                exception.Should().BeOfType<ExpressionNotSupportedException>();
            }
        }

        private IMongoCollection<UnlocakableModel> GetCollection(LinqProvider linqProvider)
        {
            var collection = GetCollection<UnlocakableModel>("test", linqProvider);
            CreateCollection(collection);
            return collection;
        }

        private class UnlocakableModel
        {
            [BsonId]
            public int profileId { get; set; }
            public Dictionary<string, List<string>> entries { get; set; }
        }
    }
#endif
}
