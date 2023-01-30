using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver.MqlApi;
using Xunit;

namespace MongoDB.Driver.Tests.MqlApi.Examples.PracticalMongoDBAggregationsBook
{
    // https://www.practical-mongodb-aggregations.com/examples/foundational/filtered-top-subset.html
    public class FilteredTopSubset : MqlIntegrationTest
    {
        [Fact]
        public void Filtered_top_subset_example_should_work()
        {
            var collection = GetCollection<Person>();

            var pipeline = Mql.Pipeline(collection)
                .Match(x => x.Vocation == "ENGINEER")
                .Sort(x => Mql.Descending(x.DateOfBirth))
                .Limit(3)
                .Project(x => new { x.PersonId, x.FirstName, x.LastName, x.DateOfBirth });
        }

        public class Person
        {
            public int PersonId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public DateTime DateOfBirth { get; set; }
            public string Vocation { get; set; }
        }
    }
}
