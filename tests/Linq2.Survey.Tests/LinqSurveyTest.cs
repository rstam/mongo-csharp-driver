using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FluentAssertions;
using Linq2.Survey.Tests.Classes;
using MongoDB.Bson;
using MongoDB.Bson.TestHelpers;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Driver.Linq.Expressions;
using MongoDB.Driver.Tests;
using Xunit;

namespace Linq2.Survey.Tests.LinqSurvey
{
    public class LinqSurveyTest
    {
        // public methods
        public void AssertNotSupported<TSource>(
            IQueryable<TSource> queryable)
        {
            var exception = Record.Exception(() => queryable.ToList());

            exception.Should().BeOfType<NotSupportedException>();
        }

        public void AssertNotSupported<TSource, TResult>(
            IQueryable<TSource> queryable,
            Expression<Func<IQueryable<TSource>, TResult>> terminator)
        {
            var provider = queryable.Provider;
            var expression = CreateExpression(queryable, terminator);

            var exception = Record.Exception(() => provider.Execute(expression));

            exception.Should().BeOfType<NotSupportedException>();
        }

        public void AssertResult<TSource, TResult>(
            IQueryable<TSource> queryable,
            Expression<Func<IQueryable<TSource>, TResult>> terminator,
            TResult expectedResult)
        {
            var provider = queryable.Provider;
            var expression = CreateExpression(queryable, terminator);
            var result = (TResult)provider.Execute(expression);
            result.Should().Be(expectedResult);
        }

        public void AssertResultId<TSource, TResult, TId>(
            IQueryable<TSource> queryable,
            Expression<Func<IQueryable<TSource>, TResult>> terminator,
            TId expectedId)
            where TResult: IHasId<TId>
        {
            var provider = queryable.Provider;
            var expression = CreateExpression(queryable, terminator);
            var result = (TResult)provider.Execute(expression);
            result.Id.Should().Be(expectedId);
        }

        public void AssertResultIds<TResult, TId>(IQueryable<TResult> queryable, params TId[] expectedIds)
            where TResult : IHasId<TId>
        {
            AssertResults(queryable, r => r.Id, expectedIds);
        }

        public void AssertResults<TResult>(
            IQueryable<TResult> queryable,
            params TResult[] expectedResults)
        {
            var results = queryable.ToList();
            results.Should().Equal(expectedResults);
        }

        public void AssertResults<TResult>(
            IQueryable<TResult> queryable,
            IEnumerable<TResult> expectedResults)
        {
            var results = queryable.ToList();
            results.Should().Equal(expectedResults);
        }

        public void AssertResults<TResult, TTransformed>(
            IQueryable<TResult> queryable,
            Func<TResult, TTransformed> transformer,
            params TTransformed[] expectedValues)
        {
            var results = queryable.ToList();
            var values = results.Select(x => transformer(x)).ToList();
            values.Should().Equal(expectedValues);
        }

        public void AssertStages<TResult>(
            IQueryable<TResult> queryable,
            params string[] expectedStages)
        {
            var stages = GetStages(queryable);
            stages.Should().Equal(Parse(expectedStages));
        }

        public void AssertStages<TSource, TResult>(
            IQueryable<TSource> queryable,
            Expression<Func<IQueryable<TSource>, TResult>> terminator,
            params string[] expectedStages)
        {
            var stages = GetStages(queryable, terminator);
            stages.Should().Equal(Parse(expectedStages));
        }

        public IMongoCollection<TDocument> CreateCollection<TDocument>(
            string databaseName = "test",
            string collectionName = "test",
            string[] documents = null)
        {
            var client = DriverTestConfiguration.Client;
            var database = client.GetDatabase(databaseName);
            var collection = database.GetCollection<TDocument>(collectionName);
            database.DropCollection(collectionName);

            if (documents != null)
            {
                var bsonDocumentCollection = database.GetCollection<BsonDocument>(collectionName);
                foreach (var document in documents)
                {
                    bsonDocumentCollection.InsertOne(BsonDocument.Parse(document));
                }
            }

            return collection;
        }

        // protected methods
        protected IEnumerable<BsonDocument> Parse(IEnumerable<string> documents)
        {
            return documents.Select(x => BsonDocument.Parse(x));
        }

        // private methods
        private Expression CreateExpression<TSource, TResult>(
            IQueryable<TSource> queryable,
            Expression<Func<IQueryable<TSource>, TResult>> terminator)
        {
            var parameter = terminator.Parameters.Single();
            var body = terminator.Body;
            return ExpressionReplacer.Replace(body, parameter, queryable.Expression);
        }

        private List<BsonDocument> GetStages<TResult>(
            IQueryable<TResult> queryable)
        {
            var mongoQueryable = (IMongoQueryable)queryable;
            var model = (AggregateQueryableExecutionModel<TResult>)mongoQueryable.GetExecutionModel();
            return model.Stages.ToList();
        }

        public List<BsonDocument> GetStages<TSource, TResult>(
            IQueryable<TSource> queryable,
            Expression<Func<IQueryable<TSource>, TResult>> terminator)
        {
            var provider = (IMongoQueryProvider)queryable.Provider;
            var expression = CreateExpression(queryable, terminator);
            var model = provider.GetExecutionModel(expression);
            var stages = (IEnumerable<BsonDocument>)Reflector.GetPropertyValue(model, "Stages", BindingFlags.Public | BindingFlags.Instance);
            return stages.ToList();
        }
    }
}
