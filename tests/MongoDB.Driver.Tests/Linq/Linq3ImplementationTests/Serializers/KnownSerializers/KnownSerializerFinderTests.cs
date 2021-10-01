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
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Linq.Linq3Implementation.Serializers.KnownSerializers;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3ImplementationTests.Serializers.KnownSerializers
{
    public class KnownSerializerFinderTests
    {
        private readonly IBsonDocumentSerializer _collectionSerializer = (IBsonDocumentSerializer)BsonSerializer.LookupSerializer<C>();

        #region class definitions for testing
        private enum E { A, B }

        private class C
        {
            public int P { get; set; }
            [BsonRepresentation(BsonType.Int32)]
            public E Ei { get; set; }
            [BsonRepresentation(BsonType.String)]
            public E Es { get; set; }
            public A A { get; set; }
        }

        private class A
        {
            public B B { get; set; }
        }

        private class B { }

        private class View
        {
            public A A { get; set; }
        }
        #endregion

        [Fact]
        public void Identity_expression_should_return_collection_serializer()
        {
            Expression<Func<C, C>> expression = x => x;

            var result = KnownSerializerFinder.FindKnownSerializers(expression, _collectionSerializer);

            var serializer = result.GetSerializer(expression.Body);
            serializer.Should().Be(_collectionSerializer);
            serializer.Should().BeOfType<BsonClassMapSerializer<C>>();
        }

        [Fact]
        public void Int_property_expression_should_return_int_serializer()
        {
            Expression<Func<C, int>> expression = x => x.P;

            var result = KnownSerializerFinder.FindKnownSerializers(expression, _collectionSerializer);

            var serializer = result.GetSerializer(expression.Body);
            _collectionSerializer.TryGetMemberSerializationInfo(nameof(C.P), out var expectedPropertySerializer);
            serializer.Should().Be(expectedPropertySerializer.Serializer);
            serializer.Should().BeOfType<Int32Serializer>();
        }

        [Fact]
        public void Enum_property_expression_should_return_enum_serializer_with_int_representation()
        {
            Expression<Func<C, E>> expression = x => x.Ei;

            var result = KnownSerializerFinder.FindKnownSerializers(expression, _collectionSerializer);

            var serializer = result.GetSerializer(expression.Body);
            _collectionSerializer.TryGetMemberSerializationInfo(nameof(C.Ei), out var expectedPropertySerializer);
            serializer.Should().Be(expectedPropertySerializer.Serializer);
            serializer.Should().BeOfType<EnumSerializer<E>>();
            var enumRepresentation = ((EnumSerializer<E>)serializer).Representation;
            enumRepresentation.Should().Be(BsonType.Int32);
        }

        [Fact]
        public void Enum_comparison_expression_should_return_enum_serializer_with_int_representation()
        {
            Expression<Func<C, bool>> expression = x => x.Ei == E.A;

            var result = KnownSerializerFinder.FindKnownSerializers(expression, _collectionSerializer);

            var equalsExpression = (BinaryExpression)expression.Body;
            var serializer = result.GetSerializer(equalsExpression.Right);
            _collectionSerializer.TryGetMemberSerializationInfo(nameof(C.Ei), out var expectedPropertySerializer);
            serializer.Should().Be(expectedPropertySerializer.Serializer);
            serializer.Should().BeOfType<EnumSerializer<E>>();
            var enumRepresentation = ((EnumSerializer<E>)serializer).Representation;
            enumRepresentation.Should().Be(BsonType.Int32);
        }

        [Fact]
        public void Enum_property_expression_should_return_enum_serializer_with_string_representation()
        {
            Expression<Func<C, E>> expression = x => x.Es;

            var result = KnownSerializerFinder.FindKnownSerializers(expression, _collectionSerializer);

            var serializer = result.GetSerializer(expression.Body);
            _collectionSerializer.TryGetMemberSerializationInfo(nameof(C.Es), out var expectedPropertySerializer);
            serializer.Should().Be(expectedPropertySerializer.Serializer);
            serializer.Should().BeOfType<EnumSerializer<E>>();
            var enumRepresentation = ((EnumSerializer<E>)serializer).Representation;
            enumRepresentation.Should().Be(BsonType.String);
        }

        [Fact]
        public void Enum_comparison_expression_should_return_enum_serializer_with_string_representation()
        {
            Expression<Func<C, bool>> expression = x => x.Es == E.A;

            var result = KnownSerializerFinder.FindKnownSerializers(expression, _collectionSerializer);

            var equalsExpression = (BinaryExpression)expression.Body;
            var serializer = result.GetSerializer(equalsExpression.Right);
            _collectionSerializer.TryGetMemberSerializationInfo(nameof(C.Es), out var expectedPropertySerializer);
            serializer.Should().Be(expectedPropertySerializer.Serializer);
            serializer.Should().BeOfType<EnumSerializer<E>>();
            var enumRepresentation = ((EnumSerializer<E>)serializer).Representation;
            enumRepresentation.Should().Be(BsonType.String);
        }

        [Fact]
        public void Conditional_expression_should_return_enum_serializer_with_int_representation()
        {
            Expression<Func<C, E>> expression = x => x.Ei == E.A ? E.B : x.Ei;

            var result = KnownSerializerFinder.FindKnownSerializers(expression, _collectionSerializer);

            var conditionalExpression = (ConditionalExpression)expression.Body;
            var serializer = result.GetSerializer(conditionalExpression.IfTrue);
            _collectionSerializer.TryGetMemberSerializationInfo(nameof(C.Ei), out var expectedPropertySerializer);
            serializer.Should().Be(expectedPropertySerializer.Serializer);
            serializer.Should().BeOfType<EnumSerializer<E>>();
            var enumRepresentation = ((EnumSerializer<E>)serializer).Representation;
            enumRepresentation.Should().Be(BsonType.Int32);
        }

        [Fact]
        public void Conditional_expression_should_return_enum_serializer_with_string_representation()
        {
            Expression<Func<C, E>> expression = x => x.Es == E.A ? E.B : x.Es;

            var result = KnownSerializerFinder.FindKnownSerializers(expression, _collectionSerializer);

            var conditionalExpression = (ConditionalExpression)expression.Body;
            var serializer = result.GetSerializer(conditionalExpression.IfTrue);
            _collectionSerializer.TryGetMemberSerializationInfo(nameof(C.Es), out var expectedPropertySerializer);
            serializer.Should().Be(expectedPropertySerializer.Serializer);
            serializer.Should().BeOfType<EnumSerializer<E>>();
            var enumRepresentation = ((EnumSerializer<E>)serializer).Representation;
            enumRepresentation.Should().Be(BsonType.String);
        }

        [Fact]
        public void Conditional_expression_with_different_enum_representations_should_throw()
        {
            Expression<Func<C, E>> expression = x => x.Ei == E.A ? E.B : x.Es;

            var result = KnownSerializerFinder.FindKnownSerializers(expression, _collectionSerializer);

            var conditionalExpression = (ConditionalExpression)expression.Body;
            Assert.Throws<InvalidOperationException>(() => result.GetSerializer(conditionalExpression.IfTrue));
        }

        [Fact]
        public void Property_chain_should_return_correct_nested_serializer()
        {
            Expression<Func<C, B>> expression = x => x.A.B;

            var result = KnownSerializerFinder.FindKnownSerializers(expression, _collectionSerializer);

            var serializer = result.GetSerializer(expression.Body);
            _collectionSerializer.TryGetMemberSerializationInfo(nameof(C.A), out var aSerializer);
            ((BsonClassMapSerializer<A>)aSerializer.Serializer).TryGetMemberSerializationInfo(nameof(A.B), out var expectedSerializer);
            serializer.Should().Be(expectedSerializer.Serializer);
        }

        [Fact]
        public void Two_property_chains_should_each_return_correct_nested_serializer()
        {
            Expression<Func<C, E>> expression = x => x.A.B == null ? x.Ei : x.Es;

            var result = KnownSerializerFinder.FindKnownSerializers(expression, _collectionSerializer);

            var conditionalExpression = (ConditionalExpression)expression.Body;
            var trueBranchSerializer = result.GetSerializer(conditionalExpression.IfTrue);
            _collectionSerializer.TryGetMemberSerializationInfo(nameof(C.Ei), out var eiSerializationInfo);
            trueBranchSerializer.Should().Be(eiSerializationInfo.Serializer);
            var falseBranchSerializer = result.GetSerializer(conditionalExpression.IfFalse);
            _collectionSerializer.TryGetMemberSerializationInfo(nameof(C.Es), out var esSerializationInfo);
            falseBranchSerializer.Should().Be(esSerializationInfo.Serializer);
        }

        [Fact]
        public void Projection_into_new_type_should_return_correct_serializer()
        {
            Expression<Func<C, View>> expression = x => new View { A = x.A };

            var result = KnownSerializerFinder.FindKnownSerializers(expression, _collectionSerializer);

            var serializer = result.GetSerializer(expression.Body);
            serializer.ValueType.Should().Be(typeof(View));
        }
    }
}
