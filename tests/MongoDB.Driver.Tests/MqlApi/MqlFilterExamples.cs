using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver.MqlApi;
using Xunit;

namespace MongoDB.Driver.Tests.MqlApi
{
    public class MqlFilterExamples : MqlIntegrationTest
    {
        [Fact]
        public void Filter_examples()
        {
            var collection = CreateCollection();
            MqlFilter<C> filter;

            // https://www.mongodb.com/docs/manual/reference/operator/query-comparison/
            filter = Mql.Filter(collection, x => x.X == 1); // { X : { $eq : 1 } }
            filter = Mql.Filter(collection, x => x.X > 1); // { X : { $gt : 1 } }
            filter = Mql.Filter(collection, x => x.X >= 1); // { X : { $gte : 1 } }
            filter = Mql.Filter(collection, x => x.X.In(1, 2, 3)); // { X : { $in : [1, 2, 3] } }
            filter = Mql.Filter(collection, x => x.X < 1); // { X : { $lt : 1 } }
            filter = Mql.Filter(collection, x => x.X <= 1); // { X : { $lte : 1 } }
            filter = Mql.Filter(collection, x => x.X != 1); // { X : { $ne : 1 } }
            filter = Mql.Filter(collection, x => x.X.Nin(1, 2, 3)); // { X : { $nin : [1, 2, 3] } }\

            // https://www.mongodb.com/docs/manual/reference/operator/query-logical/
            filter = Mql.Filter(collection, x => x.X == 1 && x.Y == 2); // { X : { $and : [{ X : { $eq : 1} }, { Y : { $eq : 2 } }] } }
            filter = Mql.Filter(collection, x => !(x.X == 1)); // { X : { $not : { $eq : 1 } } }
            filter = Mql.Filter(collection, x => Mql.Nor(x.X == 1, x.Y == 2)); // { X : { $nor : [{ X : { $eq : 1} }, { Y : { $eq : 2 } }] } }
            filter = Mql.Filter(collection, x => x.X == 1 || x.Y == 2); // { X : { $or : [{ X : { $eq : 1} }, { Y : { $eq : 2 } }] } }

            // https://www.mongodb.com/docs/manual/reference/operator/query-element/
            filter = Mql.Filter(collection, x => Mql.Exists(x.X)); // { X : { $exists : true } }
            filter = Mql.Filter(collection, x => Mql.NotExists(x.X)); // { X : { $exists : false } }
            filter = Mql.Filter(collection, x => Mql.Type(x.X, BsonType.Int32)); // { X : { $type : 'int' } }
            filter = Mql.Filter(collection, x => Mql.Type(x.X, BsonType.Int32, BsonType.Int64)); // { X : { $type : ['int', 'long'] } }

            // https://www.mongodb.com/docs/manual/reference/operator/query-evaluation/
            filter = Mql.Filter(collection, x => Mql.Expr(x.X == 1)); //{ X : { $expr : { $eq : ['$X', 1] } } }
            filter = Mql.Filter(collection, x => Mql.JsonSchema(new BsonDocument())); //{ X : { $jsonSchema : { } } }
            filter = Mql.Filter(collection, x => x.X % 2 == 1); // { X : { $mod : [2, 1] } }
            filter = Mql.Filter(collection, x => Mql.Regex(x.S, "pattern", "options")); // { S : { $regex : /pattern/options  } }
            filter = Mql.Filter(collection, x => Mql.Text("abc", new MqlTextArgs { Language = "english", CaseSensitive = false, DiacriticSensitive = false })); // { $text : { $search : 'abc', $language : 'english, $caseSensitive : false, $diacriticSensitive : false } }
            // $where is deprecated

            // https://www.mongodb.com/docs/manual/reference/operator/query-geospatial/
            // TODO: all

            // https://www.mongodb.com/docs/manual/reference/operator/query-array/
            filter = Mql.Filter(collection, x => x.A.All(1, 2, 3)); // { A : { $all : [1, 2, 3] } }
            filter = Mql.Filter(collection, x => x.A.ElemMatch(e => e >= 1 && e <= 2)); // { A : { $elemMatch : { $gt : 1, $lt : 2 } } }
            filter = Mql.Filter(collection, x => x.A.Size(2)); // { A : { $size : 2 } }

            // https://www.mongodb.com/docs/manual/reference/operator/query-bitwise/
            filter = Mql.Filter(collection, x => x.X.BitsAllClear(1)); // { X : { $bitsAllClear : 1 } }
            filter = Mql.Filter(collection, x => x.X.BitsAllSet(1)); // { X : { $bitsAllSet : 1 } }
            filter = Mql.Filter(collection, x => x.X.BitsAnyClear(1)); // { X : { $bitsAnyClear : 1 } }
            filter = Mql.Filter(collection, x => x.X.BitsAnySet(1)); // { X : { $bitsAnySet : 1 } }

            // https://www.mongodb.com/docs/manual/reference/operator/projection/
            // TODO: all

            // https://www.mongodb.com/docs/manual/reference/operator/query-miscellaneous/
            // TODO: all
        }

        private IMongoCollection<C> CreateCollection()
        {
            var collection = GetCollection<C>();

            CreateCollection(
                collection,
                new C { Id = 1, X = 1, Y = 1 },
                new C { Id = 2, X = 2, Y = 2 });

            return collection;
        }

        public class C
        {
            public int Id { get; set; }
            public int[] A { get; set; }
            public string S { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
        }
    }
}
