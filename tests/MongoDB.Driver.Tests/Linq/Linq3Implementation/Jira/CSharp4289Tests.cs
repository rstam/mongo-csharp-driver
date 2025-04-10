﻿/* Copyright 2010-present MongoDB Inc.
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
using System.Linq;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.Core.TestHelpers.XunitExtensions;
using MongoDB.Driver.Linq;
using MongoDB.Driver.TestHelpers;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3Implementation.Jira
{
    public class CSharp4289Tests : LinqIntegrationTest<CSharp4289Tests.ClassFixture>
    {
        public CSharp4289Tests(ClassFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public void Select_using_anonymous_class_should_work()
        {
            var collection = Fixture.Collection;

            var queryable = collection.AsQueryable()
                .Select(x => new { V = x.Id, W = x.Id });

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { V : '$_id', W : '$_id', _id : 0 } }");

            var results = queryable.ToList();
            results.Select(r => r.V).Should().Equal("111111111111111111111111");
            results.Select(r => r.W).Should().Equal("111111111111111111111111");
        }

        [Fact]
        public void Select_using_named_class_should_work()
        {
            var collection = Fixture.Collection;

            var queryable = collection.AsQueryable()
                .Select(x => new R(x.Id) { W = x.Id });

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { v : '$_id', w : '$_id', _id : 0 } }");

            var results = queryable.ToList();
            results.Select(r => r.V).Should().Equal("111111111111111111111111");
            results.Select(r => r.W).Should().Equal("111111111111111111111111");
        }

        public class C
        {
            [BsonRepresentation(BsonType.ObjectId)]
            public string Id { get; set; }
        }

        public class R
        {
            public R(string v)
            {
                V = v;
            }

            [BsonElement("v")] public string V { get; set; }
            [BsonElement("w")] public string W { get; set; }
        }

        public sealed class ClassFixture : MongoCollectionFixture<C>
        {
            protected override IEnumerable<C> InitialData =>
            [
                new C { Id = "111111111111111111111111" }
            ];
        }
    }
}
