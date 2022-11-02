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
using System.Text.RegularExpressions;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.TestHelpers;
using MongoDB.Driver.Linq;
using MongoDB.Driver.Linq.Linq3Implementation.Serializers;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3ImplementationTests.Jira
{
    public class CSharp4368Tests : Linq3IntegrationTest
    {
        private static readonly IBsonSerializer<Guid> __guidSerializerWithStandardRepresentation;
        private static readonly IBsonSerializer<Guid?> __nullableGuidSerializerWithStandardRepresentation;

        static CSharp4368Tests()
        {
            __guidSerializerWithStandardRepresentation = new GuidSerializer(GuidRepresentation.Standard);
            __nullableGuidSerializerWithStandardRepresentation = new NullableSerializer<Guid>(__guidSerializerWithStandardRepresentation);

            var guidClassMap = BsonClassMap.RegisterClassMap<Document<Guid>>(
                cm =>
                {
                    cm.MapMember(x => x.V).SetSerializer(__guidSerializerWithStandardRepresentation);
                });

            var nullableGuidClassMap = BsonClassMap.RegisterClassMap<Document<Guid?>>(
                cm =>
                {
                    cm.MapMember(x => x.V).SetSerializer(__nullableGuidSerializerWithStandardRepresentation);
                });
        }

        public static TestCase[] __testCases = new TestCase[]
        {
//            CreateTestCase<Anything>(new Anything(), "{ X : 1 }", x => (BsonValue)(object)x.V),
//            CreateTestCase<Anything>(new Anything(), "null", x => (BsonValue)(object)x.V),
//            CreateTestCase<byte[]>(new byte[0], "BinData(0, 'AQID')'", x => (BsonValue)x.V),
//            CreateTestCase<byte[]>(new byte[0], "null", x => (BsonValue)x.V),
//            CreateTestCase<bool>(false, "true", x => (BsonValue)x.V),
//            CreateTestCase<bool?>(false, "true", x => (BsonValue)x.V),
//            CreateTestCase<bool?>(false, "null", x => (BsonValue)x.V),
//            CreateTestCase<DateTime>(new DateTime(), "ISODate('2021-01-02T03:04:05.123')", x => (BsonValue)x.V),
//            CreateTestCase<DateTime?>(new DateTime(), "ISODate('2021-01-02T03:04:05.123')", x => (BsonValue)x.V),
//            CreateTestCase<DateTime?>(new DateTime(), "null", x => (BsonValue)x.V),
//            CreateTestCase<decimal>(0.0M, "'1'", x => (BsonValue)x.V),
//            CreateTestCase<decimal?>(0.0M, "'1'", x => (BsonValue)x.V),
//            CreateTestCase<decimal?>(0.0M, "null", x => (BsonValue)x.V),
//            CreateTestCase<Decimal128>(0.0M, "'1'", x => (BsonValue)x.V),
//            CreateTestCase<Decimal128?>(0.0M, "'1'", x => (BsonValue)x.V),
//            CreateTestCase<Decimal128?>(0.0M, "null", x => (BsonValue)x.V),
//            CreateTestCase<double>(0.0, "{ $numberDouble : '1.0' }", x => (BsonValue)x.V),
//            CreateTestCase<double?>(0.0, "{ $numberDouble : '1.0' }", x => (BsonValue)x.V),
//            CreateTestCase<double?>(0.0, "null", x => (BsonValue)x.V),
//#pragma warning disable CS0618 // Type or member is obsolete
//            CreateTestCase<Guid>(Guid.Empty, "UUID('01020304-0506-0708-090a-0b0c0d0e0f10')", x => (BsonValue)x.V),
//            CreateTestCase<Guid?>(Guid.Empty, "UUID('01020304-0506-0708-090a-0b0c0d0e0f10')", x => (BsonValue)x.V),
//            CreateTestCase<Guid?>(Guid.Empty, "null", x => (BsonValue)x.V),
//#pragma warning restore CS0618 // Type or member is obsolete
//            CreateTestCase<Guid>(Guid.Empty, "UUID('01020304-0506-0708-090a-0b0c0d0e0f10')", x => (BsonValue)(object)x.V),
//            CreateTestCase<Guid?>(Guid.Empty, "UUID('01020304-0506-0708-090a-0b0c0d0e0f10')", x => (BsonValue)(object)x.V),
//            CreateTestCase<Guid?>(Guid.Empty, "null", x => (BsonValue)(object)x.V),
//            CreateTestCase<int>(0, "1", x => (BsonValue)x.V),
//            CreateTestCase<int?>(0, "1", x => (BsonValue)x.V),
//            CreateTestCase<int?>(0, "null", x => (BsonValue)x.V),
//            CreateTestCase<long>(0L, "{ $numberLong : '1' }", x => (BsonValue)x.V),
//            CreateTestCase<long?>(0L, "{ $numberlong : '1' }", x => (BsonValue)x.V),
//            CreateTestCase<long?>(0L, "null", x => (BsonValue)x.V),
//            CreateTestCase<ObjectId>(ObjectId.Empty, "ObjectId('0102030405060708090a0b0c')", x => (BsonValue)x.V),
//            CreateTestCase<ObjectId?>(ObjectId.Empty, "ObjectId('0102030405060708090a0b0c')", x => (BsonValue)x.V),
//            CreateTestCase<ObjectId?>(ObjectId.Empty, "null", x => (BsonValue)x.V),
//            CreateTestCase<Regex>(new Regex(""), "/abc/i", x => (BsonValue)x.V),
//            CreateTestCase<Regex>(new Regex(""), "null", x => (BsonValue)x.V),
//            CreateTestCase<string>("", "'abc'", x => (BsonValue)x.V),
            CreateTestCase<string>("", "null", x => (BsonValue)x.V)
        };

        public class TestCase
        {
            public object Prototype { get; set; }
            public string ValueAsJson { get; set; }
            public LambdaExpression Projection { get; set; }
        }

        public static TestCase CreateTestCase<TValue>(
            TValue prototype,
            string valueAsJson,
            Expression<Func<Document<TValue>, BsonValue>> projection)
        {
            return new TestCase { Prototype = prototype, ValueAsJson = valueAsJson, Projection = projection };
        }

        public static IEnumerable<object[]> Convert_to_BsonValue_from_TValue_should_work_MemberData()
        {
            for (var i = 0; i < __testCases.Length; i++)
            {
                var prototype = __testCases[i].Prototype;
                var valueAsJson = __testCases[i].ValueAsJson;
                var expectedResult = BsonSerializer.Deserialize<BsonValue>(valueAsJson);
                var projectionAsString = __testCases[i].Projection.ToString();
                yield return new object[] { prototype, i, valueAsJson, expectedResult, projectionAsString };
            }
        }

        [Theory]
        [MemberData(nameof(Convert_to_BsonValue_from_TValue_should_work_MemberData))]
        [ResetGuidModeAfterTest]
        public void Convert_to_BsonValue_from_TValue_should_work<TValue>(TValue prototype, int i, string valueAsJson, BsonValue expectedResult, string projectionAsString)
        {
            var value = BsonSerializer.Deserialize<TValue>(valueAsJson);
            var projection = (Expression<Func<Document<TValue>, BsonValue>>)__testCases[i].Projection;
            var database = GetDatabase();

            var aggregate = database
                .Aggregate()
                .Documents(new[] { new Document<TValue> { V = value } })
                .Project(projection);

            var stages = Translate(database, aggregate);
            AssertStages(
                stages,
                $"{{ $documents : [{{ V : {valueAsJson} }}] }}",
                "{ $project : { _v : '$V', _id : 0 } }");

            AssertValueSerializer(aggregate, BsonValueSerializer.Instance);

            var results = aggregate.ToList();
            results.Should().Equal(expectedResult);
        }

        private void AssertValueSerializer<TValue>(IAggregateFluent<TValue> aggregate, IBsonSerializer<TValue> expectedSerializer)
        {
            var pipeline = ((AggregateFluent<NoPipelineInput, TValue>)aggregate).Pipeline;
            var renderedPipeline = pipeline.Render(NoPipelineInputSerializer.Instance, BsonSerializer.SerializerRegistry, LinqProvider.V3);
            var wrappedValueSerializer = (WrappedValueSerializer<TValue>)renderedPipeline.OutputSerializer;
            wrappedValueSerializer.ValueSerializer.Should().Be(expectedSerializer);
        }

        public class Document<TValue>
        {
            public TValue V { get; set; }
        }

        public class Anything
        {
            public int X { get; set; }
        }
    }
}
