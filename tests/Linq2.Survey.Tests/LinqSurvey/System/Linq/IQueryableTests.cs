using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using Linq2.Survey.Tests.Classes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Moq;
using Xunit;

namespace Linq2.Survey.Tests.LinqSurvey.System.Linq
{
    public class IQueryableTests : LinqSurveyTest
    {
        // public methods
        [Fact]
        public void Aggregate_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Aggregate((a, d) => new DocumentWithInt32 { Id = 0, X = a.X + d.X }));

            AssertNotSupported(queryable, terminator);
        }

        [Fact]
        public void Aggregate_with_seed_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Aggregate(0, (a, d) => a + d.X));

            AssertNotSupported(queryable, terminator);
        }

        [Fact]
        public void Aggregate_with_seed_and_selector_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Aggregate(new DocumentWithInt32 { X = 0 }, (a, d) => new DocumentWithInt32 { Id = 0, X = a.X + d.X }, a => a.X));

            AssertNotSupported(queryable, terminator);
        }

        [Fact]
        public void All_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.All(d => d.X > 0));

            AssertNotSupported(queryable, terminator);
        }

        [Fact]
        public void Any_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Any());

            AssertStages(queryable, terminator, "{ $limit : 1 }");
            AssertResult(queryable, terminator, true);
        }

        [Fact]
        public void Any_with_predicate_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Any(d => d.X == 3));

            AssertStages(queryable, terminator, "{ $match : { X : 3 } }", "{ $limit : 1 }");
            AssertResult(queryable, terminator, false);
        }

        [Fact]
        public void Average_with_decimal_selector_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : { $numberDecimal : '1.0' } }", "{ _id : 2, X : { $numberDecimal : '2.0' } }" };
            var collection = CreateCollection<DocumentWithDecimal>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Average(d => d.X));

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $avg : '$X' } } }");
            AssertResult(queryable, terminator, 1.5M);
        }

        [Fact]
        public void Average_with_decimal_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : { $numberDecimal : '1.0' } }", "{ _id : 2, X : { $numberDecimal : '2.0' } }" };
            var collection = CreateCollection<DocumentWithDecimal>(documents: documents);
            var subject = collection.AsQueryable().Select(d => d.X);

            var (queryable, terminator) = subject.WithTerminator(q => q.Average());

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $avg : '$X' } } }");
            AssertResult(queryable, terminator, 1.5M);
        }

        [Fact]
        public void Average_with_double_selector_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1.0 }", "{ _id : 2, X : 2.0 }" };
            var collection = CreateCollection<DocumentWithDouble>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Average(d => d.X));

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $avg : '$X' } } }");
            AssertResult(queryable, terminator, 1.5);
        }

        [Fact]
        public void Average_with_double_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1.0 }", "{ _id : 2, X : 2.0 }" };
            var collection = CreateCollection<DocumentWithDouble>(documents: documents);
            var subject = collection.AsQueryable().Select(d => d.X);

            var (queryable, terminator) = subject.WithTerminator(q => q.Average());

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $avg : '$X' } } }");
            AssertResult(queryable, terminator, 1.5);
        }

        [Fact]
        public void Average_with_float_selector_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1.0 }", "{ _id : 2, X : 2.0 }" };
            var collection = CreateCollection<DocumentWithSingle>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Average(d => d.X));

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $avg : '$X' } } }");
            AssertResult(queryable, terminator, 1.5F);
        }

        [Fact]
        public void Average_with_float_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1.0 }", "{ _id : 2, X : 2.0 }" };
            var collection = CreateCollection<DocumentWithSingle>(documents: documents);
            var subject = collection.AsQueryable().Select(d => d.X);

            var (queryable, terminator) = subject.WithTerminator(q => q.Average());

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $avg : '$X' } } }");
            AssertResult(queryable, terminator, 1.5F);
        }

        [Fact]
        public void Average_with_int_selector_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Average(d => d.X));

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $avg : '$X' } } }");
            AssertResult(queryable, terminator, 1.5);
        }

        [Fact]
        public void Average_with_int_is_supportedt()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable().Select(d => d.X);

            var (queryable, terminator) = subject.WithTerminator(q => q.Average());

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $avg : '$X' } } }");
            AssertResult(queryable, terminator, 1.5);
        }

        [Fact]
        public void Average_with_long_selector_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : { $numberLong : '1' } }", "{ _id : 2, X : { $numberLong : '2' } }" };
            var collection = CreateCollection<DocumentWithInt64>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Average(d => d.X));

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $avg : '$X' } } }");
            AssertResult(queryable, terminator, 1.5);
        }

        [Fact]
        public void Average_with_long_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : { $numberLong : '1' } }", "{ _id : 2, X : { $numberLong : '2' } }" };
            var collection = CreateCollection<DocumentWithInt64>(documents: documents);
            var subject = collection.AsQueryable().Select(d => d.X);

            var (queryable, terminator) = subject.WithTerminator(q => q.Average());

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $avg : '$X' } } }");
            AssertResult(queryable, terminator, 1.5);
        }

        [Fact]
        public void Average_with_nullable_decimal_selector_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : { $numberDecimal : '1.0' } }", "{ _id : 2, X : { $numberDecimal : '2.0' } }", "{ _id : 3, X : null }" };
            var collection = CreateCollection<DocumentWithNullableDecimal>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Average(d => d.X));

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $avg : '$X' } } }");
            AssertResult(queryable, terminator, 1.5M);
        }

        [Fact]
        public void Average_with_nullable_decimal_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : { $numberDecimal : '1.0' } }", "{ _id : 2, X : { $numberDecimal : '2.0' } }", "{ _id : 3, X : null }" };
            var collection = CreateCollection<DocumentWithNullableDecimal>(documents: documents);
            var subject = collection.AsQueryable().Select(d => d.X);

            var (queryable, terminator) = subject.WithTerminator(q => q.Average());

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $avg : '$X' } } }");
            AssertResult(queryable, terminator, 1.5M);
        }

        [Fact]
        public void Average_with_nullable_double_selector_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1.0 }", "{ _id : 2, X : 2.0 }", "{ _id : 3, X : null }" };
            var collection = CreateCollection<DocumentWithNullableDouble>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Average(d => d.X));

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $avg : '$X' } } }");
            AssertResult(queryable, terminator, 1.5);
        }

        [Fact]
        public void Average_with_nullable_double_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1.0 }", "{ _id : 2, X : 2.0 }", "{ _id : 3, X : null }" };
            var collection = CreateCollection<DocumentWithNullableDouble>(documents: documents);
            var subject = collection.AsQueryable().Select(d => d.X);

            var (queryable, terminator) = subject.WithTerminator(q => q.Average());

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $avg : '$X' } } }");
            AssertResult(queryable, terminator, 1.5);
        }

        [Fact]
        public void Average_with_nullable_float_selector_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1.0 }", "{ _id : 2, X : 2.0 }", "{ _id : 3, X : null }" };
            var collection = CreateCollection<DocumentWithNullableSingle>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Average(d => d.X));

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $avg : '$X' } } }");
            AssertResult(queryable, terminator, 1.5F);
        }

        [Fact]
        public void Average_with_nullable_float_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1.0 }", "{ _id : 2, X : 2.0 }", "{ _id : 3, X : null }" };
            var collection = CreateCollection<DocumentWithNullableSingle>(documents: documents);
            var subject = collection.AsQueryable().Select(d => d.X);

            var (queryable, terminator) = subject.WithTerminator(q => q.Average());

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $avg : '$X' } } }");
            AssertResult(queryable, terminator, 1.5F);
        }

        [Fact]
        public void Average_with_nullable_int_selector_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }", "{ _id : 3, X : null }" };
            var collection = CreateCollection<DocumentWithNullableInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Average(d => d.X));

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $avg : '$X' } } }");
            AssertResult(queryable, terminator, 1.5);
        }

        [Fact]
        public void Average_with_nullable_int_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }", "{ _id : 3, X : null }" };
            var collection = CreateCollection<DocumentWithNullableInt32>(documents: documents);
            var subject = collection.AsQueryable().Select(d => d.X);

            var (queryable, terminator) = subject.WithTerminator(q => q.Average());

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $avg : '$X' } } }");
            AssertResult(queryable, terminator, 1.5);
        }

        [Fact]
        public void Average_with_nullable_long_selector_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : { $numberLong : '1' } }", "{ _id : 2, X : { $numberLong : '2' } }", "{ _id : 3, X : null }" };
            var collection = CreateCollection<DocumentWithNullableInt64>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Average(d => d.X));

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $avg : '$X' } } }");
            AssertResult(queryable, terminator, 1.5);
        }

        [Fact]
        public void Average_with_nullable_long_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : { $numberLong : '1' } }", "{ _id : 2, X : { $numberLong : '2' } }", "{ _id : 3, X : null }" };
            var collection = CreateCollection<DocumentWithNullableInt64>(documents: documents);
            var subject = collection.AsQueryable().Select(d => d.X);

            var (queryable, terminator) = subject.WithTerminator(q => q.Average());

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $avg : '$X' } } }");
            AssertResult(queryable, terminator, 1.5);
        }

        [Fact]
        public void Cast_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();

            var queryable = subject.Cast<BsonDocument>();

            AssertNotSupported(queryable);
        }

        [Fact]
        public void Concat_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();

            var queryable = subject.Concat(new DocumentWithInt32[0]);

            AssertNotSupported(queryable);
        }

        [Fact]
        public void Contains_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();
            var item = new DocumentWithInt32();

            var (queryable, terminator) = subject.WithTerminator(q => q.Contains(item));

            AssertNotSupported(queryable, terminator);
        }

        [Fact]
        public void Contains_with_comparer_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();
            var item = new DocumentWithInt32();
            var comparer = Mock.Of<IEqualityComparer<DocumentWithInt32>>();

            var (queryable, terminator) = subject.WithTerminator(q => q.Contains(item, comparer));

            AssertNotSupported(queryable, terminator);
        }

        [Fact]
        public void Count_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Count());

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $sum : 1 } } }");
            AssertResult(queryable, terminator, 2);
        }

        [Fact]
        public void Count_with_predicate_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Count(d => d.X == 1));

            AssertStages(queryable, terminator, "{ $match : { X : 1 } }", "{ $group : { _id : 1, __result : { $sum : 1 } } }");
            AssertResult(queryable, terminator, 1);
        }

        [Fact]
        public void DefaultIfEmpty_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();

            var queryable = subject.DefaultIfEmpty();

            AssertNotSupported(queryable);
        }

        [Fact]
        public void DefaultIfEmpty_with_defaultValue_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();
            var defaultValue = new DocumentWithInt32();

            var queryable = subject.DefaultIfEmpty(defaultValue);

            AssertNotSupported(queryable);
        }

        [Fact]
        public void Distinct_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }", "{ _id : 3, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable().Select(d => d.X);

            var queryable = subject.Distinct();

            AssertStages(queryable, "{ $group : { _id : '$X' } }");
            AssertSortedResults(queryable, r => r, new[] { 1, 2 });
        }

        [Fact]
        public void Distinct_with_comparer_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable().Select(d => d.X);
            var comparer = Mock.Of<IEqualityComparer<int>>();

            var queryable = subject.Distinct(comparer);

            AssertNotSupported(queryable);
        }

        [Fact]
        public void ElementAt_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.ElementAt(1));

            AssertNotSupported(queryable, terminator);
        }

        [Fact]
        public void ElementAtOrDefault_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.ElementAtOrDefault(1));

            AssertNotSupported(queryable, terminator);
        }

        [Fact]
        public void Except_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();
            var source2 = new[] { new DocumentWithInt32() };

            var queryable = subject.Except(source2);

            AssertNotSupported(queryable);
        }
        [Fact]
        public void Except_with_comparer_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();
            var source2 = new[] { new DocumentWithInt32() };
            var comparer = Mock.Of<IEqualityComparer<DocumentWithInt32>>();

            var queryable = subject.Except(source2, comparer);

            AssertNotSupported(queryable);
        }

        [Fact]
        public void First_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.First());

            AssertStages(queryable, terminator, "{ $limit : 1 }");
            AssertResultId(queryable, terminator, 1);
        }

        [Fact]
        public void First_with_predicate_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.First(d => d.X == 2));

            AssertStages(queryable, terminator, "{ $match : { X : 2 } }", "{ $limit : 1 }");
            AssertResultId(queryable, terminator, 2);
        }

        [Fact]
        public void FirstOrDefault_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.FirstOrDefault());

            AssertStages(queryable, terminator, "{ $limit : 1 }");
            AssertResultId(queryable, terminator, 1);
        }

        [Fact]
        public void FirstOrDefault_with_predicate_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.FirstOrDefault(d => d.X == 3));

            AssertStages(queryable, terminator, "{ $match : { X : 3 } }", "{ $limit : 1 }");
            AssertResult(queryable, terminator, null);
        }

        [Fact]
        public void GroupBy_with_keySelector_has_a_bug()
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
        public void GroupBy_with_keySelector_and_comparer_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();
            var comparer = Mock.Of<IEqualityComparer<int>>();

            var queryable = subject.GroupBy(d => d.X, comparer);

            AssertNotSupported(queryable);
        }

        [Fact]
        public void GroupBy_with_keySelector_and_elementSelector_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();

            var queryable = subject.GroupBy(d => d.X, d => d.Id);

            AssertNotSupported(queryable);
        }

        [Fact]
        public void GroupBy_with_keySelector_and_elementSelector_and_comparer_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();
            var comparer = Mock.Of<IEqualityComparer<int>>();

            var queryable = subject.GroupBy(d => d.X, d => d.Id, comparer);

            AssertNotSupported(queryable);
        }

        [Fact]
        public void GroupBy_with_keySelector_and_elementSelector_and_resultSelector_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();

            var queryable = subject.GroupBy(d => d.X, d => d.Id, (k, e) => e.Count());

            AssertNotSupported(queryable);
        }

        [Fact]
        public void GroupBy_with_keySelector_and_elementSelector_and_resultSelector_and_comparer_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();
            var comparer = Mock.Of<IEqualityComparer<int>>();

            var queryable = subject.GroupBy(d => d.X, d => d.Id, (k, e) => e.Count(), comparer);

            AssertNotSupported(queryable);
        }

        [Fact]
        public void GroupBy_with_keySelector_and_resultSelector_has_a_bug()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();

            var queryable = subject.GroupBy(d => d.X, (k, e) => e.Count());

            AssertStages(queryable, "{ $group : { $sum : 1, _id : 0} }"); // bug: invalid stage
        }

        [Fact]
        public void GroupBy_with_keySelector_resultSelector_and_comparer_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();
            var comparer = Mock.Of<IEqualityComparer<int>>();

            var queryable = subject.GroupBy(d => d.X, (k, e) => e.Count(), comparer);

            AssertNotSupported(queryable);
        }

        [Fact]
        public void GroupJoin_with_ienumerable_inner_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();
            var inner = new DocumentWithInt32[0];

            var queryable = subject.GroupJoin(inner, o => o.Id, i => i.Id, (o, i) => new { o, i });

            AssertNotSupported(queryable);
        }

        [Fact]
        public void GroupJoin_with_imongocollection_inner_is_supported()
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
        public void GroupJoin_with_iqueryable_inner_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();
            var innerDocuments = new[] { "{ _id : 1, X : 11 }" };
            var inner = CreateCollection<DocumentWithInt32>(collectionName: "inner", documents: innerDocuments).AsQueryable();

            var queryable = subject.GroupJoin(inner, o => o.Id, i => i.Id, (o, i) => new { o, i });

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
        public void GroupJoin_with_comparer_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();
            var inner = CreateCollection<DocumentWithInt32>(collectionName: "inner").AsQueryable();
            var comparer = Mock.Of<IEqualityComparer<int>>();

            var queryable = subject.GroupJoin(inner, o => o.Id, i => i.Id, (o, i) => new { o, i }, comparer);

            AssertNotSupported(queryable);
        }

        [Fact]
        public void Intersect_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();
            var source2 = new DocumentWithInt32[0];

            var queryable = subject.Intersect(source2);

            AssertNotSupported(queryable);
        }

        [Fact]
        public void Intersect_with_comparer_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();
            var source2 = new DocumentWithInt32[0];
            var comparer = Mock.Of<IEqualityComparer<DocumentWithInt32>>();

            var queryable = subject.Intersect(source2, comparer);

            AssertNotSupported(queryable);
        }

        [Fact]
        public void Join_with_ienumerable_inner_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();
            var inner = new DocumentWithInt32[0];

            var queryable = subject.Join(inner, o => o.Id, i => i.Id, (o, i) => new { o, i });

            AssertNotSupported(queryable);
        }

        [Fact]
        public void Join_with_imongocollection_inner_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();
            var innerDocuments = new[] { "{ _id : 1, X : 11 }" };
            var innerCollection = CreateCollection<DocumentWithInt32>(collectionName: "inner", documents: innerDocuments);

            var queryable = subject.Join(innerCollection, o => o.Id, i => i.Id, (o, i) => new { o, i });

            AssertStages(queryable, "{ $lookup : { from : 'inner', localField : '_id', foreignField : '_id', as : 'i' } }", "{ $unwind : '$i' }");
            var results = queryable.AsEnumerable().OrderBy(d => d.o.Id).ToList();
            results.Count().Should().Be(1);
            var result1 = results[0];
            result1.o.Id.Should().Be(1);
            result1.i.Id.Should().Be(1);
        }

        [Fact]
        public void Join_with_iqueryable_inner_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();
            var innerDocuments = new[] { "{ _id : 1, X : 11 }" };
            var inner = CreateCollection<DocumentWithInt32>(collectionName: "inner", documents: innerDocuments).AsQueryable();

            var queryable = subject.Join(inner, o => o.Id, i => i.Id, (o, i) => new { o, i });

            AssertStages(queryable, "{ $lookup : { from : 'inner', localField : '_id', foreignField : '_id', as : 'i' } }", "{ $unwind : '$i' }");
            var results = queryable.AsEnumerable().OrderBy(d => d.o.Id).ToList();
            results.Count().Should().Be(1);
            var result1 = results[0];
            result1.o.Id.Should().Be(1);
            result1.i.Id.Should().Be(1);
        }

        [Fact]
        public void Join_with_comparer_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();
            var inner = CreateCollection<DocumentWithInt32>(collectionName: "inner").AsQueryable();
            var comparer = Mock.Of<IEqualityComparer<int>>();

            var queryable = subject.Join(inner, o => o.Id, i => i.Id, (o, i) => new { o, i }, comparer);

            AssertNotSupported(queryable);
        }

        [Fact]
        public void Last_is_not_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Last());

            AssertNotSupported(queryable, terminator);
        }

        [Fact]
        public void Last_with_predicate_is_not_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Last(d => d.X == 1));

            AssertNotSupported(queryable, terminator);
        }

        [Fact]
        public void LastOrDefault_is_not_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.LastOrDefault());

            AssertNotSupported(queryable, terminator);
        }

        [Fact]
        public void LastOrDefault_with_predicate_is_not_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.LastOrDefault(d => d.X == 3));

            AssertNotSupported(queryable, terminator);
        }

        [Fact]
        public void LongCount_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.LongCount());

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $sum : 1 } } }");
            AssertResult(queryable, terminator, 2);
        }

        [Fact]
        public void LongCount_with_predicate_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.LongCount(d => d.X == 1));

            AssertStages(queryable, terminator, "{ $match : { X : 1 } }", "{ $group : { _id : 1, __result : { $sum : 1 } } }");
            AssertResult(queryable, terminator, 1);
        }

        [Fact]
        public void Max_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Max());

            AssertNotSupported(queryable, terminator);
        }

        [Fact]
        public void Max_with_selector_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Max(d => d.X));

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $max : '$X' } } }");
            AssertResult(queryable, terminator, 2);
        }

        [Fact]
        public void Min_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Min());

            AssertNotSupported(queryable, terminator);
        }

        [Fact]
        public void Min_with_selector_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Min(d => d.X));

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $min : '$X' } } }");
            AssertResult(queryable, terminator, 1);
        }

        [Fact]
        public void OfType_with_hierarchical_discriminator_is_supported()
        {
            _ = new BaseDocumentWithHierarchicalDiscriminator(); // force execution of static constructor
            BsonSerializer.LookupDiscriminatorConvention(typeof(BaseDocumentWithHierarchicalDiscriminator)).Should().BeOfType<HierarchicalDiscriminatorConvention>();

            var documents = new[]
            {
                "{ _id : 1, _t : 'BaseDocumentWithHierarchicalDiscriminator', X : 1 }",
                "{ _id : 2, _t : ['BaseDocumentWithHierarchicalDiscriminator', 'DerivedDocumentWithHierarchicalDiscriminator'], X : 2, Y : 2 }"
            };
            var collection = CreateCollection<BaseDocumentWithHierarchicalDiscriminator>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.OfType<DerivedDocumentWithHierarchicalDiscriminator>();

            AssertStages(queryable, "{ $match : { _t : 'DerivedDocumentWithHierarchicalDiscriminator' } }");
            AssertResultIds(queryable, 2);
        }

        [Fact]
        public void OfType_with_scalar_discriminator_is_supported()
        {
            _ = new BaseDocumentWithScalarDiscriminator(); // force execution of static constructor
            BsonSerializer.LookupDiscriminatorConvention(typeof(BaseDocumentWithScalarDiscriminator)).Should().BeOfType<ScalarDiscriminatorConvention>();

            var documents = new[]
            {
                "{ _id : 1, _t : 'BaseDocumentWithScalarDiscriminator', X : 1 }",
                "{ _id : 2, _t : 'DerivedDocumentWithScalarDiscriminator', X : 2, Y : 2 }"
            };
            var collection = CreateCollection<BaseDocumentWithScalarDiscriminator>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.OfType<DerivedDocumentWithScalarDiscriminator>();

            AssertStages(queryable, "{ $match : { _t : 'DerivedDocumentWithScalarDiscriminator' } }");
            AssertResultIds(queryable, 2);
        }

        [Fact]
        public void OrderBy_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.OrderBy(d => d.Id);

            AssertStages(queryable, "{ $sort : { _id : 1 } }");
            AssertResultIds(queryable, 1, 2);
        }

        [Fact]
        public void OrderBy_with_comparer_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();
            var comparer = Mock.Of<IComparer<int>>();

            var queryable = subject.OrderBy(d => d.Id, comparer);

            AssertNotSupported(queryable);
        }

        [Fact]
        public void OrderByDescending_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.OrderByDescending(d => d.Id);

            AssertStages(queryable, "{ $sort : { _id : -1 } }");
            AssertResultIds(queryable, 2, 1);
        }

        [Fact]
        public void OrderByDescending_with_comparer_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();
            var comparer = Mock.Of<IComparer<int>>();

            var queryable = subject.OrderByDescending(d => d.Id, comparer);

            AssertNotSupported(queryable);
        }

        [Fact]
        public void Reverse_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();

            var queryable = subject.Reverse();

            AssertNotSupported(queryable);
        }

        [Fact]
        public void Select_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.Select(d => d.X);

            AssertStages(queryable, "{ $project : { X : '$X', _id : 0 } }");
            AssertResults(queryable, 1, 2);
        }

        [Fact]
        public void Select_with_selector_taking_index_is_not_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.Select((d, i) => d.X);

            AssertNotSupported(queryable);
        }

        [Fact]
        public void SelectMany_is_supported()
        {
            var documents = new[] { "{ _id : 1, A : [1] }", "{ _id : 2, A : [2, 3] }" };
            var collection = CreateCollection<DocumentWithInt32Array>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.SelectMany(d => d.A);

            AssertStages(queryable, "{ $unwind : '$A' }", "{ $project : { A : '$A', _id : 0 } }");
            AssertResults(queryable, 1, 2, 3);
        }

        [Fact]
        public void SelectMany_with_selector_taking_index_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32Array>();
            var subject = collection.AsQueryable();

            var queryable = subject.SelectMany((d, i) => d.A);

            AssertNotSupported(queryable);
        }

        [Fact]
        public void SelectMany_with_collectionSelector_is_supported()
        {
            var documents = new[] { "{ _id : 1, A : [1] }", "{ _id : 2, A : [2, 3] }" };
            var collection = CreateCollection<DocumentWithInt32Array>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.SelectMany(d => d.A, (d, e) => e);

            AssertStages(queryable, "{ $unwind : '$A' }", "{ $project : { A : '$A', _id : 0 } }");
            AssertResults(queryable, 1, 2, 3);
        }

        [Fact]
        public void SelectMany_with_collectionSelector_taking_index_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32Array>();
            var subject = collection.AsQueryable();

            var queryable = subject.SelectMany((d, i) => d.A, (d, e) => e);

            AssertNotSupported(queryable);
        }

        [Fact]
        public void SequenceEqual_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();
            var source2 = new DocumentWithInt32[0];

            var (queryable, terminator) = subject.WithTerminator(q => q.SequenceEqual(source2));

            AssertNotSupported(queryable, terminator);
        }

        [Fact]
        public void SequenceEqual_with_comparer_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();
            var source2 = new DocumentWithInt32[0];
            var comparer = Mock.Of<IEqualityComparer<DocumentWithInt32>>();

            var (queryable, terminator) = subject.WithTerminator(q => q.SequenceEqual(source2, comparer));

            AssertNotSupported(queryable, terminator);
        }

        [Fact]
        public void Single_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Single());

            AssertStages(queryable, terminator, "{ $limit : 2 }");
            AssertResultId(queryable, terminator, 1);
        }

        [Fact]
        public void Single_with_predicate_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Single(d => d.X == 2));

            AssertStages(queryable, terminator, "{ $match : { X : 2 } }", "{ $limit : 2 }");
            AssertResultId(queryable, terminator, 2);
        }

        [Fact]
        public void SingleOrDefault_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.SingleOrDefault());

            AssertStages(queryable, terminator, "{ $limit : 2 }");
            AssertResultId(queryable, terminator, 1);
        }

        [Fact]
        public void SingleOrDefault_with_predicate_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.SingleOrDefault(d => d.X == 3));

            AssertStages(queryable, terminator, "{ $match : { X : 3 } }", "{ $limit : 2 }");
            AssertResult(queryable, terminator, null);
        }

        [Fact]
        public void Skip_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.Skip(1);

            AssertStages(queryable, "{ $skip : 1 }");
            AssertResultIds(queryable, 2);
        }

        [Fact]
        public void SkipWhile_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();

            var queryable = subject.SkipWhile(d => d.X < 2);

            AssertNotSupported(queryable);
        }

        [Fact]
        public void SkipWhile_with_predicate_taking_index_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();

            var queryable = subject.SkipWhile((d, i) => d.X < 2);

            AssertNotSupported(queryable);
        }

        [Fact]
        public void Sum_with_decimal_selector_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : { $numberDecimal : '1.0' } }", "{ _id : 2, X : { $numberDecimal : '2.0' } }" };
            var collection = CreateCollection<DocumentWithDecimal>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Sum(d => d.X));

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $sum : '$X' } } }");
            AssertResult(queryable, terminator, 3.0M);
        }

        [Fact]
        public void Sum_with_decimal_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : { $numberDecimal : '1.0' } }", "{ _id : 2, X : { $numberDecimal : '2.0' } }" };
            var collection = CreateCollection<DocumentWithDecimal>(documents: documents);
            var subject = collection.AsQueryable().Select(d => d.X);

            var (queryable, terminator) = subject.WithTerminator(q => q.Sum());

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $sum : '$X' } } }");
            AssertResult(queryable, terminator, 3.0M);
        }

        [Fact]
        public void Sum_with_double_selector_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1.0 }", "{ _id : 2, X : 2.0 }" };
            var collection = CreateCollection<DocumentWithDouble>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Sum(d => d.X));

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $sum : '$X' } } }");
            AssertResult(queryable, terminator, 3.0);
        }

        [Fact]
        public void Sum_with_double_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1.0 }", "{ _id : 2, X : 2.0 }" };
            var collection = CreateCollection<DocumentWithDouble>(documents: documents);
            var subject = collection.AsQueryable().Select(d => d.X);

            var (queryable, terminator) = subject.WithTerminator(q => q.Sum());

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $sum : '$X' } } }");
            AssertResult(queryable, terminator, 3.0);
        }

        [Fact]
        public void Sum_with_float_selector_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1.0 }", "{ _id : 2, X : 2.0 }" };
            var collection = CreateCollection<DocumentWithSingle>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Sum(d => d.X));

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $sum : '$X' } } }");
            AssertResult(queryable, terminator, 3.0F);
        }

        [Fact]
        public void Sum_with_float_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1.0 }", "{ _id : 2, X : 2.0 }" };
            var collection = CreateCollection<DocumentWithSingle>(documents: documents);
            var subject = collection.AsQueryable().Select(d => d.X);

            var (queryable, terminator) = subject.WithTerminator(q => q.Sum());

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $sum : '$X' } } }");
            AssertResult(queryable, terminator, 3.0F);
        }

        [Fact]
        public void Sum_with_int_selector_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Sum(d => d.X));

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $sum : '$X' } } }");
            AssertResult(queryable, terminator, 3);
        }

        [Fact]
        public void Sum_with_int_is_supportedt()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable().Select(d => d.X);

            var (queryable, terminator) = subject.WithTerminator(q => q.Sum());

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $sum : '$X' } } }");
            AssertResult(queryable, terminator, 3);
        }

        [Fact]
        public void Sum_with_long_selector_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : { $numberLong : '1' } }", "{ _id : 2, X : { $numberLong : '2' } }" };
            var collection = CreateCollection<DocumentWithInt64>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Sum(d => d.X));

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $sum : '$X' } } }");
            AssertResult(queryable, terminator, 3);
        }

        [Fact]
        public void Sum_with_long_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : { $numberLong : '1' } }", "{ _id : 2, X : { $numberLong : '2' } }" };
            var collection = CreateCollection<DocumentWithInt64>(documents: documents);
            var subject = collection.AsQueryable().Select(d => d.X);

            var (queryable, terminator) = subject.WithTerminator(q => q.Sum());

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $sum : '$X' } } }");
            AssertResult(queryable, terminator, 3);
        }

        [Fact]
        public void Sum_with_nullable_decimal_selector_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : { $numberDecimal : '1.0' } }", "{ _id : 2, X : { $numberDecimal : '2.0' } }", "{ _id : 3, X : null }" };
            var collection = CreateCollection<DocumentWithNullableDecimal>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Sum(d => d.X));

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $sum : '$X' } } }");
            AssertResult(queryable, terminator, 3.0M);
        }

        [Fact]
        public void Sum_with_nullable_decimal_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : { $numberDecimal : '1.0' } }", "{ _id : 2, X : { $numberDecimal : '2.0' } }", "{ _id : 3, X : null }" };
            var collection = CreateCollection<DocumentWithNullableDecimal>(documents: documents);
            var subject = collection.AsQueryable().Select(d => d.X);

            var (queryable, terminator) = subject.WithTerminator(q => q.Sum());

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $sum : '$X' } } }");
            AssertResult(queryable, terminator, 3.0M);
        }

        [Fact]
        public void Sum_with_nullable_double_selector_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1.0 }", "{ _id : 2, X : 2.0 }", "{ _id : 3, X : null }" };
            var collection = CreateCollection<DocumentWithNullableDouble>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Sum(d => d.X));

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $sum : '$X' } } }");
            AssertResult(queryable, terminator, 3.0);
        }

        [Fact]
        public void Sum_with_nullable_double_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1.0 }", "{ _id : 2, X : 2.0 }", "{ _id : 3, X : null }" };
            var collection = CreateCollection<DocumentWithNullableDouble>(documents: documents);
            var subject = collection.AsQueryable().Select(d => d.X);

            var (queryable, terminator) = subject.WithTerminator(q => q.Sum());

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $sum : '$X' } } }");
            AssertResult(queryable, terminator, 3.0);
        }

        [Fact]
        public void Sum_with_nullable_float_selector_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1.0 }", "{ _id : 2, X : 2.0 }", "{ _id : 3, X : null }" };
            var collection = CreateCollection<DocumentWithNullableSingle>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Sum(d => d.X));

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $sum : '$X' } } }");
            AssertResult(queryable, terminator, 3.0F);
        }

        [Fact]
        public void Sum_with_nullable_float_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1.0 }", "{ _id : 2, X : 2.0 }", "{ _id : 3, X : null }" };
            var collection = CreateCollection<DocumentWithNullableSingle>(documents: documents);
            var subject = collection.AsQueryable().Select(d => d.X);

            var (queryable, terminator) = subject.WithTerminator(q => q.Sum());

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $sum : '$X' } } }");
            AssertResult(queryable, terminator, 3.0F);
        }

        [Fact]
        public void Sum_with_nullable_int_selector_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }", "{ _id : 3, X : null }" };
            var collection = CreateCollection<DocumentWithNullableInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Sum(d => d.X));

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $sum : '$X' } } }");
            AssertResult(queryable, terminator, 3);
        }

        [Fact]
        public void Sum_with_nullable_int_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }", "{ _id : 3, X : null }" };
            var collection = CreateCollection<DocumentWithNullableInt32>(documents: documents);
            var subject = collection.AsQueryable().Select(d => d.X);

            var (queryable, terminator) = subject.WithTerminator(q => q.Sum());

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $sum : '$X' } } }");
            AssertResult(queryable, terminator, 3);
        }

        [Fact]
        public void Sum_with_nullable_long_selector_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : { $numberLong : '1' } }", "{ _id : 2, X : { $numberLong : '2' } }", "{ _id : 3, X : null }" };
            var collection = CreateCollection<DocumentWithNullableInt64>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Sum(d => d.X));

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $sum : '$X' } } }");
            AssertResult(queryable, terminator, 3);
        }

        [Fact]
        public void Sum_with_nullable_long_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : { $numberLong : '1' } }", "{ _id : 2, X : { $numberLong : '2' } }", "{ _id : 3, X : null }" };
            var collection = CreateCollection<DocumentWithNullableInt64>(documents: documents);
            var subject = collection.AsQueryable().Select(d => d.X);

            var (queryable, terminator) = subject.WithTerminator(q => q.Sum());

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $sum : '$X' } } }");
            AssertResult(queryable, terminator, 3);
        }

        [Fact]
        public void Take_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.Take(1);

            AssertStages(queryable, "{ $limit : 1 }");
            AssertResultIds(queryable, 1);
        }

        [Fact]
        public void TakeWhile_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();

            var queryable = subject.TakeWhile(d => d.X < 2);

            AssertNotSupported(queryable);
        }

        [Fact]
        public void TakeWhile_with_predicate_taking_index_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();

            var queryable = subject.TakeWhile((d, i) => d.X < 2);

            AssertNotSupported(queryable);
        }

        [Fact]
        public void ThenBy_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 1 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable().OrderBy(d => d.X);

            var queryable = subject.ThenBy(d => d.Id);

            AssertStages(queryable, "{ $sort : { X : 1, _id : 1 } }");
            AssertResultIds(queryable, 1, 2);
        }

        [Fact]
        public void ThenBy_with_comparer_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable().OrderBy(d => d.X); ;
            var comparer = Mock.Of<IComparer<int>>();

            var queryable = subject.ThenBy(d => d.Id, comparer);

            AssertNotSupported(queryable);
        }

        [Fact]
        public void ThenByDescending_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 1 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable().OrderBy(d => d.X); ;

            var queryable = subject.ThenByDescending(d => d.Id);

            AssertStages(queryable, "{ $sort : { X : 1, _id : -1 } }");
            AssertResultIds(queryable, 2, 1);
        }

        [Fact]
        public void ThenByDescending_with_comparer_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable().OrderBy(d => d.X); ;
            var comparer = Mock.Of<IComparer<int>>();

            var queryable = subject.ThenByDescending(d => d.Id, comparer);

            AssertNotSupported(queryable);
        }

        [Fact]
        public void Union_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();
            var source2 = new DocumentWithInt32[0];

            var queryable = subject.Union(source2);

            AssertNotSupported(queryable);
        }

        [Fact]
        public void Union_with_comparer_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();
            var source2 = new DocumentWithInt32[0];
            var comparer = Mock.Of<IEqualityComparer<DocumentWithInt32>>();

            var queryable = subject.Union(source2, comparer);

            AssertNotSupported(queryable);
        }

        [Fact]
        public void Where_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.Where(d => d.X == 1);

            AssertStages(queryable, "{ $match : { X : 1 } }");
            AssertResultIds(queryable, 1);
        }

        [Fact]
        public void Where_with_predicate_taking_index_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();

            var queryable = subject.Where((d, i) => d.X == 1);

            AssertNotSupported(queryable);
        }
    }

    public static class IQueryableExtensions
    {
        public static (IQueryable<TDocument>, Expression<Func<IQueryable<TDocument>, TResult>>) WithTerminator<TDocument, TResult>(this IMongoQueryable<TDocument> queryable, Expression<Func<IQueryable<TDocument>, TResult>> terminator)
        {
            return (queryable, terminator);
        }
    }
}
