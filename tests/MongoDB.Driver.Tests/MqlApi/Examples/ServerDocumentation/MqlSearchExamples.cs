using System;
using System.Globalization;
using MongoDB.Driver.MqlApi;
using Xunit;

namespace MongoDB.Driver.Tests.MqlApi.Examples.ServerDocumentation
{
    public class MqlSearchExamples : MqlIntegrationTest
    {
        [Fact]
        public void Search_examples()
        {
            var collection = CreateCollection();

            // var stage = MqlStage.Search();
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
