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
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.Linq;
using MongoDB.TestHelpers.XunitExtensions;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3Implementation.Jira
{
    public class CSharp4754Tests : Linq3IntegrationTest
    {
        [Fact]
        public void Update_Set_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = GetCollection(linqProvider);
            var builder = Builders<PlaceHeader>.Update;
            var prop = BuildPropAccessor<string>(nameof(PlaceHeader.Name));
            var update = builder.Set(prop, "str");

            var renderedUpdate = update.Render()
        }

        private IMongoCollection<PlaceHeader> GetCollection(LinqProvider linqProvider)
        {
            var collection = GetCollection<PlaceHeader>("test", linqProvider);
            CreateCollection(
                collection);
            return collection;
        }

        private static Expression<Func<PlaceHeader, T>> BuildPropAccessor<T>(string propName)
        {
            var param = Expression.Parameter(typeof(PlaceHeader));
            var body = Expression.ConvertChecked(
              Expression.Property(param, propName),
              typeof(T));
            var prop = Expression.Lambda<Func<PlaceHeader, T>>(body, param);
            return prop;
        }

        public class PlaceHeader
        {
            public string Name { get; set; }
            public int IntegerProperty { get; set; }
        }
    }
}
