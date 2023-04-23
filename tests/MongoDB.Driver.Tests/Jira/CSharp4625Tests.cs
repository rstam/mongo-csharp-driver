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

using FluentAssertions;
using MongoDB.Bson;
using Xunit;

namespace MongoDB.Driver.Tests.Jira.CSharp624
{
    public class CSharp4625Tests
    {
        [Fact]
        public void Find_should_work()
        {
            var client = DriverTestConfiguration.Client;
            var database = client.GetDatabase("teacher_profile");
            var collection = database.GetCollection<BsonDocument>("chapters");
            var filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse("64439214f877fcad42b31c38"));
            var result = collection.Find(filter).FirstOrDefault();
            result.Should().BeNull();
        }
    }
}
