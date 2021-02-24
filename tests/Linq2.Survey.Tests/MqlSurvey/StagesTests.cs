using System.Linq;
using FluentAssertions;
using Linq2.Survey.Tests.Classes;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Xunit;

namespace Linq2.Survey.Tests.MqlSurvey
{
    public class StagesTests : LinqSurveyTest
    {
        [Fact]
        public void AddFields_stage_is_not_supported()
        {
            // LINQ always does full projections
        }

        [Fact]
        public void Bucket_stage_is_not_supported()
        {
            // no LINQ equivalent
        }

        [Fact]
        public void BucketAuto_stage_is_not_supported()
        {
            // no LINQ equivalent
        }

        [Fact]
        public void CollStats_stage_is_not_supported()
        {
            // no LINQ equivalent
        }

        [Fact]
        public void Count_stage_is_not_supported()
        {
            // LINQ Count method is implemented using $group with _id : 1 and $sum : 1
        }

        [Fact]
        public void CurrentOp_stage_is_not_supported()
        {
            // no LINQ equivalent
        }

        [Fact]
        public void Facet_stage_is_not_supported()
        {
            // no LINQ equivalent?
        }

        [Fact]
        public void GeoNear_stage_is_not_supported()
        {
            // no LINQ equivalent
        }

        [Fact]
        public void GraphLookup_stage_is_not_supported()
        {
            // no LINQ equivalent
        }

        [Fact]
        public void Group_stage_is_supported_but_has_bugs()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }", "{ _id : 3, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.GroupBy(d => d.X);

            AssertStages(queryable, "{ $group : { _id : '$X' } }"); // bug: failure to project $$ROOT
            var results = queryable.AsEnumerable().OrderBy(g => g.Key).ToList();
            results.Count.Should().Be(2);
            AssertGrouping(results[0], 1); // bug: grouping is empty
            AssertGrouping(results[1], 2); // bug: grouping is empty

            void AssertGrouping(IGrouping<int, DocumentWithInt32> grouping, int expectedKey, params string[] expectedElements)
            {
                grouping.Key.Should().Be(expectedKey);
                grouping.ToList().Should().Equal(expectedElements.Select(e => BsonSerializer.Deserialize<DocumentWithInt32>(e)));
            }
        }

        [Fact]
        public void IndexStats_stage_is_not_supported()
        {
            // no LINQ equivalent
        }

        [Fact]
        public void Limit_stage_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.Take(1);

            AssertStages(queryable, "{ $limit : 1 }");
            AssertResultIds(queryable, 1);
        }

        [Fact]
        public void ListLocalSessions_stage_is_not_supported()
        {
            // no LINQ equivalent
        }

        [Fact]
        public void ListSessions_stage_is_not_supported()
        {
            // no LINQ equivalent
        }

        [Fact]
        public void Lookup_stage_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();
            var innerDocuments = new[] { "{ _id : 1, X : 11 }" };
            var innerCollection = CreateCollection<DocumentWithInt32>(collectionName: "inner", documents: innerDocuments);

            var queryable = subject.GroupJoin(innerCollection, o => o.Id, i => i.Id, (o, i) => new { o, i });

            AssertStages(queryable, "{ $lookup : { from : 'inner', localField : '_id', foreignField : '_id', as : 'i' } }");
            var results = queryable.AsEnumerable().OrderBy(d => d.o.Id).ToList();
            results.Count().Should().Be(2);
            var result1 = results[0];
            var result2 = results[1];
            result1.o.Id.Should().Be(1);
            result1.i.Select(d => d.Id).Should().Equal(1);
            result2.o.Id.Should().Be(2);
            result2.i.Should().BeEmpty();
        }

        [Fact]
        public void Match_stage_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.Where(d => d.X == 1);

            AssertStages(queryable, "{ $match : { X : 1 } }");
            AssertResultIds(queryable, 1);
        }

        [Fact]
        public void Out_stage_is_not_supported()
        {
            // no LINQ equivalent
        }

        [Fact]
        public void PlanCacheStats_stage_is_not_supported()
        {
            // no LINQ equivalent
        }

        [Fact]
        public void ProjectStage_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.Select(d => d.X);

            AssertStages(queryable, "{ $project : { X : '$X', _id : 0 } }");
            AssertResults(queryable, 1, 2);
        }

        [Fact]
        public void Redact_stage_is_not_supported()
        {
            // no LINQ equivalent
        }

        [Fact]
        public void ReplaceRoot_stage_is_not_supported()
        {
            // no LINQ equivalent
        }

        [Fact]
        public void ReplaceWith_stage_is_not_supported()
        {
            // no LINQ equivalent
        }

        [Fact]
        public void Sample_stage_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.Sample(100);

            AssertStages(queryable, "{ $sample : { size : 100 } }");
            AssertSortedResultIds(queryable, 1, 2);
        }

        [Fact]
        public void Set_stage_is_not_supported()
        {
            // LINQ always does full projections
        }

        [Fact]
        public void Skip_stage_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.Skip(1);

            AssertStages(queryable, "{ $skip : 1 }");
            AssertResultIds(queryable, 2);
        }

        [Fact]
        public void Sort_stage_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.OrderBy(d => d.Id);

            AssertStages(queryable, "{ $sort : { _id : 1 } }");
            AssertResultIds(queryable, 1, 2);
        }

        [Fact]
        public void SortByCount_stage_is_not_supported()
        {
            // no LINQ equivalent
        }

        [Fact]
        public void UnionWith_stage_is_not_supported()
        {
            // Union appears to be a LINQ equivalent
        }

        [Fact]
        public void Unset_stage_is_not_supported()
        {
            // LINQ always does full projections
        }

        [Fact]
        public void Unwind_stage_is_supported()
        {
            var documents = new[] { "{ _id : 1, A : [1] }", "{ _id : 2, A : [2, 3] }" };
            var collection = CreateCollection<DocumentWithInt32Array>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.SelectMany(d => d.A);

            AssertStages(queryable, "{ $unwind : '$A' }", "{ $project : { A : '$A', _id : 0 } }");
            AssertResults(queryable, 1, 2, 3);
        }
    }
}
