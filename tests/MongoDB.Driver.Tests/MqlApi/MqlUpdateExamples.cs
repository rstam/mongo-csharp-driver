using System;
using System.Linq;
using MongoDB.Driver.MqlApi;
using Xunit;

namespace MongoDB.Driver.Tests.MqlApi
{
    public class MqlUpdateExamples : MqlIntegrationTest
    {
        [Fact]
        public void Update_examples()
        {
            var collection = CreateCollection();
            MqlUpdate<C> update;

            // https://www.mongodb.com/docs/manual/reference/operator/update-field/
            update = Mql.Update(collection).CurrentDate(x => x.D); // { $currentDate : { D : true } }
            update = Mql.Update(collection).Inc(x => x.X, 1); // { $inc : { X : 1 } }
            update = Mql.Update(collection).Min(x => x.X, 1); // { $min : { X : 1 } }
            update = Mql.Update(collection).Max(x => x.X, 1); // { $max : { X : 1 } }
            update = Mql.Update(collection).Mul(x => x.X, 2); // { $mul : { X : 2 } }
            update = Mql.Update(collection).Rename("a", "b"); // { $rename : { a : 'b' } }
            update = Mql.Update(collection).Set(x => x.X, 1); // { $set : { X : 1 } }
            update = Mql.Update(collection).SetOnInsert(x => x.X, 1); // { $setOnInsert : { X : 1 } }
            update = Mql.Update(collection).UnsetField("a"); // { $unset : { a : '' } }

            // https://www.mongodb.com/docs/manual/reference/operator/update-array/
            update = Mql.Update(collection).AddToSet(x => x.A, 1); // { $addToSet : { A : 1 } }
            update = Mql.Update(collection).PopFirst(x => x.A); // { $pop : { A : -1 } }
            update = Mql.Update(collection).PopLast(x => x.A); // { $pop : { A : 1 } }
            update = Mql.Update(collection).Pull(x => x.A, 2); // { $pull : { A : 2 } }
            update = Mql.Update(collection).Pull(x => x.A, e => e >= 6); // { $pull : { A : { $gte : 6 } } }
            update = Mql.Update(collection).Push(x => x.A, 1); // { $push : { A : 1 } }
            update = Mql.Update(collection).PullAll(x => x.A, 1, 2, 3); // { $pullAll : { A : [1, 2, 3] } }

            // https://www.mongodb.com/docs/manual/reference/operator/update-bitwise/
            update = Mql.Update(collection).BitAnd(x => x.X, 1); // { $bit : { X : { and : 1 } } }
            update = Mql.Update(collection).BitOr(x => x.X, 1); // { $bit : { X : { or : 1 } } }
            update = Mql.Update(collection).BitXor(x => x.X, 1); // { $bit : { X : { xor : 1 } } }
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
            public DateTime D { get; set; }
            public string S { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
        }
    }
}
