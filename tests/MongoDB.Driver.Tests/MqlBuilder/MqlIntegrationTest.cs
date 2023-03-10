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

using System.Linq;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Optimizers;
using MongoDB.Driver.MqlBuilder;
using MongoDB.Driver.MqlBuilder.Translators.ExpressionToFilterTranslators;

namespace MongoDB.Driver.Tests.MqlBuilder
{
    public abstract class MqlIntegrationTest
    {
        public void AssertStages(BsonDocument[] stages, params string[] expectedStages)
        {
            stages.Should().Equal(expectedStages.Select(s => BsonDocument.Parse(s)));
        }

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

        public static BsonDocument[] TranslatePipeline<TInput, TOutput>(MqlPipeline<TInput, TOutput> pipeline)
        {
            var translatedPipeline = MqlPipelineTranslator.Translate(pipeline);
            var optimizedPipeline = AstPipelineOptimizer.Optimize(translatedPipeline);
            var renderedPipeline = optimizedPipeline.Render();
            return ((BsonArray)renderedPipeline).Cast<BsonDocument>().ToArray();
        }
    }
}
