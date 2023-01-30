using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Driver.MqlApi;
using Xunit;

namespace MongoDB.Driver.Tests.MqlApi.Examples.JavaEngineeringProposal
{
    public class MqlGoogleDocExamples : MqlIntegrationTest
    {
        [Fact]
        public void Present_state_example()
        {
            var collection = CreateCollection();

            var pipeline = Mql.Pipeline(collection)
                .Project(
                    x =>
                        new
                        {
                            result =
                                x.NumList
                                .Filter(n => n % 2 == 0)
                                .Map(n => n * 10)
                                .Reduce(0, (a, i) => a + i)
                        });
        }

        [Fact]
        public void Switch_example()
        {
            var collection = CreateCollection();

            var pipeline = Mql.Pipeline(collection)
                .Project(
                    x =>
                        new
                        {
                            result =
                                Mql.Switch(
                                    Mql.Case(x.S == "F", "A"),
                                    Mql.Case(x.S == "T", "C"),
                                    Mql.Default("D"))
                        });
        }

        [Fact]
        public void Aggregations_example()
        {
            var collection = CreateCollection();

            var pipeline = Mql.Pipeline(collection)
                .Match(x => x.Id == "A")
                .AddFields(x => new { result = x.NumList.Filter(n => n % 2 == 0).Map(n => n * 10).Reduce(0, (a, b) => a + b) })
                .Unset(x => x["NumList"]); // AddFields could be enhanced to return a new POCO instead of a BsonDocument
        }

        private IMongoCollection<C> CreateCollection()
        {
            var collection = GetCollection<C>();
            return collection;
        }

        public class C
        {
            public string Id { get; set; }
            public List<int> NumList { get; set; }
            public string S { get; set; }
        }
    }
}
