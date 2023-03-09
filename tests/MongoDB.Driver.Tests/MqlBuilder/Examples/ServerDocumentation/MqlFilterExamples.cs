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

using System.Linq;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.MqlBuilder;
using Xunit;

namespace MongoDB.Driver.Tests.MqlBuilder.Examples.ServerDocumentation
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
            filter = Mql.Filter(collection, x => x.X.Nin(1, 2, 3)); // { X : { $nin : [1, 2, 3] } }

            // https://www.mongodb.com/docs/manual/reference/operator/query-logical/
            filter = Mql.Filter(collection, x => x.X == 1 && x.Y == 2); // { X : { $and : [{ X : { $eq : 1} }, { Y : { $eq : 2 } }] } }
            filter = Mql.Filter(collection, x => Mql.Nor(x.X == 1, x.Y == 2)); // { X : { $nor : [{ X : { $eq : 1} }, { Y : { $eq : 2 } }] } }
            filter = Mql.Filter(collection, x => !(x.X == 1)); // { X : { $not : { $eq : 1 } } }
            filter = Mql.Filter(collection, x => x.X == 1 || x.Y == 2); // { X : { $or : [{ X : { $eq : 1} }, { Y : { $eq : 2 } }] } }

            // https://www.mongodb.com/docs/manual/reference/operator/query-element/
            filter = Mql.Filter(collection, x => Mql.Exists(x.X)); // { X : { $exists : true } }
            filter = Mql.Filter(collection, x => Mql.NotExists(x.X)); // { X : { $exists : false } }
            filter = Mql.Filter(collection, x => Mql.Type(x.X, BsonType.Int32)); // { X : { $type : 'int' } }
            filter = Mql.Filter(collection, x => Mql.Type(x.X, BsonType.Int32, BsonType.Int64)); // { X : { $type : ['int', 'long'] } }

            // https://www.mongodb.com/docs/manual/reference/operator/query-evaluation/
            filter = Mql.Filter(collection, x => Mql.Expr(x.X == 1)); //{ $expr : { $eq : ['$X', 1] } }
            filter = Mql.Filter(collection, x => Mql.JsonSchema(new BsonDocument())); //{ $jsonSchema : { } }
            filter = Mql.Filter(collection, x => x.X % 2 == 1); // { X : { $mod : [2, 1] } }
            filter = Mql.Filter(collection, x => Mql.Regex(x.S, "pattern", "options")); // { S : { $regex : /pattern/options } }
            filter = Mql.Filter(collection, x => Mql.Text("abc", new MqlTextArgs { Language = "english", CaseSensitive = true, DiacriticSensitive = false })); // { $text : { $search : 'abc', $language : 'english, $caseSensitive : true, $diacriticSensitive : false } }
            // $where is deprecated

            // https://www.mongodb.com/docs/manual/reference/operator/query-geospatial/
            // TODO: all

            // https://www.mongodb.com/docs/manual/reference/operator/query-array/
            filter = Mql.Filter(collection, x => x.A.All(1, 2, 3)); // { A : { $all : [1, 2, 3] } }
            filter = Mql.Filter(collection, x => x.A.ElemMatch(e => e >= 1 && e <= 2)); // { A : { $elemMatch : { $gte : 1, $lte : 2 } } }
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

        [Fact]
        public void All_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/query-array/
            var collection = CreateCollection();
            Assert(
                Mql.Filter(collection, x => x.A.All(1, 2, 3)),
                "{ A : { $all : [1, 2, 3] } }");
        }

        [Fact]
        public void And_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/query-logical/
            var collection = CreateCollection();
            Assert(
                Mql.Filter(collection, x => x.X == 1 && x.Y == 2),
                "{ X : 1, Y : 2 }");
        }

        [Fact]
        public void BitsAllClear_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/query-bitwise/
            var collection = CreateCollection();
            Assert(
                Mql.Filter(collection, x => x.X.BitsAllClear(1)),
                "{ X : { $bitsAllClear : 1 } }");
        }

        [Fact]
        public void BitsAllSet_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/query-bitwise/
            var collection = CreateCollection();
            Assert(
                Mql.Filter(collection, x => x.X.BitsAllSet(1)),
                "{ X : { $bitsAllSet : 1 } }");
        }

        [Fact]
        public void BitsAnyClear_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/query-bitwise/
            var collection = CreateCollection();
            Assert(
                Mql.Filter(collection, x => x.X.BitsAnyClear(1)),
                "{ X : { $bitsAnyClear : 1 } }");
        }

        [Fact]
        public void BitsAnySet_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/query-bitwise/
            var collection = CreateCollection();
            Assert(
                Mql.Filter(collection, x => x.X.BitsAnySet(1)),
                "{ X : { $bitsAnySet : 1 } }");
        }

        [Fact]
        public void ElemMatch_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/query-array/
            var collection = CreateCollection();
            Assert(
                Mql.Filter(collection, x => x.A.ElemMatch(e => e >= 1 && e <= 2)),
                "{ A : { $elemMatch : { $gte : 1, $lte : 2 } } }");
        }

        [Fact]
        public void Eq_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/query-comparison/
            var collection = CreateCollection();
            Assert(
                Mql.Filter(collection, x => x.X == 1),
                "{ X : 1 }");
        }

        [Fact]
        public void Exists_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/query-element/
            var collection = CreateCollection();
            Assert(
                Mql.Filter(collection, x => Mql.Exists(x.X)),
                "{ X : { $exists : true } }");
        }

        [Fact]
        public void Expr_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/query-evaluation/
            var collection = CreateCollection();
            Assert(
                Mql.Filter(collection, x => Mql.Expr(x.X == 1)),
                "{ $expr : { $eq : ['$X', 1] } }");
        }

        [Fact]
        public void Gt_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/query-comparison/
            var collection = CreateCollection();
            Assert(
                Mql.Filter(collection, x => x.X > 1),
                "{ X : { $gt : 1 } }");
        }

        [Fact]
        public void Gte_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/query-comparison/
            var collection = CreateCollection();
            Assert(
                Mql.Filter(collection, x => x.X >= 1),
                "{ X : { $gte : 1 } }");
        }

        [Fact]
        public void In_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/query-comparison/
            var collection = CreateCollection();
            Assert(
                Mql.Filter(collection, x => x.X.In(1, 2, 3)),
                "{ X: { $in : [1, 2, 3] } }");
        }

        [Fact]
        public void JsonSchema_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/query-evaluation/
            var collection = CreateCollection();
            var schema = new BsonDocument("X", 1);
            Assert(
                Mql.Filter(collection, x => Mql.JsonSchema(schema)),
                "{ $jsonSchema :  { X : 1 } }");
        }

        [Fact]
        public void Lt_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/query-comparison/
            var collection = CreateCollection();
            Assert(
                Mql.Filter(collection, x => x.X < 1),
                "{ X : { $lt : 1 } }");
        }

        [Fact]
        public void Lte_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/query-comparison/
            var collection = CreateCollection();
            Assert(
                Mql.Filter(collection, x => x.X <= 1),
                "{ X : { $lte : 1 } }");
        }

        [Fact]
        public void Mod_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/query-evaluation/
            var collection = CreateCollection();
            Assert(
                Mql.Filter(collection, x => x.X % 2 == 1),
                "{ X : { $mod : [2, 1] } }");
        }

        [Fact]
        public void Ne_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/query-comparison/
            var collection = CreateCollection();
            Assert(
                Mql.Filter(collection, x => x.X != 1),
                "{ X : { $ne : 1 } }");
        }

        [Fact]
        public void Nin_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/query-comparison/
            var collection = CreateCollection();
            Assert(
                Mql.Filter(collection, x => x.X.Nin(1, 2, 3)),
                "{ X: { $nin : [1, 2, 3] } }");
        }

        [Fact]
        public void Nor_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/query-logical/
            var collection = CreateCollection();
            Assert(
                Mql.Filter(collection, x => Mql.Nor(x.X == 1, x.Y == 2)),
                "{ $nor : [{ X : 1 }, { Y : 2 }] }");
        }

        [Fact]
        public void Not_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/query-logical/
            var collection = CreateCollection();
            Assert(
                Mql.Filter(collection, x => !(x.X == 1)),
                "{ X : { $ne : 1 } }");
        }

        [Fact]
        public void NotExists_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/query-element/
            var collection = CreateCollection();
            Assert(
                Mql.Filter(collection, x => Mql.NotExists(x.X)),
                "{ X : { $exists : false } }");
        }

        [Fact]
        public void Or_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/query-logical/
            var collection = CreateCollection();
            Assert(
                Mql.Filter(collection, x => x.X == 1 || x.Y == 2),
                "{ $or : [{ X : 1 }, { Y : 2 }] }");
        }

        [Fact]
        public void Regex_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/query-evaluation/
            var collection = CreateCollection();
            Assert(
                Mql.Filter(collection, x => Mql.Regex(x.S, "pattern", "is")),
                "{ S : /pattern/is }");
        }

        [Fact]
        public void Size_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/query-array/
            var collection = CreateCollection();
            Assert(
                Mql.Filter(collection, x => x.A.Size(2)),
                "{ A : { $size : 2 } }");
        }

        [Fact]
        public void Text_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/query-evaluation/
            var collection = CreateCollection();
            Assert(
                Mql.Filter(collection, x => Mql.Text("abc", new MqlTextArgs { Language = "english", CaseSensitive = true, DiacriticSensitive = false })),
                "{ $text : { $search : 'abc', $language : 'english', $caseSensitive : true, $diacriticSensitive : false } }");
        }

        [Fact]
        public void Type_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/query-element/
            var collection = CreateCollection();
            Assert(
                Mql.Filter(collection, x => Mql.Type(x.X, BsonType.Int32)),
                "{ X : { $type : 'int' } }");
        }

        [Fact]
        public void Type_with_array_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/query-element/
            var collection = CreateCollection();
            Assert(
                Mql.Filter(collection, x => Mql.Type(x.X, BsonType.Int32, BsonType.Int64)),
                "{ X : { $type : ['int', 'long'] } }");
        }

        private void Assert<TDocument>(MqlFilter<TDocument> filter, string expectedTranslation)
        {
            var translatedFilter = TranslateFilter(filter);
            translatedFilter.Should().Be(expectedTranslation);
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
