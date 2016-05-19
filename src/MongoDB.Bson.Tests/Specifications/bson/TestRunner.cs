/* Copyright 2016 MongoDB Inc.
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace MongoDB.Bson.Specifications.bson
{
    [TestFixture]
    public class TestRunner
    {
        [TestCaseSource(typeof(TestCaseFactory), "GetTestCases")]
        public void RunTestDefinition(TestType testType, BsonDocument definition)
        {
            switch (testType)
            {
                case TestType.Valid:
                    RunValid(definition);
                    break;
                case TestType.ParseError:
                    RunParseError(definition);
                    break;
            }
        }

        private void RunValid(BsonDocument definition)
        {
            var lossy = definition.GetValue("lossy", false).ToBoolean();
            var B = BsonUtils.ParseHexString(((string)definition["bson"]).ToLowerInvariant());
            var E = ((string)definition["extjson"]).Replace(" ", "");

            var cB = B;
            if (definition.Contains("canonical_bson"))
            {
                cB = BsonUtils.ParseHexString(((string)definition["canonical_bson"]).ToLowerInvariant());
            }

            var cE = E;
            if (definition.Contains("canonical_extjson"))
            {
                cE = ((string)definition["canonical_extjson"]).Replace(" ", "");
            }

            EncodeBson(DecodeBson(B)).Should().Equal(cB, "B -> B");
            EncodeExtjson(DecodeBson(B)).Should().Be(cE, "B -> E");
            EncodeExtjson(DecodeExtjson(E)).Should().Be(cE, "E -> E");
            if (!lossy)
            {
                EncodeBson(DecodeExtjson(E)).Should().Equal(cB, "E -> B");
            }

            if (B != cB)
            {
                EncodeBson(DecodeBson(cB)).Should().Equal(cB, "(2) B -> B");
                EncodeExtjson(DecodeBson(cB)).Should().Be(cE, "(2) B -> E");
            }

            if (E != cE)
            {
                EncodeExtjson(DecodeExtjson(cE)).Should().Be(cE, "(2) E -> E");
                if (!lossy)
                {
                    EncodeBson(DecodeExtjson(cE)).Should().Equal(cB, "(2) E -> B");
                }
            }
        }

        private BsonDocument DecodeBson(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            using (var reader = new BsonBinaryReader(stream))
            {
                var context = BsonDeserializationContext.CreateRoot(reader);
                return BsonDocumentSerializer.Instance.Deserialize(context);
            }
        }

        private BsonDocument DecodeExtjson(string extjson)
        {
            return BsonDocument.Parse(extjson);
        }

        private byte[] EncodeBson(BsonDocument document)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BsonBinaryWriter(stream))
            {
                var context = BsonSerializationContext.CreateRoot(writer);
                BsonDocumentSerializer.Instance.Serialize(context, document);
                return stream.ToArray();
            }
        }

        private string EncodeExtjson(BsonDocument document)
        {
            return document.ToString().Replace(" ", "");
        }

        private void RunParseError(BsonDocument definition)
        {
            var subject = (string)definition["string"];
            var style = NumberStyles.Float & ~NumberStyles.AllowTrailingWhite;
            Decimal128 result;
            if (Decimal128.TryParse(subject, style, NumberFormatInfo.CurrentInfo, out result))
            {
                Assert.Fail($"{subject} should have resulted in a parse failure.");
            }
        }

        public enum TestType
        {
            Valid,
            ParseError,
            DecodeError
        }

        private static class TestCaseFactory
        {
            public static IEnumerable<ITestCaseData> GetTestCases()
            {
                const string prefix = "MongoDB.Bson.Tests.Specifications.bson.tests.";
                return Assembly
                    .GetExecutingAssembly()
                    .GetManifestResourceNames()
                    .Where(path => path.StartsWith(prefix) && path.EndsWith(".json"))
                    .SelectMany(path =>
                    {
                        var definition = ReadDefinition(path);
                        var fullName = path.Remove(0, prefix.Length);

                        IEnumerable<ITestCaseData> tests = Enumerable.Empty<ITestCaseData>();

                        if (definition.Contains("valid"))
                        {
                            tests = tests.Concat(GetTestCasesHelper(
                                TestType.Valid,
                                (string)definition["description"],
                                definition["valid"].AsBsonArray.Cast<BsonDocument>()));
                        }
                        if (definition.Contains("parseErrors"))
                        {
                            tests = tests.Concat(GetTestCasesHelper(
                            TestType.ParseError,
                            (string)definition["description"],
                            definition["parseErrors"].AsBsonArray.Cast<BsonDocument>()));
                        }
                        if (definition.Contains("decodeErrors"))
                        {
                            tests = tests.Concat(GetTestCasesHelper(
                                TestType.DecodeError,
                                (string)definition["description"],
                                definition["decodeErrors"].AsBsonArray.Cast<BsonDocument>()));
                        }
                        return tests;
                    });
            }

            private static IEnumerable<TestCaseData> GetTestCasesHelper(TestType type, string description, IEnumerable<BsonDocument> documents)
            {
                var dataList = new List<ITestCaseData>();
                var nameList = new Dictionary<string, int>();
                foreach (BsonDocument document in documents)
                {
                    var data = new TestCaseData(type, document);
                    data.SetCategory("Specifications");
                    data.SetCategory("bson");

                    var name = GetTestName(description, document);
                    int i = 0;
                    if (nameList.TryGetValue(name, out i))
                    {
                        nameList[name] = i + 1;
                        name += " #" + i;
                    }
                    else
                    {
                        nameList[name] = 1;
                    }

                    yield return data.SetName(name);
                }
            }

            private static string GetTestName(string description, BsonDocument definition)
            {
                var name = description;
                if (definition.Contains("description"))
                {
                    name += " - " + (string)definition["description"];
                }

                return name;
            }

            private static BsonDocument ReadDefinition(string path)
            {
                using (var definitionStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path))
                using (var definitionStringReader = new StreamReader(definitionStream))
                {
                    var definitionString = definitionStringReader.ReadToEnd();
                    return BsonDocument.Parse(definitionString);
                }
            }
        }
    }
}