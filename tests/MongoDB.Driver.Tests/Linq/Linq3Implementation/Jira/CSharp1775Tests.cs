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

using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver.TestHelpers;
using FluentAssertions;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3Implementation.Jira;

public class CSharp1775Tests : LinqIntegrationTest<CSharp1775Tests.ClassFixture>
{
    public CSharp1775Tests(ClassFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public void Test1()
    {
        var awardProviderProductCollection = Fixture.AwardProviderProduct;
        var productCollection = Fixture.ProductCollection;

        var query = from p in awardProviderProductCollection.AsQueryable()
            join o in productCollection.AsQueryable() on p.Sku equals o.Sku into joined
            from j in joined.DefaultIfEmpty()
            where p.Reference["AwardChoiceSource"] == "NORC"
            select new AwardProviderProduct()
            {

                Reference = p.Reference,
                Sku = p.Sku

            };

        var stages = Translate(awardProviderProductCollection, query);
        AssertStages(stages, "{ }"); // TBD
    }

    public class AwardProviderProduct
    {
        public int Id { get; set; }
        public int Sku { get; set; }
        public Dictionary<string, string> Reference { get; set; }
    }

    public class Product
    {
        public int Id { get; set; }
        public int Sku { get; set; }
    }

    public sealed class ClassFixture : MongoDatabaseFixture
    {
        public IMongoCollection<AwardProviderProduct> AwardProviderProduct { get; private set; }
        public IMongoCollection<Product> ProductCollection { get; private set; }

        protected override void InitializeFixture()
        {
            AwardProviderProduct = CreateCollection<AwardProviderProduct>("awardProviderProduct");
            ProductCollection = CreateCollection<Product>("product");
        }
    }
}
