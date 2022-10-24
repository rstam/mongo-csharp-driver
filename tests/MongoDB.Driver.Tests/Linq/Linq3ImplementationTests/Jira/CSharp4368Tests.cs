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

        [Theory]
        [InlineData("{ X : 1 }")]
        [InlineData("null")]
        public void Convert_to_BsonValue_from_anything_using_double_cast_should_work(string valueAsJson)
        {
            var value = BsonSerializer.Deserialize<Anything>(valueAsJson);
            var expectedResult = BsonSerializer.Deserialize<BsonValue>(valueAsJson);
            Convert_to_BsonValue_from_TValue_should_work<Anything>(value, valueAsJson, expectedResult, x => (BsonValue)(object)x.V);
        }

        [Fact]
        public void Convert_to_BsonValue_from_bool_should_work()
        {
            Convert_to_BsonValue_from_TValue_should_work<bool>(true, "true", true, x => (BsonValue)x.V);
        }

        [Theory]
        [InlineData("BinData(0, 'AQID')")]
        [InlineData("null")]
        public void Convert_to_BsonValue_from_byte_array_should_work(string valueAsJson)
        {
            var value = BsonSerializer.Deserialize<byte[]>(valueAsJson);
            var expectedResult = BsonSerializer.Deserialize<BsonValue>(valueAsJson);
            Convert_to_BsonValue_from_TValue_should_work<byte[]>(value, valueAsJson, expectedResult, x => (BsonValue)x.V);
        }

        [Fact]
        public void Convert_to_BsonValue_from_date_should_work()
        {
            var valueAsJson = "ISODate('2021-01-02T03:04:05.123Z')";
            var value = BsonSerializer.Deserialize<DateTime>(valueAsJson);
            var expectedResult = BsonSerializer.Deserialize<BsonValue>(valueAsJson);
            Convert_to_BsonValue_from_TValue_should_work<DateTime>(value, valueAsJson, expectedResult, x => (BsonValue)x.V);
        }

        [Fact]
        public void Convert_to_BsonValue_from_decimal_should_work()
        {
            Convert_to_BsonValue_from_TValue_should_work<decimal>(1M, "'1'", "1", x => (BsonValue)x.V); // note: default representation of decimal is string
        }

        [Fact]
        public void Convert_to_BsonValue_from_decimal128_should_work()
        {
            Convert_to_BsonValue_from_TValue_should_work<Decimal128>(1M, "'1'", "1", x => (BsonValue)x.V); // note: default representation of Decimal128 is string
        }

        [Fact]
        public void Convert_to_BsonValue_from_double_should_work()
        {
            Convert_to_BsonValue_from_TValue_should_work<double>(1D, "1.0", 1D, x => (BsonValue)x.V);
        }

        [Fact]
        [ResetGuidModeAfterTest]
        public void Convert_to_BsonValue_from_Guid_should_work()
        {
            GuidMode.Set(GuidRepresentationMode.V3);

            var valueAsJson = "HexData(4, '0102030405060708090a0b0c0d0e0f10')";
            var value = Deserialize(__guidSerializerWithStandardRepresentation, valueAsJson);
            var expectedResult = BsonSerializer.Deserialize<BsonValue>(valueAsJson);
#pragma warning disable CS0618 // Type or member is obsolete
            Convert_to_BsonValue_from_TValue_should_work<Guid>(value, valueAsJson, expectedResult, x => (BsonValue)x.V);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        [Fact]
        [ResetGuidModeAfterTest]
        public void Convert_to_BsonValue_from_Guid_using_double_cast_should_work()
        {
            GuidMode.Set(GuidRepresentationMode.V3);

            var valueAsJson = "HexData(4, '0102030405060708090a0b0c0d0e0f10')";
            var value = Deserialize(__guidSerializerWithStandardRepresentation, valueAsJson);
            var expectedResult = BsonSerializer.Deserialize<BsonValue>(valueAsJson);
            Convert_to_BsonValue_from_TValue_should_work<Guid>(value, valueAsJson, expectedResult, x => (BsonValue)(object)x.V);
        }

        [Fact]
        public void Convert_to_BsonValue_from_int_should_work()
        {
            Convert_to_BsonValue_from_TValue_should_work<int>(1, "1", 1, x => (BsonValue)x.V);
        }

        [Fact]
        public void Convert_to_BsonValue_from_long_should_work()
        {
            Convert_to_BsonValue_from_TValue_should_work<long>(1L, "{ $numberLong : '1' }", 1L, x => (BsonValue)x.V);
        }

        [Theory]
        [InlineData(true, "true")]
        [InlineData(null, "null")]
        public void Convert_to_BsonValue_from_nullable_bool_should_work(bool? value, string valueAsJson)
        {
            Convert_to_BsonValue_from_TValue_should_work<bool?>(value, valueAsJson, (BsonValue)value, x => (BsonValue)x.V);
        }

        [Theory]
        [InlineData("ISODate('2021-01-02T03:04:05.123')")]
        [InlineData("null")]
        public void Convert_to_BsonValue_from_nullable_date_should_work(string valueAsJson)
        {
            var value = BsonSerializer.Deserialize<DateTime?>(valueAsJson);
            var expectedResult = BsonSerializer.Deserialize<BsonValue>(valueAsJson);
            Convert_to_BsonValue_from_TValue_should_work<DateTime?>(value, valueAsJson, (BsonValue)value, x => (BsonValue)x.V);
        }

        [Theory]
        [InlineData("'1'")]
        [InlineData("null")]
        public void Convert_to_BsonValue_from_nullable_decimal_should_work(string valueAsJson)
        {
            var value = BsonSerializer.Deserialize<decimal?>(valueAsJson);
            var expectedResult = BsonSerializer.Deserialize<BsonValue>(valueAsJson);
            Convert_to_BsonValue_from_TValue_should_work<decimal?>(value, valueAsJson, expectedResult, x => (BsonValue)x.V);
        }

        [Theory]
        [InlineData("'1'")]
        [InlineData("null")]
        public void Convert_to_BsonValue_from_nullable_decimal128_should_work(string valueAsJson)
        {
            var value = BsonSerializer.Deserialize<Decimal128?>(valueAsJson);
            var expectedResult = BsonSerializer.Deserialize<BsonValue>(valueAsJson);
            Convert_to_BsonValue_from_TValue_should_work<Decimal128?>(value, valueAsJson, expectedResult, x => (BsonValue)x.V);
        }

        [Theory]
        [InlineData("1.0")]
        [InlineData("null")]
        public void Convert_to_BsonValue_from_nullable_double_should_work(string valueAsJson)
        {
            var value = BsonSerializer.Deserialize<double?>(valueAsJson);
            var expectedResult = BsonSerializer.Deserialize<BsonValue>(valueAsJson);
            Convert_to_BsonValue_from_TValue_should_work<double?>(value, valueAsJson, (BsonValue)value, x => (BsonValue)x.V);
        }

        [Theory]
        [InlineData("UUID('01020304-0506-0708-090a-0b0c0d0e0f10')")]
        [InlineData("null")]
        [ResetGuidModeAfterTest]
        public void Convert_to_BsonValue_from_nullable_Guid_should_work(string valueAsJson)
        {
            GuidMode.Set(GuidRepresentationMode.V3);

            var value = Deserialize(__nullableGuidSerializerWithStandardRepresentation, valueAsJson);
            var expectedResult = BsonSerializer.Deserialize<BsonValue>(valueAsJson);
#pragma warning disable CS0618 // Type or member is obsolete
            Convert_to_BsonValue_from_TValue_should_work<Guid?>(value, valueAsJson, expectedResult, x => (BsonValue)x.V);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        [Theory]
        [InlineData("UUID('01020304-0506-0708-090a-0b0c0d0e0f10')")]
        [InlineData("null")]
        [ResetGuidModeAfterTest]
        public void Convert_to_BsonValue_from_nullable_Guid_using_double_cast_should_work(string valueAsJson)
        {
            GuidMode.Set(GuidRepresentationMode.V3);

            var value = Deserialize(__nullableGuidSerializerWithStandardRepresentation, valueAsJson);
            var expectedResult = BsonSerializer.Deserialize<BsonValue>(valueAsJson);
            Convert_to_BsonValue_from_TValue_should_work<Guid?>(value, valueAsJson, expectedResult, x => (BsonValue)(object)x.V);
        }

        [Theory]
        [InlineData("1")]
        [InlineData("null")]
        public void Convert_to_BsonValue_from_nullable_int_should_work(string valueAsJson)
        {
            var value = BsonSerializer.Deserialize<int?>(valueAsJson);
            var expectedResult = BsonSerializer.Deserialize<BsonValue>(valueAsJson);
            Convert_to_BsonValue_from_TValue_should_work<int?>(value, valueAsJson, expectedResult, x => (BsonValue)x.V);
        }

        [Theory]
        [InlineData("{ $numberLong : '1' }")]
        [InlineData("null")]
        public void Convert_to_BsonValue_from_nullable_long_should_work(string valueAsJson)
        {
            var value = BsonSerializer.Deserialize<long?>(valueAsJson);
            var expectedResult = BsonSerializer.Deserialize<BsonValue>(valueAsJson);
            Convert_to_BsonValue_from_TValue_should_work<long?>(value, valueAsJson, expectedResult, x => (BsonValue)x.V);
        }

        [Theory]
        [InlineData("ObjectId('0102030405060708090a0b0c')")]
        [InlineData("null")]
        public void Convert_to_BsonValue_from_nullable_ObjectId_should_work(string valueAsJson)
        {
            var value = BsonSerializer.Deserialize<ObjectId?>(valueAsJson);
            var expectedResult = BsonSerializer.Deserialize<BsonValue>(valueAsJson);
            Convert_to_BsonValue_from_TValue_should_work<ObjectId?>(value, valueAsJson, expectedResult, x => (BsonValue)x.V);
        }

        [Fact]
        public void Convert_to_BsonValue_from_ObjectId_should_work()
        {
            var valueAsJson = "ObjectId('0102030405060708090a0b0c')";
            var value = BsonSerializer.Deserialize<ObjectId>(valueAsJson);
            var expectedResult = BsonSerializer.Deserialize<BsonValue>(valueAsJson);
            Convert_to_BsonValue_from_TValue_should_work<ObjectId>(value, valueAsJson, expectedResult, x => (BsonValue)x.V);
        }

        [Theory]
        [InlineData("/abc/i")]
        [InlineData("null")]
        public void Convert_to_BsonValue_from_Regex_should_work(string valueAsJson)
        {
            var value = BsonSerializer.Deserialize<Regex>(valueAsJson);
            var expectedResult = BsonSerializer.Deserialize<BsonValue>(valueAsJson);
            Convert_to_BsonValue_from_TValue_should_work<Regex>(value, valueAsJson, expectedResult, x => (BsonValue)x.V);
        }

        [Theory]
        [InlineData("'abc'")]
        [InlineData("null")]
        public void Convert_to_BsonValue_from_string_should_work(string valueAsJson)
        {
            var value = BsonSerializer.Deserialize<string>(valueAsJson);
            var expectedResult = BsonSerializer.Deserialize<BsonValue>(valueAsJson);
            Convert_to_BsonValue_from_TValue_should_work<string>(value, valueAsJson, expectedResult, x => (BsonValue)x.V);
        }

        private void Convert_to_BsonValue_from_TValue_should_work<TValue>(TValue value, string valueAsJson, BsonValue expectedResult, Expression<Func<Document<TValue>, BsonValue>> projection)
        {
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

        private TValue Deserialize<TValue>(IBsonSerializer<TValue> serializer, string json)
        {
            using (var reader = new JsonReader(json))
            {
                var context = BsonDeserializationContext.CreateRoot(reader);
                return serializer.Deserialize(context);
            }
        }

        private class Document<TValue>
        {
            public TValue V { get; set; }
        }

        private class Anything
        {
            public int X { get; set; }
        }
    }
}
