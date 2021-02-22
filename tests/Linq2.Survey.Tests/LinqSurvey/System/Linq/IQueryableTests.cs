using System;
using System.Linq;
using System.Linq.Expressions;
using Linq2.Survey.Tests.Classes;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Xunit;

namespace Linq2.Survey.Tests.LinqSurvey.System.Linq
{
    public class IQueryableTests : LinqSurveyTest
    {
        // public methods
        [Fact]
        public void Any_should_translate_to_limit_stage()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Any());

            AssertStages(queryable, terminator, "{ $limit : 1 }");
            AssertResult(queryable, terminator, true);
        }

        [Fact]
        public void Any_with_predicate_should_translate_to_match_and_limit_stages()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt>(documents: documents);
            var subject = collection.AsQueryable();

            var (queryable, terminator) = subject.WithTerminator(q => q.Any(d => d.X == 3));

            AssertStages(queryable, terminator, "{ $match : { X : 3 } }", "{ $limit : 1 }");
            AssertResult(queryable, terminator, false);
        }

        [Fact]
        public void Where_should_translate_to_match_stage()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt>(documents: documents);
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
