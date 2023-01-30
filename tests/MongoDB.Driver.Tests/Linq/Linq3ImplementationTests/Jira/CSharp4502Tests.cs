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
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver.Linq;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3ImplementationTests.Jira
{
    public class CSharp4502Tests : Linq3IntegrationTest
    {
        [Fact]
        public async void AppendStage_with_null_resultSerializer_should_work()
        {
            var collection = CreateCollection();
            Expression<Func<MyEntry, bool>> filter = x => x.Id == 1;

            await MarkAllAsDeleted(collection, filter);
        }

        public async Task MarkAllAsDeleted(IMongoCollection<MyEntry> collection, Expression<Func<MyEntry, bool>> filter)
        {
            // Define changes to apply to the database.
            var changes = new List<Tuple<Expression<Func<MyEntry, object>>, object>>();
            changes.Add(new Tuple<Expression<Func<AddressGroupEntry, object>>, object>(entry => entry.Deleted, true));

            var mongoUpdateDefinition = ToMongoUpdateDefinition(changes);

            await collection.UpdateManyAsync(filter, mongoUpdateDefinition);
            // Result handling omitted
        }

        public static UpdateDefinition<TEntry> ToMongoUpdateDefinition<TEntry>(List<Tuple<Expression<Func<TEntry, object>>, object>> changes)
        {
            var updates = new List<UpdateDefinition<TEntry>>();
            foreach (var change in changes)
            {
                updates.Add(Builders<TEntry>.Update.Set(change.Item1 as dynamic, change.Item2 as dynamic));
            }
            return Builders<TEntry>.Update.Combine(updates);
        }

        private IMongoCollection<MyEntry> CreateCollection()
        {
            var collection = GetCollection<MyEntry>("entries");

            CreateCollection(
                collection,
                new MyEntry { Id = 1 },
                new MyEntry { Id = 2 });
           
            return collection;
        }

        public class MyEntry
        {
            public int Id { get; set; }
        }

        public class AddressGroupEntry
        {
            public bool Deleted { get; set; }
        }
    }
}
