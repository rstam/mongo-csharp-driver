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
using MongoDB.Driver.MqlBuilder;
using Xunit;

namespace MongoDB.Driver.Tests.MqlBuilder.Examples.PracticalMongoDBAggregationsBook
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
