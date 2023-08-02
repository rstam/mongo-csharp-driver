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
        public void ExpressionProjectionDefinition_with_field_projection_Render_should_work(
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
        public void ExpressionProjectionDefinition_with_field_projection_RenderForFind_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var projectionDefinition = new ExpressionProjectionDefinition<C, int>(x => x.Id, translationOptions: null);
            var serializerRegistry = BsonSerializer.SerializerRegistry;
            var sourceSerializer = serializerRegistry.GetSerializer<C>();

            var renderedProjection = projectionDefinition.RenderForFind(sourceSerializer, serializerRegistry, linqProvider);

            renderedProjection.Document.Should().Be("{ _id : 1 }");
        }

        private class C
        {
            public int Id { get; set; }
        }
    }
}
