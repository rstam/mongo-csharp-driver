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

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.MqlApi;
using Xunit;

namespace MongoDB.Driver.Tests.MqlApi.Examples.PracticalMongoDBAggregationsBook
{
    // https://www.practical-mongodb-aggregations.com/examples/foundational/unpack-array-group-differently.html
    public class UnpackArraysAndGroupDifferently : MqlIntegrationTest
    {
        [Fact]
        public void Unpack_arrays_and_group_differently_example_should_work()
        {
            var collection = GetCollection<Order>();

            var pipeline = Mql.Pipeline(collection)
                .Unwind(x => x.Products, prototype: (OrderUnwound)null)
                .Match(x => x.Product.Price > 15.00M)
                .Group(x => new
                {
                    _id = x.Product.ProductId,
                    product = Mql.FirstAccumulator(x.Product.Name),
                    total_value = Mql.SumAccumulator(x.Product.Price),
                    quantity = Mql.SumAccumulator(1)
                })
                .Set(x => new { product_id = x._id })
                .Unset(x => x["_id"])
                .As<Result>();
        }

        public class Order
        {
            [BsonElement("order_id")] public long OrderId { get; set; }
            [BsonElement("products")] public Product[] Products { get; set; }
        }

        public class OrderUnwound
        {
            [BsonElement("order_id")] public long OrderId { get; set; }
            [BsonElement("products")] public Product Product { get; set; } // note element name is still plural because it was unwound
        }

        public class Product
        {
            [BsonElement("prod_id")] public string ProductId { get; set; }
            [BsonElement("name")] public string Name { get; set; }
            [BsonElement("price")] public decimal Price { get; set; }
        }

        public class Result
        {
            [BsonElement("product_id")] public long ProductId { get; set; }
            [BsonElement("product")] public string Product { get; set; }
            [BsonElement("total_value")] public decimal TotalValue { get; set; }
            [BsonElement("quantity")] public long Quantity { get; set; }
        }
    }
}
