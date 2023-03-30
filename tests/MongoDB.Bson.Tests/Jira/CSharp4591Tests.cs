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
using Xunit;

namespace MongoDB.Bson.Tests.Jira
{
    public class CSharp4591Tests
    {
        [Fact]
        public void Deserialize_C_should_work()
        {
            var json = "{ X : 1 }";
            var c = BsonSerializer.Deserialize<C>(json);
            c.X.Should().Be(1);
        }

        [Fact]
        public void Deserialize_D_should_work()
        {
            var json = "{ X : 1 }";
            var d = BsonSerializer.Deserialize<D>(json);
            d.X.Should().Be(1);
            d.Y.Should().Be(0); // because it was not present in the json
        }

        [Fact]
        public void Deserialize_D_with_Y_should_work()
        {
            var json = "{ X : 1, Y : 2 }";
            var d = BsonSerializer.Deserialize<D>(json);
            d.X.Should().Be(1);
            d.Y.Should().Be(2);
        }

#if NET6_0_OR_GREATER
        [Fact]
        public void Deserialize_E_should_work()
        {
            var json = "{ X : 1 }";
            var e = BsonSerializer.Deserialize<E>(json);
            e.X.Should().Be(1);
            e.Y.Should().Be(0); // because it was not present in the json
        }

        [Fact]
        public void Deserialize_E_with_Y_should_work()
        {
            var json = "{ X : 1, Y : 2 }";
            var e = BsonSerializer.Deserialize<E>(json);
            e.X.Should().Be(1);
            e.Y.Should().Be(2);
        }
#endif

        [Fact]
        public void Serialize_C_should_work()
        {
            var c = new C(1);
            var json = c.ToJson();
            json.Should().Be("{ \"X\" : 1 }");
        }

        [Fact]
        public void Serialize_D_should_work()
        {
            var d = new D(1);
            var json = d.ToJson();
            json.Should().Be("{ \"X\" : 1, \"Y\" : 0 }");
        }

        [Fact]
        public void Serialize_D_with_initializer_should_work()
        {
            var d = new D(1) { Y = 2 };
            var json = d.ToJson();
            json.Should().Be("{ \"X\" : 1, \"Y\" : 2 }");
        }

#if NET6_0_OR_GREATER
        [Fact]
        public void Serialize_E_should_work()
        {
            var e = new E(1);
            var json = e.ToJson();
            json.Should().Be("{ \"X\" : 1, \"Y\" : 0 }");
        }

        [Fact]
        public void Serialize_E_with_initializer_should_work()
        {
            var e = new E(1) { Y = 2 };
            var json = e.ToJson();
            json.Should().Be("{ \"X\" : 1, \"Y\" : 2 }");
        }
#endif

        private class C
        {
            public C(int x) { X = x; }
            public int X { get; }
        }

        private class D
        {
            public D(int x) { X = x; }
            public int X { get; }
            public int Y { get; set; }
        }

#if NET6_0_OR_GREATER
        private class E
        {
            public E(int x) { X = x; }
            public int X { get; }
            public int Y { get; init; }
        }
#endif
    }
}

