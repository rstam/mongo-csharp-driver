using System;
using System.Linq;
using Linq2.Survey.Tests.Classes;
using MongoDB.Driver;
using Xunit;

namespace Linq2.Survey.Tests.MqlSurvey
{
    public class AggregationOperatorTests : LinqSurveyTest
    {
        [Fact]
        public void Abs_operator_with_decimal_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : { $numberDecimal : '1.0' } }", "{ _id : 2, X : { $numberDecimal : '-2.0' } }" };
            var collection = CreateCollection<DocumentWithDecimal>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.Select(d => Math.Abs(d.X));

            AssertStages(queryable, "{ $project : { __fld0 : { $abs : '$X' }, _id : 0 } }");
            AssertResults(queryable, 1.0M, 2.0M);
        }

        [Fact]
        public void Abs_operator_with_float_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : { $numberDouble : '1.0' } }", "{ _id : 2, X : { $numberDouble : '-2.0' } }" };
            var collection = CreateCollection<DocumentWithSingle>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.Select(d => Math.Abs(d.X));

            AssertStages(queryable, "{ $project : { __fld0 : { $abs : '$X' }, _id : 0 } }");
            AssertResults(queryable, 1.0F, 2.0F);
        }

        [Fact]
        public void Abs_operator_with_double_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : { $numberDouble : '1.0' } }", "{ _id : 2, X : { $numberDouble : '-2.0' } }" };
            var collection = CreateCollection<DocumentWithDouble>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.Select(d => Math.Abs(d.X));

            AssertStages(queryable, "{ $project : { __fld0 : { $abs : '$X' }, _id : 0 } }");
            AssertResults(queryable, 1.0, 2.0);
        }

        [Fact]
        public void Abs_operator_with_int_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : -2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.Select(d => Math.Abs(d.X));

            AssertStages(queryable, "{ $project : { __fld0 : { $abs : '$X' }, _id : 0 } }");
            AssertResults(queryable, 1, 2);
        }

        [Fact]
        public void Abs_operator_with_long_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : { $numberLong : '1' } }", "{ _id : 2, X : { $numberLong : '-2' } }" };
            var collection = CreateCollection<DocumentWithInt64>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.Select(d => Math.Abs(d.X));

            AssertStages(queryable, "{ $project : { __fld0 : { $abs : '$X' }, _id : 0 } }");
            AssertResults(queryable, 1, 2);
        }

        [Fact]
        public void Abs_operator_with_nullable_decimal_is_not_supported()
        {
            // Math.Abs does not have an overload for nullable decimal
        }

        [Fact]
        public void Abs_operator_with_nullable_double_is_not_supported()
        {
            // Math.Abs does not have an overload for nullable double
        }

        [Fact]
        public void Abs_operator_with_nullable_float_is_not_supported()
        {
            // Math.Abs does not have an overload for nullable float
        }

        [Fact]
        public void Abs_operator_with_nullable_int_is_not_supported()
        {
            // Math.Abs does not have an overload for nullable int
        }

        [Fact]
        public void Abs_operator_with_nullable_long_is_not_supported()
        {
            // Math.Abs does not have an overload for nullable long
        }

        [Fact]
        public void Accumulator_operator_is_not_supported()
        {
            // no LINQ equivalent
        }

        [Fact]
        public void Acos_is_not_supported()
        {
        }

        [Fact]
        public void Acosh_is_not_supported()
        {
            // Math.Abs does not have an Acosh method
        }

        [Fact]
        public void Add_operator_with_decimal_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : { $numberDecimal : '1.0' } }", "{ _id : 2, X : { $numberDecimal : '2.0' } }" };
            var collection = CreateCollection<DocumentWithDecimal>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.Select(d => d.X + 1.0M);

            AssertStages(queryable, "{ $project : { __fld0 : { $add : ['$X', { $numberDecimal : '1.0' }] }, _id : 0 } }");
            AssertResults(queryable, 2.0M, 3.0M);
        }

        [Fact]
        public void Add_operator_with_float_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : { $numberDouble : '1.0' } }", "{ _id : 2, X : { $numberDouble : '2.0' } }" };
            var collection = CreateCollection<DocumentWithSingle>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.Select(d => d.X + 1.0F);

            AssertStages(queryable, "{ $project : { __fld0 : { $add : ['$X', { $numberDouble : '1.0' }] }, _id : 0 } }");
            AssertResults(queryable, 2.0F, 3.0F);
        }

        [Fact]
        public void Add_operator_with_double_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : { $numberDouble : '1.0' } }", "{ _id : 2, X : { $numberDouble : '2.0' } }" };
            var collection = CreateCollection<DocumentWithDouble>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.Select(d => d.X + 1.0);

            AssertStages(queryable, "{ $project : { __fld0 : { $add : ['$X', { $numberDouble : '1.0' }] }, _id : 0 } }");
            AssertResults(queryable, 2.0, 3.0);
        }

        [Fact]
        public void Add_operator_with_int_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.Select(d => d.X + 1);

            AssertStages(queryable, "{ $project : { __fld0 : { $add : ['$X', 1] }, _id : 0 } }");
            AssertResults(queryable, 2, 3);
        }

        [Fact]
        public void Add_operator_with_long_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : { $numberLong : '1' } }", "{ _id : 2, X : { $numberLong : '2' } }" };
            var collection = CreateCollection<DocumentWithInt64>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.Select(d => d.X + 1);

            AssertStages(queryable, "{ $project : { __fld0 : { $add : ['$X', { $numberLong : '1' }] }, _id : 0 } }");
            AssertResults(queryable, 2, 3);
        }

        [Fact]
        public void AddToSet_is_supported_but_has_bugs()
        {
            var documents = new[] { "{ _id : 1, A : [1, 2, 2, 3] }" };
            var collection = CreateCollection<DocumentWithInt32Array>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.GroupBy(d => d.Id, (k, e) => e.Select(d => d.A).Distinct());

            AssertStages(queryable, "{ $group : { $addToSet : '$A', _id : 0 } }"); // bug: invalid stage
        }

        [Fact]
        public void AllElementsTrue_is_supported()
        {
            var documents = new[] { "{ _id : 1, A : [1, 2, 2, 3] }" };
            var collection = CreateCollection<DocumentWithInt32Array>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.Select(d => d.A.All(e => e > 0));

            AssertStages(queryable, "{ $project : { __fld0 : { $allElementsTrue : { $map : { input : '$A', as : 'e', in : { $gt : ['$$e', 0] } } } }, _id : 0 } }");
            AssertResults(queryable, true);
        }

    }
}
