using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using Linq2.Survey.Tests.Classes;
using MongoDB.Bson;
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
        public void Any_should_translate_to_limit_stage()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Any());

            AssertStages(queryable, terminator, "{ $limit : 1 }");
            AssertResult(queryable, terminator, true);
        }

        [Fact]
        public void Any_with_predicate_should_translate_to_match_and_limit_stages()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Any(d => d.X == 3));

            AssertStages(queryable, terminator, "{ $match : { X : 3 } }", "{ $limit : 1 }");
            AssertResult(queryable, terminator, false);
        }

        [Fact]
        public void Average_with_decimal_selector_should_return_expected_result()
        {
            var documents = new[] { "{ _id : 1, X : { $numberDecimal : '1.0' } }", "{ _id : 2, X : { $numberDecimal : '2.0' } }" };
            var collection = CreateCollection<DocumentWithDecimal>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Average(d => d.X));

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $avg : '$X' } } }");
            AssertResult(queryable, terminator, 1.5M);
        }

        [Fact]
        public void Average_with_decimal_should_return_expected_result()
        {
            var documents = new[] { "{ _id : 1, X : { $numberDecimal : '1.0' } }", "{ _id : 2, X : { $numberDecimal : '2.0' } }" };
            var collection = CreateCollection<DocumentWithDecimal>(documents: documents);
            var subject = collection.AsQueryable().Select(d => d.X);

            var (queryable, terminator) = subject.WithTerminator(q => q.Average());

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $avg : '$X' } } }");
            AssertResult(queryable, terminator, 1.5M);
        }

        [Fact]
        public void Average_with_double_selector_should_return_expected_result()
        {
            var documents = new[] { "{ _id : 1, X : 1.0 }", "{ _id : 2, X : 2.0 }" };
            var collection = CreateCollection<DocumentWithDouble>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Average(d => d.X));

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $avg : '$X' } } }");
            AssertResult(queryable, terminator, 1.5);
        }

        [Fact]
        public void Average_with_double_should_return_expected_result()
        {
            var documents = new[] { "{ _id : 1, X : 1.0 }", "{ _id : 2, X : 2.0 }" };
            var collection = CreateCollection<DocumentWithDouble>(documents: documents);
            var subject = collection.AsQueryable().Select(d => d.X);

            var (queryable, terminator) = subject.WithTerminator(q => q.Average());

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $avg : '$X' } } }");
            AssertResult(queryable, terminator, 1.5);
        }

        [Fact]
        public void Average_with_float_selector_should_return_expected_result()
        {
            var documents = new[] { "{ _id : 1, X : 1.0 }", "{ _id : 2, X : 2.0 }" };
            var collection = CreateCollection<DocumentWithSingle>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Average(d => d.X));

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $avg : '$X' } } }");
            AssertResult(queryable, terminator, 1.5F);
        }

        [Fact]
        public void Average_with_float_should_return_expected_result()
        {
            var documents = new[] { "{ _id : 1, X : 1.0 }", "{ _id : 2, X : 2.0 }" };
            var collection = CreateCollection<DocumentWithSingle>(documents: documents);
            var subject = collection.AsQueryable().Select(d => d.X);

            var (queryable, terminator) = subject.WithTerminator(q => q.Average());

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $avg : '$X' } } }");
            AssertResult(queryable, terminator, 1.5F);
        }

        [Fact]
        public void Average_with_int_selector_should_return_expected_result()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Average(d => d.X));

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $avg : '$X' } } }");
            AssertResult(queryable, terminator, 1.5);
        }

        [Fact]
        public void Average_with_int_should_return_expected_result()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable().Select(d => d.X);

            var (queryable, terminator) = subject.WithTerminator(q => q.Average());

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $avg : '$X' } } }");
            AssertResult(queryable, terminator, 1.5);
        }

        [Fact]
        public void Average_with_long_selector_should_return_expected_result()
        {
            var documents = new[] { "{ _id : 1, X : { $numberLong : '1' } }", "{ _id : 2, X : { $numberLong : '2' } }" };
            var collection = CreateCollection<DocumentWithInt64>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Average(d => d.X));

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $avg : '$X' } } }");
            AssertResult(queryable, terminator, 1.5);
        }

        [Fact]
        public void Average_with_long_should_return_expected_result()
        {
            var documents = new[] { "{ _id : 1, X : { $numberLong : '1' } }", "{ _id : 2, X : { $numberLong : '2' } }" };
            var collection = CreateCollection<DocumentWithInt64>(documents: documents);
            var subject = collection.AsQueryable().Select(d => d.X);

            var (queryable, terminator) = subject.WithTerminator(q => q.Average());

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $avg : '$X' } } }");
            AssertResult(queryable, terminator, 1.5);
        }

        [Fact]
        public void Average_with_nullable_decimal_selector_should_return_expected_result()
        {
            var documents = new[] { "{ _id : 1, X : { $numberDecimal : '1.0' } }", "{ _id : 2, X : { $numberDecimal : '2.0' } }", "{ _id : 3, X : null }" };
            var collection = CreateCollection<DocumentWithNullableDecimal>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Average(d => d.X));

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $avg : '$X' } } }");
            AssertResult(queryable, terminator, 1.5M);
        }

        [Fact]
        public void Average_with_nullable_decimal_should_return_expected_result()
        {
            var documents = new[] { "{ _id : 1, X : { $numberDecimal : '1.0' } }", "{ _id : 2, X : { $numberDecimal : '2.0' } }", "{ _id : 3, X : null }" };
            var collection = CreateCollection<DocumentWithNullableDecimal>(documents: documents);
            var subject = collection.AsQueryable().Select(d => d.X);

            var (queryable, terminator) = subject.WithTerminator(q => q.Average());

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $avg : '$X' } } }");
            AssertResult(queryable, terminator, 1.5M);
        }

        [Fact]
        public void Average_with_nullable_double_selector_should_return_expected_result()
        {
            var documents = new[] { "{ _id : 1, X : 1.0 }", "{ _id : 2, X : 2.0 }", "{ _id : 3, X : null }" };
            var collection = CreateCollection<DocumentWithNullableDouble>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Average(d => d.X));

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $avg : '$X' } } }");
            AssertResult(queryable, terminator, 1.5);
        }

        [Fact]
        public void Average_with_nullable_double_should_return_expected_result()
        {
            var documents = new[] { "{ _id : 1, X : 1.0 }", "{ _id : 2, X : 2.0 }", "{ _id : 3, X : null }" };
            var collection = CreateCollection<DocumentWithNullableDouble>(documents: documents);
            var subject = collection.AsQueryable().Select(d => d.X);

            var (queryable, terminator) = subject.WithTerminator(q => q.Average());

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $avg : '$X' } } }");
            AssertResult(queryable, terminator, 1.5);
        }

        [Fact]
        public void Average_with_nullable_float_selector_should_return_expected_result()
        {
            var documents = new[] { "{ _id : 1, X : 1.0 }", "{ _id : 2, X : 2.0 }", "{ _id : 3, X : null }" };
            var collection = CreateCollection<DocumentWithNullableSingle>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Average(d => d.X));

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $avg : '$X' } } }");
            AssertResult(queryable, terminator, 1.5F);
        }

        [Fact]
        public void Average_with_nullable_float_should_return_expected_result()
        {
            var documents = new[] { "{ _id : 1, X : 1.0 }", "{ _id : 2, X : 2.0 }", "{ _id : 3, X : null }" };
            var collection = CreateCollection<DocumentWithNullableSingle>(documents: documents);
            var subject = collection.AsQueryable().Select(d => d.X);

            var (queryable, terminator) = subject.WithTerminator(q => q.Average());

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $avg : '$X' } } }");
            AssertResult(queryable, terminator, 1.5F);
        }

        [Fact]
        public void Average_with_nullable_int_selector_should_return_expected_result()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }", "{ _id : 3, X : null }" };
            var collection = CreateCollection<DocumentWithNullableInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Average(d => d.X));

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $avg : '$X' } } }");
            AssertResult(queryable, terminator, 1.5);
        }

        [Fact]
        public void Average_with_nullable_int_should_return_expected_result()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }", "{ _id : 3, X : null }" };
            var collection = CreateCollection<DocumentWithNullableInt32>(documents: documents);
            var subject = collection.AsQueryable().Select(d => d.X);

            var (queryable, terminator) = subject.WithTerminator(q => q.Average());

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $avg : '$X' } } }");
            AssertResult(queryable, terminator, 1.5);
        }

        [Fact]
        public void Average_with_nullable_long_selector_should_return_expected_result()
        {
            var documents = new[] { "{ _id : 1, X : { $numberLong : '1' } }", "{ _id : 2, X : { $numberLong : '2' } }", "{ _id : 3, X : null }" };
            var collection = CreateCollection<DocumentWithNullableInt64>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Average(d => d.X));

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $avg : '$X' } } }");
            AssertResult(queryable, terminator, 1.5);
        }

        [Fact]
        public void Average_with_nullable_long_should_return_expected_result()
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
        public void Count_should_translate_to_group_stage()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Count());

            AssertStages(queryable, terminator, "{ $group : { _id : 1, __result : { $sum : 1 } } }");
            AssertResult(queryable, terminator, 2);
        }

        [Fact]
        public void Count_with_predicate_should_translate_to_match_and_group_stages()
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
        public void Distinct_should_translate_to_group_stage()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }", "{ _id : 3, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable().Select(d => d.X);

            var queryable = subject.Distinct();

            AssertStages(queryable, "{ $group : { _id : '$X' } }");
            AssertResults(queryable, new[] { 1, 2 });
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
        public void Except_wit_comparer_is_not_supported()
        {
            var collection = CreateCollection<DocumentWithInt32>();
            var subject = collection.AsQueryable();
            var source2 = new[] { new DocumentWithInt32() };
            var comparer = Mock.Of<IEqualityComparer<DocumentWithInt32>>();

            var queryable = subject.Except(source2, comparer);

            AssertNotSupported(queryable);
        }

        [Fact]
        public void Where_should_translate_to_match_stage()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.Where(d => d.X == 1);

            AssertStages(queryable, "{ $match : { X : 1 } }");
            AssertResultIds(queryable, 1);
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
