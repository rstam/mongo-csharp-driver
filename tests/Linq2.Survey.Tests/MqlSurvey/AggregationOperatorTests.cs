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
        public void Acos_operator_is_not_supported()
        {
        }

        [Fact]
        public void Acosh_operator_is_not_supported()
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
        public void AddToSet_operator_is_supported_but_has_bugs()
        {
            var documents = new[] { "{ _id : 1, A : [1, 2, 2, 3] }" };
            var collection = CreateCollection<DocumentWithInt32Array>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.GroupBy(d => d.Id, (k, e) => e.Select(d => d.A).Distinct());

            AssertStages(queryable, "{ $group : { $addToSet : '$A', _id : 0 } }"); // bug: invalid stage
        }

        [Fact]
        public void AllElementsTrue_operator_is_supported()
        {
            var documents = new[] { "{ _id : 1, A : [1, 2, 2, 3] }" };
            var collection = CreateCollection<DocumentWithInt32Array>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.Select(d => d.A.All(e => e > 0));

            AssertStages(queryable, "{ $project : { __fld0 : { $allElementsTrue : { $map : { input : '$A', as : 'e', in : { $gt : ['$$e', 0] } } } }, _id : 0 } }");
            AssertResults(queryable, true);
        }

        [Fact]
        public void And_operator_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.Select(d => d.Id == 1 && d.X == 1);

            AssertStages(queryable, "{ $project : { __fld0 : { $and : [{ $eq : ['$_id', 1] }, { $eq : ['$X', 1] }] }, _id : 0 } }");
            AssertResults(queryable, true, false);
        }

        [Fact]
        public void AnyElementsTrue_operator_is_supported()
        {
            var documents = new[] { "{ _id : 1, A : [1, 2, 2, 3] }" };
            var collection = CreateCollection<DocumentWithInt32Array>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.Select(d => d.A.Any(e => e > 0));

            AssertStages(queryable, "{ $project : { __fld0 : { $anyElementTrue : { $map : { input : '$A', as : 'e', in : { $gt : ['$$e', 0] } } } }, _id : 0 } }");
            AssertResults(queryable, true);
        }

        [Fact]
        public void ArrayElemAt_operator_is_supported()
        {
            var documents = new[] { "{ _id : 1, A : [1, 2, 3] }" };
            var collection = CreateCollection<DocumentWithInt32Array>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.Select(d => d.A[0]);

            AssertStages(queryable, "{ $project : { __fld0 : { $arrayElemAt : ['$A', 0] }, _id : 0 } }");
            AssertResults(queryable, 1);
        }

        [Fact]
        public void ArrayToObject_operator_is_not_supported()
        {
            // no LINQ equivalent
        }

        [Fact]
        public void Asin_operator_is_not_supported()
        {
        }

        [Fact]
        public void Asinh_operator_is_not_supported()
        {
            // Math.Abs does not have an Asinh method
        }


        [Fact]
        public void Atan_operator_is_not_supported()
        {
        }

        [Fact]
        public void Atan2_operator_is_not_supported()
        {
        }

        [Fact]
        public void Atanh_operator_is_not_supported()
        {
            // Math.Abs does not have an Acosh method
        }

        [Fact]
        public void Avg_operator_in_group_stage_is_supported_but_has_bugs()
        {
            var documents = new[] { "{ _id : 1, X : 1 }", "{ _id : 2, X : 1 }", "{ _id : 3, X : 3 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.GroupBy(d => d.X, (k, e) => e.Select(d => d.Id).Average());

            AssertStages(queryable, "{ $group : { $avg : '$_id', _id : 0 } }"); // bug: invalid stage
        }

        [Fact]
        public void Avg_operator_in_project_stage_is_supported()
        {
            var documents = new[] { "{ _id : 1, A : [1, 2, 3] }" };
            var collection = CreateCollection<DocumentWithInt32Array>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.Select(d => d.A.Average());

            AssertStages(queryable, "{ $project : { __fld0 : { $avg : '$A' }, _id : 0 } }");
            AssertResults(queryable, 2.0);
        }

        [Fact]
        public void BinarySize_operator_is_not_supported()
        {
            // no LINQ equivalent
        }

        [Fact]
        public void BsonSize_operator_is_not_supported()
        {
            // no LINQ equivalent
        }

        [Fact]
        public void Ceil_operator_with_decimal_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : { $numberDecimal : '1.5' } }", "{ _id : 2, X : { $numberDecimal : '2.0' } }" };
            var collection = CreateCollection<DocumentWithDecimal>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.Select(d => Math.Ceiling(d.X));

            AssertStages(queryable, "{ $project : { __fld0 : { $ceil : '$X' }, _id : 0 } }");
            AssertResults(queryable, 2.0M, 2.0M);
        }

        [Fact]
        public void Ceil_operator_with_double_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1.5 }", "{ _id : 2, X : 2.0 }" };
            var collection = CreateCollection<DocumentWithDouble>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.Select(d => Math.Ceiling(d.X));

            AssertStages(queryable, "{ $project : { __fld0 : { $ceil : '$X' }, _id : 0 } }");
            AssertResults(queryable, 2.0, 2.0);
        }

        [Fact]
        public void Ceil_operator_with_float_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 1.5 }", "{ _id : 2, X : 2.0 }" };
            var collection = CreateCollection<DocumentWithSingle>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.Select(d => Math.Ceiling(d.X));

            AssertStages(queryable, "{ $project : { __fld0 : { $ceil : '$X' }, _id : 0 } }");
            AssertResults(queryable, 2.0, 2.0);
        }

        [Fact]
        public void Ceil_operator_with_int_is_not_supported()
        {
            // no overload of Math.Ceiling for int
        }

        [Fact]
        public void Ceil_operator_with_long_is_not_supported()
        {
            // no overload of Math.Ceiling for long
        }

        [Fact]
        public void Ceil_operator_with_nullable_decimal_is_not_supported()
        {
            // no overload of Math.Ceiling for nullable decimal
        }

        [Fact]
        public void Ceil_operator_with_nullable_double_is_not_supported()
        {
            // no overload of Math.Ceiling for nullable double
        }

        [Fact]
        public void Ceil_operator_with_nullable_float_is_not_supported()
        {
            // no overload of Math.Ceiling for nullable float
        }

        [Fact]
        public void Ceil_operator_with_nullable_int_is_not_supported()
        {
            // no overload of Math.Ceiling for nullable int
        }

        [Fact]
        public void Ceil_operator_with_nullable_long_is_not_supported()
        {
            // no overload of Math.Ceiling for nullable long
        }

        [Fact]
        public void Cmp_operator_with_bson_document_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : { A : 1 }, Y : { A : 2 } }", "{ _id : 2, X : { A : 2 }, Y : { A : 2 } }", "{ _id : 3, X : { A : 3 }, Y : { A : 2 } }" };
            var collection = CreateCollection<DocumentWithTwoBsonDocuments>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.Select(d => d.X.CompareTo(d.Y));

            AssertStages(queryable, "{ $project : { __fld0 : { $cmp : ['$X', '$Y'] }, _id : 0 } }");
            AssertResults(queryable, -1, 0, 1);
        }

        [Fact]
        public void Cmp_operator_with_int_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 2 }", "{ _id : 2, X : 2 }", "{ _id : 3, X : 2 }" };
            var collection = CreateCollection<DocumentWithInt32>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.Select(d => d.Id.CompareTo(d.X));

            AssertStages(queryable, "{ $project : { __fld0 : { $cmp : ['$_id', '$X'] }, _id : 0 } }");
            AssertResults(queryable, -1, 0, 1);
        }

        [Fact]
        public void Concat_operator_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : 'abc', Y : 'def' }", "{ _id : 2, X : 'ghi', Y : null }" };
            var collection = CreateCollection<DocumentWithTwoStrings>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.Select(d => d.X + d.Y);

            AssertStages(queryable, "{ $project : { __fld0 : { $concat : ['$X', '$Y'] }, _id : 0 } }");
            AssertResults(queryable, "abcdef", null);
        }


        [Fact]
        public void ConcatArrays_operator_is_supported()
        {
            var documents = new[] { "{ _id : 1, X : [1, 2], Y : [3] }", "{ _id : 2, X : [1], Y : null }" };
            var collection = CreateCollection<DocumentWithTwoInt32Arrays>(documents: documents);
            var subject = collection.AsQueryable();

            var queryable = subject.Select(d => d.X.Concat(d.Y));

            AssertStages(queryable, "{ $project : { __fld0 : { $concatArrays : ['$X', '$Y'] }, _id : 0 } }");
            AssertEnumerableResults(queryable, new[] { 1, 2, 3 }, null);
        }
    }
}
