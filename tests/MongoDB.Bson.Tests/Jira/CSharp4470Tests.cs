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
    public class CSharp4470Tests
    {
        static CSharp4470Tests()
        {
            var cm = new BsonClassMap<C>();
            cm.MapConstructor(typeof(C).GetConstructor(new[] { typeof(string) }), "S");
            cm.MapProperty(x => x.S);
            cm.SetIgnoreExtraElements(false); // if set to false still doesn't throw a NullReferenceException
            BsonClassMap.RegisterClassMap(cm);
        }

        [Fact]
        public void Deerialize_should_return_expected_result()
        {
            var document = BsonDocument.Parse("{ S : 's' }");
            var result = BsonSerializer.Deserialize<C>(document);
            result.S.Should().Be("s");
            result.T.Should().BeNull();
        }

        [Fact]
        public void Deerialize_with_unmapped_element_should_return_expected_result()
        {
            var document = BsonDocument.Parse("{ S : 's', T : 't' }"); // note that T will be ignored because IgnoreExtraElements was set to true
            var result = BsonSerializer.Deserialize<C>(document);
            result.S.Should().Be("s");
            result.T.Should().BeNull();
        }

        [Fact]
        public void Serialize_should_have_expected_result()
        {
            var c = new C("s") { T = "t" };
            var document = c.ToBsonDocument();
            document.Should().Be("{ S : 's' }"); // note that T is not in the serialized form because it was not mapped
        }

        public class C
        {
            public C(string s)
            {
                S = s;
            }

            public string S { get; set; } // mapped
            public string T { get; set; } // not mapped
        }
    }
}
