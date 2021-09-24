using System.Linq;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Linq;
using MongoDB.Driver.Linq.Linq3Implementation;
using MongoDB.Driver.Linq.Linq3Implementation.Translators.ExpressionToExecutableQueryTranslators;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3ImplementationTests.Serializers.KnownSerializers
{
    public class KnownSerializersTests
    {
        private static readonly IMongoClient __client;
        private static readonly IMongoCollection<C1> __collection;
        private static readonly IMongoCollection<C2> __collection2;
        private static readonly IMongoCollection<C3> __collection3;
        private static readonly IMongoDatabase __database;

        static KnownSerializersTests()
        {
            __client = DriverTestConfiguration.Client;
            __database = __client.GetDatabase(DriverTestConfiguration.DatabaseNamespace.DatabaseName);
            __collection = __database.GetCollection<C1>(DriverTestConfiguration.CollectionNamespace.CollectionName);
            __collection2 = __database.GetCollection<C2>(DriverTestConfiguration.CollectionNamespace.CollectionName);

            BsonClassMap.RegisterClassMap<C3>(cm =>
            {
                cm.AutoMap();
                cm.MapMember(p => p.Es).SetSerializer(new EnumSerializer<E>(BsonType.String));
            });
            __collection3 = __database.GetCollection<C3>(DriverTestConfiguration.CollectionNamespace.CollectionName);
        }

        public enum E { A, B };

        class C1
        {
            public E Ei { get; set; }
            [BsonRepresentation(BsonType.String)]
            public E Es { get; set; }
        }

        class C2
        {
            public E Ei { get; set; }
            [BsonRepresentation(BsonType.String)]
            public E Es { get; set; }
        }

        class C3
        {
            public E Es { get; set; }
        }

        class Results
        {
            public bool Result { get; set; }
        }

        [Theory]
        [InlineData(E.A, "{ \"Result\" : { \"$eq\" : [ \"$Ei\", 0 ] }, \"_id\" : 0 }")]
        [InlineData(E.B, "{ \"Result\" : { \"$eq\" : [ \"$Ei\", 1 ] }, \"_id\" : 0 }")]
        public void Where_operator_equal_should_render_correctly(E value, string expectedProjection)
        {
            var subject = __collection.AsQueryable3();

            var queryable = subject.Select(x => new Results { Result = x.Ei == value });

            AssertProjection<C1,Results>(queryable, expectedProjection);
        }

        [Theory]
        [InlineData(E.A, "{ \"Result\" : { \"$eq\" : [ \"$Es\", \"A\" ] }, \"_id\" : 0 }")]
        [InlineData(E.B, "{ \"Result\" : { \"$eq\" : [ \"$Es\", \"B\" ] }, \"_id\" : 0 }")]
        public void Where_operator_equal_should_render_enum_as_string(E value, string expectedProjection)
        {
            var subject = __collection2.AsQueryable3();

            var queryable = subject.Select(x => new Results { Result = x.Es == value });

            AssertProjection<C2,Results>(queryable, expectedProjection);
        }

        [Theory]
        [InlineData(E.A, "{ \"Result\" : { \"$eq\" : [ \"$Es\", \"A\" ] }, \"_id\" : 0 }")]
        [InlineData(E.B, "{ \"Result\" : { \"$eq\" : [ \"$Es\", \"B\" ] }, \"_id\" : 0 }")]
        public void Where_operator_equal_should_render_enum_as_string_when_configured_with_class_map(E value, string expectedProjection)
        {
            var subject = __collection3.AsQueryable3();

            var queryable = subject.Select(x => new Results { Result = x.Es == value });

            AssertProjection<C3,Results>(queryable, expectedProjection);
        }

        [Theory]
        [InlineData(E.A, "{ \"Result\" : { \"$and\" : [{ \"$eq\" : [ \"$Ei\", 0 ] }, { \"$eq\" : [ \"$Es\", \"A\" ] }]}, \"_id\" : 0 }")]
        [InlineData(E.B, "{ \"Result\" : { \"$and\" : [{ \"$eq\" : [ \"$Ei\", 1 ] }, { \"$eq\" : [ \"$Es\", \"B\" ] }]}, \"_id\" : 0 }")]
        public void Where_operator_equal_should_render_string_enum_as_string_and_int32_enum_as_int32(E value, string expectedProjection)
        {
            var subject = __collection2.AsQueryable3();

            var queryable = subject.Select(x => new Results { Result = x.Ei == value && x.Es == value });

            AssertProjection<C2,Results>(queryable, expectedProjection);
        }

        // private methods
        private static void AssertProjection<T,TOutput>(IQueryable<TOutput> queryable, string expectedProjection)
        {
            var stages = Translate<T, TOutput>(queryable);
            stages.Should().HaveCount(1);
            stages[0].Should().Be($"{{ \"$project\" : {expectedProjection} }}");
        }

        private static BsonDocument[] Translate<T, TOutput>(IQueryable<TOutput> queryable)
        {
            var provider = (MongoQueryProvider<T>)queryable.Provider;
            var executableQuery = ExpressionToExecutableQueryTranslator.Translate<T, TOutput>(provider, queryable.Expression);
            return executableQuery.Pipeline.Stages.Select(s => (BsonDocument)s.Render()).ToArray();
        }
    }
}
