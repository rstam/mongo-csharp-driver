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
using System.Linq;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Xunit;

namespace MongoDB.Bson.Tests.Serialization
{
    public class LazyBsonArraySerializerTests
    {
        public class C : IDisposable
        {
            public LazyBsonArray A;

            public void Dispose()
            {
                if (A != null)
                {
                    A.Dispose();
                    A = null;
                }
            }
        }

        [Fact]
        public void TestRoundTrip()
        {
            var bsonDocument = new BsonDocument { { "A", new BsonArray { 1, 2 } } };
            var bson = bsonDocument.ToBson();

            using (var c = BsonSerializer.Deserialize<C>(bson))
            {
                Assert.True(bson.SequenceEqual(c.ToBson()));
            }
        }

        [Fact]
        public void Equals_null_should_return_false()
        {
            var x = new LazyBsonArraySerializer();

            var result = x.Equals(null);

            result.Should().Be(false);
        }

        [Fact]
        public void Equals_object_should_return_false()
        {
            var x = new LazyBsonArraySerializer();
            var y = new object();

            var result = x.Equals(y);

            result.Should().Be(false);
        }

        [Fact]
        public void Equals_self_should_return_true()
        {
            var x = new LazyBsonArraySerializer();

            var result = x.Equals(x);

            result.Should().Be(true);
        }

        [Fact]
        public void Equals_with_equal_fields_should_return_true()
        {
            var x = new LazyBsonArraySerializer();
            var y = new LazyBsonArraySerializer();

            var result = x.Equals(y);

            result.Should().Be(true);
        }

        [Fact]
        public void GetHashCode_should_return_zero()
        {
            var x = new LazyBsonArraySerializer();

            var result = x.GetHashCode();

            result.Should().Be(0);
        }
    }
}
