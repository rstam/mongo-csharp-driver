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
using System.Globalization;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Linq;
using MongoDB.Driver.Linq.Linq3Implementation.Serializers;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3ImplementationTests.Jira
{
    public class CSharp4368Tests : Linq3IntegrationTest
    {
        [Fact]
        public void Convert_to_BsonValue_from_bool_should_work()
        {
            Convert_to_BsonValue_from_TValue_should_work<bool>(true, true, "true", "{ $toBool : '$V' }", x => (BsonValue)x.V);
        }

        [Theory]
        [InlineData(new byte[] { 1, 2, 3 })]
        [InlineData(null)]
        public void Convert_to_BsonValue_from_byte_array_should_work(byte[] value)
        {
            var expectedResult = value == null ? BsonNull.Value : (BsonValue)value;
            var valueAsJson = value == null ? "null" : (new BsonBinaryData(value, 0)).ToJson();
            var convertExpression = "{ $cond : { if : { $in : [{ $type : '$V' }, ['null', 'binData']] }, then : '$V', else : { $convert : { input : '$V', to : 'binData' } } } }";
            Convert_to_BsonValue_from_TValue_should_work<byte[]>(value, expectedResult, valueAsJson, convertExpression, x => (BsonValue)x.V);
        }

        [Fact]
        public void Convert_to_BsonValue_from_date_should_work()
        {
            var value = new DateTime(2021, 1, 2, 3, 4, 5, DateTimeKind.Utc);
            var expectedResult = (BsonValue)value;
            var valueAsJson = expectedResult.ToJson();
            Convert_to_BsonValue_from_TValue_should_work<DateTime>(value, expectedResult, valueAsJson, "{ $toDate : '$V' }", x => (BsonValue)x.V);
        }

        [Fact]
        public void Convert_to_BsonValue_from_decimal_should_work()
        {
            Convert_to_BsonValue_from_TValue_should_work<decimal>(1M, 1M, "'1'", "{ $toDecimal : '$V' }", x => (BsonValue)x.V); // note: default representation of decimal is string
        }

        [Fact]
        public void Convert_to_BsonValue_from_decimal128_should_work()
        {
            Convert_to_BsonValue_from_TValue_should_work<Decimal128>(1M, 1M, "'1'", "{ $toDecimal : '$V' }", x => (BsonValue)x.V); // note: default representation of Decimal128 is string
        }

        [Fact]
        public void Convert_to_BsonValue_from_double_should_work()
        {
            Convert_to_BsonValue_from_TValue_should_work<double>(1D, 1D, "1.0", "{ $toDouble : '$V' }", x => (BsonValue)x.V);
        }

        [Fact]
        public void Convert_to_BsonValue_from_int_should_work()
        {
            Convert_to_BsonValue_from_TValue_should_work<int>(1, 1, "1", "{ $toInt : '$V' }", x => (BsonValue)x.V);
        }

        [Fact]
        public void Convert_to_BsonValue_from_long_should_work()
        {
            Convert_to_BsonValue_from_TValue_should_work<long>(1L, 1L, "{ $numberLong : '1' }", "{ $toLong : '$V' }", x => (BsonValue)x.V);
        }

        [Theory]
        [InlineData(true, "true")]
        [InlineData(null, "null")]
        public void Convert_to_BsonValue_from_nullable_bool_should_work(bool? value, string valueAsJson)
        {
            Convert_to_BsonValue_from_TValue_should_work<bool?>(value, value, valueAsJson, "{ $toBool : '$V' }", x => (BsonValue)x.V);
        }

        [Theory]
        [InlineData("2021-01-02T03:04:05.123Z", "ISODate('2021-01-02T03:04:05.123')")]
        [InlineData(null, "null")]
        public void Convert_to_BsonValue_from_nullable_date_should_work(string valueAsString, string valueAsJson)
        {
            var value = valueAsString == null ? (DateTime?)null : DateTime.Parse(valueAsString, null, DateTimeStyles.AssumeUniversal);
            var expectedResult = value == null ? BsonNull.Value : (BsonValue)value;
            Convert_to_BsonValue_from_TValue_should_work<DateTime?>(value, value, valueAsJson, "{ $toDate : '$V' }", x => (BsonValue)x.V);
        }

        [Theory]
        [InlineData(1.0, "'1'")]
        [InlineData(null, "null")]
        public void Convert_to_BsonValue_from_nullable_decimal_should_work(double? valueAsDouble, string valueAsJson)
        {
            var value = (decimal?)valueAsDouble;
            Convert_to_BsonValue_from_TValue_should_work<decimal?>(value, value, valueAsJson, "{ $toDecimal : '$V' }", x => (BsonValue)x.V);
        }

        [Theory]
        [InlineData(1.0, "'1'")]
        [InlineData(null, "null")]
        public void Convert_to_BsonValue_from_nullable_decimal128_should_work(double? valueAsDouble, string valueAsJson)
        {
            var value = (Decimal128?)valueAsDouble;
            Convert_to_BsonValue_from_TValue_should_work<Decimal128?>(value, value, valueAsJson, "{ $toDecimal : '$V' }", x => (BsonValue)x.V);
        }

        [Theory]
        [InlineData(1.0, "1.0")]
        [InlineData(null, "null")]
        public void Convert_to_BsonValue_from_nullable_double_should_work(double? value, string valueAsJson)
        {
            Convert_to_BsonValue_from_TValue_should_work<double?>(value, value, valueAsJson, "{ $toDouble : '$V' }", x => (BsonValue)x.V);
        }

        [Theory]
        [InlineData(1, "1")]
        [InlineData(null, "null")]
        public void Convert_to_BsonValue_from_nullable_int_should_work(int? value, string valueAsJson)
        {
            Convert_to_BsonValue_from_TValue_should_work<int?>(value, value, valueAsJson, "{ $toInt : '$V' }", x => (BsonValue)x.V);
        }

        [Theory]
        [InlineData(1L, "{ $numberLong : '1' }")]
        [InlineData(null, "null")]
        public void Convert_to_BsonValue_from_nullable_long_should_work(long? value, string valueAsJson)
        {
            Convert_to_BsonValue_from_TValue_should_work<long?>(value, value, valueAsJson, "{ $toLong : '$V' }", x => (BsonValue)x.V);
        }

        [Theory]
        [InlineData("0102030405060708090a0b0c", "ObjectId('0102030405060708090a0b0c')")]
        [InlineData(null, "null")]
        public void Convert_to_BsonValue_from_nullable_ObjectId_should_work(string valueAsString, string valueAsJson)
        {
            var value = valueAsString == null ? (ObjectId?)null : ObjectId.Parse(valueAsString);
            Convert_to_BsonValue_from_TValue_should_work<ObjectId?>(value, value, valueAsJson, "{ $toObjectId : '$V' }", x => (BsonValue)x.V);
        }

        [Fact]
        public void Convert_to_BsonValue_from_ObjectId_should_work()
        {
            Convert_to_BsonValue_from_TValue_should_work<ObjectId>(ObjectId.Parse("0102030405060708090a0b0c"), ObjectId.Parse("0102030405060708090a0b0c"), "ObjectId('0102030405060708090a0b0c')", "{ $toObjectId : '$V' }", x => (BsonValue)x.V);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData(null)]
        public void Convert_to_BsonValue_from_Regex_should_work(string pattern)
        {
            var value = pattern == null ? null : new Regex(pattern, RegexOptions.IgnoreCase);
            var expectedResult = value == null ? BsonNull.Value : (BsonValue)value;
            var valueAsJson = expectedResult.ToJson();
            var convertExpression = "{ $cond : { if : { $in : [{ $type : '$V' }, ['null', 'regex']] }, then : '$V', else : { $convert : { input : '$V', to : 'regex' } } } }";
            Convert_to_BsonValue_from_TValue_should_work<Regex>(value, expectedResult, valueAsJson, convertExpression, x => (BsonValue)x.V);
        }

        [Theory]
        [InlineData("abc", "'abc'")]
        [InlineData(null, "null")]
        public void Convert_to_BsonValue_from_string_should_work(string value, string valueAsJson)
        {
            var expectedResult = value == null ? BsonNull.Value : (BsonValue)value;
            Convert_to_BsonValue_from_TValue_should_work<string>(value, expectedResult, valueAsJson, "{ $toString : '$V' }", x => (BsonValue)x.V);
        }

        private void Convert_to_BsonValue_from_TValue_should_work<TValue>(TValue value, BsonValue expectedResult, string valueAsJson, string convertExpression, Expression<Func<Document<TValue>, BsonValue>> projection)
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
                $"{{ $project : {{ _v : {convertExpression}, _id : 0 }} }}");

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

        private class Document<TValue>
        {
            public TValue V { get; set; }
        }
    }
}
