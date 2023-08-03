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
using FluentAssertions;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq;
using MongoDB.TestHelpers.XunitExtensions;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3Implementation.Jira
{
    public class CSharp4681Tests : Linq3IntegrationTest
    {
        [Theory]
        [ParameterAttributeData]
        public void Find_Project_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = GetCollection(linqProvider);

            var find = collection
                .Find("{}")
                .Project(x => x.Id);

            var projection = TranslateFindProjection(collection, find);
            projection.Should().Be("{ _id : 1 }");

            var result = find.Single();
            result.Should().Be(1);
        }

        [Theory]
        [ParameterAttributeData]
        public void FindSync_with_Builder_Expression_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = GetCollection(linqProvider);
            var projection = Builders<C>.Projection.Expression(x => x.Id);
            var options = new FindOptions<C, int> { Projection = projection };

            var result = collection.FindSync("{}", options).Single();

            result.Should().Be(1);
        }

        [Theory]
        [ParameterAttributeData]
        public void FindSync_with_new_ExpressionProjectionDefinition_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = GetCollection(linqProvider);
            var projection = new ExpressionProjectionDefinition<C, int>(x => x.Id, translationOptions: null); // note: ExpressionProjectionDefinition is internal
            var options = new FindOptions<C, int> { Projection = projection };

            var result = collection.FindSync("{}", options).Single();

            result.Should().Be(1);
        }

        [Theory]
        [ParameterAttributeData]
        public void FindSync_with_new_FindExpressionProjectionDefinition_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = GetCollection(linqProvider);
            var projection = new FindExpressionProjectionDefinition<C, int>(x => x.Id);
            var options = new FindOptions<C, int> { Projection = projection };

            var result = collection.FindSync("{}", options).Single();

            result.Should().Be(1);
        }

        [Theory]
        [ParameterAttributeData]
        public void FindOneAndDelete_with_Builder_Expression_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = GetCollection(linqProvider);
            var projection = Builders<C>.Projection.Expression(x => x.Id);
            var options = new FindOneAndDeleteOptions<C, int> { Projection = projection };

            var result = collection.FindOneAndDelete(x => x.Id == 1, options);

            result.Should().Be(1);
        }

        [Theory]
        [ParameterAttributeData]
        public void FindOneAndDelete_with_new_ExpressionProjectionDefinition_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = GetCollection(linqProvider);
            var projection = new ExpressionProjectionDefinition<C, int>(x => x.Id, translationOptions: null); // note: ExpressionProjectionDefinition is internal
            var options = new FindOneAndDeleteOptions<C, int> { Projection = projection };

            var result = collection.FindOneAndDelete(x => x.Id == 1, options);

            result.Should().Be(1);
        }

        [Theory]
        [ParameterAttributeData]
        public void FindOneAndDelete_with_new_FindExpressionProjectionDefinition_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = GetCollection(linqProvider);
            var projection = new FindExpressionProjectionDefinition<C, int>(x => x.Id);
            var options = new FindOneAndDeleteOptions<C, int> { Projection = projection };

            var result = collection.FindOneAndDelete(x => x.Id == 1, options);

            result.Should().Be(1);
        }

        [Theory]
        [ParameterAttributeData]
        public void FindOneAndReplace_with_Builder_Expression_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = GetCollection(linqProvider);
            var replacement = new C { Id = 1 };
            var projection = Builders<C>.Projection.Expression(x => x.Id);
            var options = new FindOneAndReplaceOptions<C, int> { Projection = projection };

            var result = collection.FindOneAndReplace(x => x.Id == 1, replacement, options);

            result.Should().Be(1);
        }

        [Theory]
        [ParameterAttributeData]
        public void FindOneAndReplace_with_new_ExpressionProjectionDefinition_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = GetCollection(linqProvider);
            var replacement = new C { Id = 1 };
            var projection = new ExpressionProjectionDefinition<C, int>(x => x.Id, translationOptions: null); // note: ExpressionProjectionDefinition is internal
            var options = new FindOneAndReplaceOptions<C, int> { Projection = projection };

            var result = collection.FindOneAndReplace(x => x.Id == 1, replacement, options);

            result.Should().Be(1);
        }

        [Theory]
        [ParameterAttributeData]
        public void FindOneAndReplace_with_new_FindExpressionProjectionDefinition_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = GetCollection(linqProvider);
            var replacement = new C { Id = 1 };
            var projection = new FindExpressionProjectionDefinition<C, int>(x => x.Id);
            var options = new FindOneAndReplaceOptions<C, int> { Projection = projection };

            var result = collection.FindOneAndReplace(x => x.Id == 1, replacement, options);

            result.Should().Be(1);
        }

        [Theory]
        [ParameterAttributeData]
        public void FindOneAndUpdate_with_Builder_Expression_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = GetCollection(linqProvider);
            var update = Builders<C>.Update.Set(x => x.Id, 1);
            var projection = Builders<C>.Projection.Expression(x => x.Id);
            var options = new FindOneAndUpdateOptions<C, int> { Projection = projection };

            var result = collection.FindOneAndUpdate(x => x.Id == 1, update, options);

            result.Should().Be(1);
        }

        [Theory]
        [ParameterAttributeData]
        public void FindOneAndUpdate_with_new_ExpressionProjectionDefinition_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = GetCollection(linqProvider);
            var update = Builders<C>.Update.Set(x => x.Id, 1);
            var projection = new ExpressionProjectionDefinition<C, int>(x => x.Id, translationOptions: null); // note: ExpressionProjectionDefinition is internal
            var options = new FindOneAndUpdateOptions<C, int> { Projection = projection };

            var result = collection.FindOneAndUpdate(x => x.Id == 1, update, options);

            result.Should().Be(1);
        }

        [Theory]
        [ParameterAttributeData]
        public void FindOneAndUpdate_with_new_FindExpressionProjectionDefinition_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = GetCollection(linqProvider);
            var update = Builders<C>.Update.Set(x => x.Id, 1);
            var projection = new FindExpressionProjectionDefinition<C, int>(x => x.Id);
            var options = new FindOneAndUpdateOptions<C, int> { Projection = projection };

            var result = collection.FindOneAndUpdate(x => x.Id == 1, update, options);

            result.Should().Be(1);
        }

        [Theory]
        [ParameterAttributeData]
        public void Aggregate_Project_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = GetCollection(linqProvider);

            var aggregate = collection
                .Aggregate()
                .Project(x => x.Id);

            var stages = Translate(collection, aggregate);
            if (linqProvider == LinqProvider.V2)
            {
                AssertStages(stages, "{ $project : { _id : 1 } }");
            }
            else
            {
                AssertStages(stages, "{ $project : { _v : '$_id', _id : 0 } }");
            }

            var result = aggregate.Single();
            result.Should().Be(1);
        }

        [Theory]
        [ParameterAttributeData]
        public void ExpressionProjectionDefinition_Render_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var projectionDefinition = new ExpressionProjectionDefinition<C, int>(x => x.Id, translationOptions: null);
            var serializerRegistry = BsonSerializer.SerializerRegistry;
            var sourceSerializer = serializerRegistry.GetSerializer<C>();

            var renderedProjection = projectionDefinition.Render(sourceSerializer, serializerRegistry, linqProvider);

            renderedProjection.Document.Should().Be("{ _v : '$_id', _id : 0 }");
        }

        [Theory]
        [ParameterAttributeData]
        public void ExpressionProjectionDefinition_RenderForFind_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var projectionDefinition = new ExpressionProjectionDefinition<C, int>(x => x.Id, translationOptions: null);
            var serializerRegistry = BsonSerializer.SerializerRegistry;
            var sourceSerializer = serializerRegistry.GetSerializer<C>();

            var renderedProjection = projectionDefinition.RenderForFind(sourceSerializer, serializerRegistry, linqProvider);

            renderedProjection.Document.Should().Be("{ _id : 1 }");
        }

        [Theory]
        [ParameterAttributeData]
        public void FindExpressionProjectionDefinition_Render_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var projectionDefinition = new FindExpressionProjectionDefinition<C, int>(x => x.Id);
            var serializerRegistry = BsonSerializer.SerializerRegistry;
            var sourceSerializer = serializerRegistry.GetSerializer<C>();

            var renderedProjection = projectionDefinition.Render(sourceSerializer, serializerRegistry, linqProvider);

            renderedProjection.Document.Should().Be("{ _id : 1 }");
        }

        [Theory]
        [ParameterAttributeData]
        public void FindExpressionProjectionDefinition_RenderForFind_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var projectionDefinition = new FindExpressionProjectionDefinition<C, int>(x => x.Id);
            var serializerRegistry = BsonSerializer.SerializerRegistry;
            var sourceSerializer = serializerRegistry.GetSerializer<C>();

            var renderedProjection = projectionDefinition.RenderForFind(sourceSerializer, serializerRegistry, linqProvider);

            renderedProjection.Document.Should().Be("{ _id : 1 }");
        }

        private IMongoCollection<C> GetCollection(LinqProvider linqProvider)
        {
            var collection = GetCollection<C>("test", linqProvider);
            CreateCollection(
                collection,
                new C { Id = 1 });
            return collection;
        }

        private class C
        {
            public int Id { get; set; }
        }
    }
}
