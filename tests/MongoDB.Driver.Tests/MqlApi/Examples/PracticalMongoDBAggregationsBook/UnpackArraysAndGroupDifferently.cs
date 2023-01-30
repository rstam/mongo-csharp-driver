using System;
using System.Collections.Generic;
using System.Text;
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
