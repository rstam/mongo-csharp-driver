using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using MongoDB.Driver.MqlApi;
using Xunit;

namespace MongoDB.Driver.Tests.MqlApi.Examples.PracticalMongoDBAggregationsBook
{
    // https://www.practical-mongodb-aggregations.com/examples/foundational/group-and-total.html
    public class GroupAndTotal : MqlIntegrationTest
    {
        [Fact]
        public void Group_and_total_example_should_work()
        {
            var collection = GetCollection<Order>();

            var pipeline = Mql.Pipeline(collection)
                .Match(x => x.OrderDate >= new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc) && x.OrderDate < new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                .Sort(x => Mql.Ascending(x.OrderDate))
                .Group(
                    x => new
                    {
                        _id = x.CustomerId,
                        FirstPurchaseDate = Mql.FirstAccumulator(x.OrderDate),
                        TotalValue = Mql.SumAccumulator(x.Value),
                        TotalOrders = Mql.SumAccumulator(1),
                        Orders = Mql.PushAccumulator(new { x.OrderDate, x.Value })
                    })
                .Sort(x => Mql.Ascending(x.FirstPurchaseDate))
                .Set(x => new { CustomerId = x._id })
                .Unset(x => x["_id"])
                .As<Result>();
        }

        public class Order
        {
            public int CustomerId { get; set; }
            public DateTime OrderDate { get; set; }
            public decimal Value { get; set; }
        }

        public class Result
        {
            public int CustomerId { get; set; }
            public DateTime FirstPurchaseDate { get; set; }
            public Decimal TotalValue { get; set; }
            public long TotalOrders { get; set; }
            public OrderSummary[] orders { get; set; }
        }

        public class OrderSummary
        {
            public DateTime OrderDate { get; set; }
            public decimal Value { get; set; }
        }
    }
}
