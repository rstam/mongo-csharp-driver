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

using MongoDB.Bson;
using MongoDB.Driver.MqlApi;
using MongoDB.Driver.MqlApi.Translators.FilterTranslators;

namespace MongoDB.Driver.Tests.MqlApi
{
    public abstract class MqlIntegrationTest
    {
        public void CreateCollection<TDocument>(
            IMongoCollection<TDocument> collection,
            params TDocument[] documents)
        {
            collection.Database.DropCollection(collection.CollectionNamespace.CollectionName);
            collection.InsertMany(documents);
        }

        public IMongoCollection<TDocument> GetCollection<TDocument>()
        {
            var client = DriverTestConfiguration.Client;
            var database = client.GetDatabase(DriverTestConfiguration.DatabaseNamespace.DatabaseName);
            return database.GetCollection<TDocument>(DriverTestConfiguration.CollectionNamespace.CollectionName);
        }

        public BsonDocument TranslateFilter<TDocument>(MqlFilter<TDocument> filter)
        {
            var translatedFilter = MqlFilterTranslator.Translate(filter);
            return (BsonDocument)translatedFilter.Render();
        }
    }
}
