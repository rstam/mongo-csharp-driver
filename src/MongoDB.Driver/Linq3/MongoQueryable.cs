﻿/* Copyright 2010-present MongoDB Inc.
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Driver.Linq3
{
    // this class is analogous to .NET's Queryable class and contains MongoDB specific extension methods for IQueryable
    internal static class MongoQueryable
    {
        public static Task<bool> AnyAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<bool>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, CancellationToken, Task<bool>>(MongoQueryable.AnyAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<bool> AnyAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(predicate), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<bool>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, bool>>, CancellationToken, Task<bool>>(MongoQueryable.AnyAsync, source, predicate, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<decimal> AverageAsync(this IQueryable<decimal> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<decimal>(
                Expression.Call(
                    GetMethodInfo<IQueryable<decimal>, CancellationToken, Task<decimal>>(MongoQueryable.AverageAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<decimal?> AverageAsync(this IQueryable<decimal?> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<decimal?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<decimal?>, CancellationToken, Task<decimal?>>(MongoQueryable.AverageAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<double> AverageAsync(this IQueryable<double> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<double>(
                Expression.Call(
                    GetMethodInfo<IQueryable<double>, CancellationToken, Task<double>>(MongoQueryable.AverageAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<double?> AverageAsync(this IQueryable<double?> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<double?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<double?>, CancellationToken, Task<double?>>(MongoQueryable.AverageAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<float> AverageAsync(this IQueryable<float> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<float>(
                Expression.Call(
                    GetMethodInfo<IQueryable<float>, CancellationToken, Task<float>>(MongoQueryable.AverageAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<float?> AverageAsync(this IQueryable<float?> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<float?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<float?>, CancellationToken, Task<float?>>(MongoQueryable.AverageAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<double> AverageAsync(this IQueryable<int> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<double>(
                Expression.Call(
                    GetMethodInfo<IQueryable<int>, CancellationToken, Task<double>>(MongoQueryable.AverageAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<double?> AverageAsync(this IQueryable<int?> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<double?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<int?>, CancellationToken, Task<double?>>(MongoQueryable.AverageAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<double> AverageAsync(this IQueryable<long> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<double>(
                Expression.Call(
                    GetMethodInfo<IQueryable<long>, CancellationToken, Task<double>>(MongoQueryable.AverageAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<double?> AverageAsync(this IQueryable<long?> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<double?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<long?>, CancellationToken, Task<double?>>(MongoQueryable.AverageAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<decimal> AverageAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, decimal>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<decimal>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, decimal>>, CancellationToken, Task<decimal>>(MongoQueryable.AverageAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<decimal?> AverageAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, decimal?>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<decimal?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, decimal?>>, CancellationToken, Task<decimal?>>(MongoQueryable.AverageAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<double> AverageAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, double>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<double>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, double>>, CancellationToken, Task<double>>(MongoQueryable.AverageAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<double?> AverageAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, double?>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<double?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, double?>>, CancellationToken, Task<double?>>(MongoQueryable.AverageAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<float> AverageAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, float>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<float>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, float>>, CancellationToken, Task<float>>(MongoQueryable.AverageAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<float?> AverageAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, float?>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<float?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, float?>>, CancellationToken, Task<float?>>(MongoQueryable.AverageAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<double> AverageAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, int>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<double>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, int>>, CancellationToken, Task<double>>(MongoQueryable.AverageAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<double?> AverageAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, int?>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<double?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, int?>>, CancellationToken, Task<double?>>(MongoQueryable.AverageAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<double> AverageAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, long>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<double>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, long>>, CancellationToken, Task<double>>(MongoQueryable.AverageAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<double?> AverageAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, long?>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<double?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, long?>>, CancellationToken, Task<double?>>(MongoQueryable.AverageAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<int> CountAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<int>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, CancellationToken, Task<int>>(MongoQueryable.CountAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<int> CountAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate , CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(predicate), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<int>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, bool>>, CancellationToken, Task<int>>(MongoQueryable.CountAsync, source, predicate, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<TSource> FirstAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<TSource>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, CancellationToken, Task<TSource>>(MongoQueryable.FirstAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<TSource> FirstAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(predicate), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<TSource>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, bool>>, CancellationToken, Task<TSource>>(MongoQueryable.FirstAsync, source, predicate, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<TSource>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, CancellationToken, Task<TSource>>(MongoQueryable.FirstOrDefaultAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(predicate), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<TSource>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, bool>>, CancellationToken, Task<TSource>>(MongoQueryable.FirstOrDefaultAsync, source, predicate, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static IQueryable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IQueryable<TOuter> outer, IMongoCollection<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, IEnumerable<TInner>, TResult>> resultSelector)
        {
            return outer.GroupJoin(inner.AsQueryable3(), outerKeySelector, innerKeySelector, resultSelector);
        }

        public static Task<long> LongCountAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<long>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, CancellationToken, Task<long>>(MongoQueryable.LongCountAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<long> LongCountAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(predicate), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<long>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, bool>>, CancellationToken, Task<long>>(MongoQueryable.LongCountAsync, source, predicate, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<TSource> MaxAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<TSource>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, CancellationToken, Task<TSource>>(MongoQueryable.MaxAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<TResult> MaxAsync<TSource, TResult>(this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<TResult>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, TResult>>, CancellationToken, Task<TResult>>(MongoQueryable.MaxAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<TSource> MinAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<TSource>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, CancellationToken, Task<TSource>>(MongoQueryable.MinAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<TResult> MinAsync<TSource, TResult>(this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<TResult>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, TResult>>, CancellationToken, Task<TResult>>(MongoQueryable.MinAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static IQueryable<TSource> Sample<TSource>(this IQueryable<TSource> source, long size)
        {
            Expression[] arguments = new[] { source.Expression, Expression.Constant(size) };
            return source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, long, IQueryable<TSource>>(new Func<IQueryable<TSource>, long, IQueryable<TSource>>(MongoQueryable.Sample), source, size),
                    arguments));
        }

        public static Task<TSource> SingleAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<TSource>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, CancellationToken, Task<TSource>>(MongoQueryable.SingleAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<TSource> SingleAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(predicate), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<TSource>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, bool>>, CancellationToken, Task<TSource>>(MongoQueryable.SingleAsync, source, predicate, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<TSource>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, CancellationToken, Task<TSource>>(MongoQueryable.SingleOrDefaultAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(predicate), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<TSource>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, bool>>, CancellationToken, Task<TSource>>(MongoQueryable.SingleOrDefaultAsync, source, predicate, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static decimal StandardDeviationPopulation(this IQueryable<decimal> source)
        {
            var arguments = new[] { source.Expression };
            return ((MongoQueryProvider)source.Provider).Execute<decimal>(
                Expression.Call(
                    GetMethodInfo<IQueryable<decimal>, decimal>(MongoQueryable.StandardDeviationPopulation, source),
                    arguments));
        }

        public static decimal? StandardDeviationPopulation(this IQueryable<decimal?> source)
        {
            var arguments = new[] { source.Expression };
            return ((MongoQueryProvider)source.Provider).Execute<decimal?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<decimal?>, decimal?>(MongoQueryable.StandardDeviationPopulation, source),
                    arguments));
        }

        public static double StandardDeviationPopulation(this IQueryable<double> source)
        {
            var arguments = new[] { source.Expression };
            return ((MongoQueryProvider)source.Provider).Execute<double>(
                Expression.Call(
                    GetMethodInfo<IQueryable<double>, double>(MongoQueryable.StandardDeviationPopulation, source),
                    arguments));
        }

        public static double? StandardDeviationPopulation(this IQueryable<double?> source)
        {
            var arguments = new[] { source.Expression };
            return ((MongoQueryProvider)source.Provider).Execute<double?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<double?>, double?>(MongoQueryable.StandardDeviationPopulation, source),
                    arguments));
        }

        public static float StandardDeviationPopulation(this IQueryable<float> source)
        {
            var arguments = new[] { source.Expression };
            return ((MongoQueryProvider)source.Provider).Execute<float>(
                Expression.Call(
                    GetMethodInfo<IQueryable<float>, float>(MongoQueryable.StandardDeviationPopulation, source),
                    arguments));
        }

        public static float? StandardDeviationPopulation(this IQueryable<float?> source)
        {
            var arguments = new[] { source.Expression };
            return ((MongoQueryProvider)source.Provider).Execute<float?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<float?>, float?>(MongoQueryable.StandardDeviationPopulation, source),
                    arguments));
        }

        public static double StandardDeviationPopulation(this IQueryable<int> source)
        {
            var arguments = new[] { source.Expression };
            return ((MongoQueryProvider)source.Provider).Execute<double>(
                Expression.Call(
                    GetMethodInfo<IQueryable<int>, double>(MongoQueryable.StandardDeviationPopulation, source),
                    arguments));
        }

        public static double? StandardDeviationPopulation(this IQueryable<int?> source)
        {
            var arguments = new[] { source.Expression };
            return ((MongoQueryProvider)source.Provider).Execute<double?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<int?>, double?>(MongoQueryable.StandardDeviationPopulation, source),
                    arguments));
        }

        public static double StandardDeviationPopulation(this IQueryable<long> source)
        {
            var arguments = new[] { source.Expression };
            return ((MongoQueryProvider)source.Provider).Execute<double>(
                Expression.Call(
                    GetMethodInfo<IQueryable<long>, double>(MongoQueryable.StandardDeviationPopulation, source),
                    arguments));
        }

        public static double? StandardDeviationPopulation(this IQueryable<long?> source)
        {
            var arguments = new[] { source.Expression };
            return ((MongoQueryProvider)source.Provider).Execute<double?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<long?>, double?>(MongoQueryable.StandardDeviationPopulation, source),
                    arguments));
        }

        public static decimal StandardDeviationPopulation<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, decimal>> selector)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector) };
            return ((MongoQueryProvider)source.Provider).Execute<decimal>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, decimal>>, decimal>(MongoQueryable.StandardDeviationPopulation, source, selector),
                    arguments));
        }

        public static decimal? StandardDeviationPopulation<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, decimal?>> selector)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector) };
            return ((MongoQueryProvider)source.Provider).Execute<decimal?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, decimal?>>, decimal?>(MongoQueryable.StandardDeviationPopulation, source, selector),
                    arguments));
        }

        public static double StandardDeviationPopulation<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, double>> selector)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector) };
            return ((MongoQueryProvider)source.Provider).Execute<double>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, double>>, double>(MongoQueryable.StandardDeviationPopulation, source, selector),
                    arguments));
        }

        public static double? StandardDeviationPopulation<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, double?>> selector)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector) };
            return ((MongoQueryProvider)source.Provider).Execute<double?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, double?>>, double?>(MongoQueryable.StandardDeviationPopulation, source, selector),
                    arguments));
        }

        public static float StandardDeviationPopulation<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, float>> selector)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector) };
            return ((MongoQueryProvider)source.Provider).Execute<float>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, float>>, float>(MongoQueryable.StandardDeviationPopulation, source, selector),
                    arguments));
        }

        public static float? StandardDeviationPopulation<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, float?>> selector)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector) };
            return ((MongoQueryProvider)source.Provider).Execute<float?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, float?>>, float?>(MongoQueryable.StandardDeviationPopulation, source, selector),
                    arguments));
        }

        public static double StandardDeviationPopulation<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, int>> selector)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector) };
            return ((MongoQueryProvider)source.Provider).Execute<double>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, int>>, double>(MongoQueryable.StandardDeviationPopulation, source, selector),
                    arguments));
        }

        public static double? StandardDeviationPopulation<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, int?>> selector)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector) };
            return ((MongoQueryProvider)source.Provider).Execute<double?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, int?>>, double?>(MongoQueryable.StandardDeviationPopulation, source, selector),
                    arguments));
        }

        public static double StandardDeviationPopulation<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, long>> selector)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector) };
            return ((MongoQueryProvider)source.Provider).Execute<double>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, long>>, double>(MongoQueryable.StandardDeviationPopulation, source, selector),
                    arguments));
        }

        public static double? StandardDeviationPopulation<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, long?>> selector)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector) };
            return ((MongoQueryProvider)source.Provider).Execute<double?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, long?>>, double?>(MongoQueryable.StandardDeviationPopulation, source, selector),
                    arguments));
        }

        public static Task<decimal> StandardDeviationPopulationAsync(this IQueryable<decimal> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<decimal>(
                Expression.Call(
                    GetMethodInfo<IQueryable<decimal>, CancellationToken, Task<decimal>>(MongoQueryable.StandardDeviationPopulationAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<decimal?> StandardDeviationPopulationAsync(this IQueryable<decimal?> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<decimal?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<decimal?>, CancellationToken, Task<decimal?>>(MongoQueryable.StandardDeviationPopulationAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<double> StandardDeviationPopulationAsync(this IQueryable<double> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<double>(
                Expression.Call(
                    GetMethodInfo<IQueryable<double>, CancellationToken, Task<double>>(MongoQueryable.StandardDeviationPopulationAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<double?> StandardDeviationPopulationAsync(this IQueryable<double?> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<double?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<double?>, CancellationToken, Task<double?>>(MongoQueryable.StandardDeviationPopulationAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<float> StandardDeviationPopulationAsync(this IQueryable<float> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<float>(
                Expression.Call(
                    GetMethodInfo<IQueryable<float>, CancellationToken, Task<float>>(MongoQueryable.StandardDeviationPopulationAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<float?> StandardDeviationPopulationAsync(this IQueryable<float?> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<float?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<float?>, CancellationToken, Task<float?>>(MongoQueryable.StandardDeviationPopulationAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<double> StandardDeviationPopulationAsync(this IQueryable<int> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<double>(
                Expression.Call(
                    GetMethodInfo<IQueryable<int>, CancellationToken, Task<double>>(MongoQueryable.StandardDeviationPopulationAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<double?> StandardDeviationPopulationAsync(this IQueryable<int?> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<double?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<int?>, CancellationToken, Task<double?>>(MongoQueryable.StandardDeviationPopulationAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<double> StandardDeviationPopulationAsync(this IQueryable<long> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<double>(
                Expression.Call(
                    GetMethodInfo<IQueryable<long>, CancellationToken, Task<double>>(MongoQueryable.StandardDeviationPopulationAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<double?> StandardDeviationPopulationAsync(this IQueryable<long?> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<double?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<long?>, CancellationToken, Task<double?>>(MongoQueryable.StandardDeviationPopulationAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<decimal> StandardDeviationPopulationAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, decimal>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<decimal>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, decimal>>, CancellationToken, Task<decimal>>(MongoQueryable.StandardDeviationPopulationAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<decimal?> StandardDeviationPopulationAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, decimal?>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<decimal?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, decimal?>>, CancellationToken, Task<decimal?>>(MongoQueryable.StandardDeviationPopulationAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<double> StandardDeviationPopulationAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, double>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<double>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, double>>, CancellationToken, Task<double>>(MongoQueryable.StandardDeviationPopulationAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<double?> StandardDeviationPopulationAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, double?>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<double?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, double?>>, CancellationToken, Task<double?>>(MongoQueryable.StandardDeviationPopulationAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<float> StandardDeviationPopulationAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, float>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<float>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, float>>, CancellationToken, Task<float>>(MongoQueryable.StandardDeviationPopulationAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<float?> StandardDeviationPopulationAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, float?>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<float?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, float?>>, CancellationToken, Task<float?>>(MongoQueryable.StandardDeviationPopulationAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<double> StandardDeviationPopulationAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, int>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<double>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, int>>, CancellationToken, Task<double>>(MongoQueryable.StandardDeviationPopulationAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<double?> StandardDeviationPopulationAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, int?>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<double?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, int?>>, CancellationToken, Task<double?>>(MongoQueryable.StandardDeviationPopulationAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<double> StandardDeviationPopulationAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, long>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<double>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, long>>, CancellationToken, Task<double>>(MongoQueryable.StandardDeviationPopulationAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<double?> StandardDeviationPopulationAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, long?>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<double?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, long?>>, CancellationToken, Task<double?>>(MongoQueryable.StandardDeviationPopulationAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static decimal StandardDeviationSample(this IQueryable<decimal> source)
        {
            var arguments = new[] { source.Expression };
            return ((MongoQueryProvider)source.Provider).Execute<decimal>(
                Expression.Call(
                    GetMethodInfo<IQueryable<decimal>, decimal>(MongoQueryable.StandardDeviationSample, source),
                    arguments));
        }

        public static decimal? StandardDeviationSample(this IQueryable<decimal?> source)
        {
            var arguments = new[] { source.Expression };
            return ((MongoQueryProvider)source.Provider).Execute<decimal?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<decimal?>, decimal?>(MongoQueryable.StandardDeviationSample, source),
                    arguments));
        }

        public static double StandardDeviationSample(this IQueryable<double> source)
        {
            var arguments = new[] { source.Expression };
            return ((MongoQueryProvider)source.Provider).Execute<double>(
                Expression.Call(
                    GetMethodInfo<IQueryable<double>, double>(MongoQueryable.StandardDeviationSample, source),
                    arguments));
        }

        public static double? StandardDeviationSample(this IQueryable<double?> source)
        {
            var arguments = new[] { source.Expression };
            return ((MongoQueryProvider)source.Provider).Execute<double?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<double?>, double?>(MongoQueryable.StandardDeviationSample, source),
                    arguments));
        }

        public static float StandardDeviationSample(this IQueryable<float> source)
        {
            var arguments = new[] { source.Expression };
            return ((MongoQueryProvider)source.Provider).Execute<float>(
                Expression.Call(
                    GetMethodInfo<IQueryable<float>, float>(MongoQueryable.StandardDeviationSample, source),
                    arguments));
        }

        public static float? StandardDeviationSample(this IQueryable<float?> source)
        {
            var arguments = new[] { source.Expression };
            return ((MongoQueryProvider)source.Provider).Execute<float?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<float?>, float?>(MongoQueryable.StandardDeviationSample, source),
                    arguments));
        }

        public static double StandardDeviationSample(this IQueryable<int> source)
        {
            var arguments = new[] { source.Expression };
            return ((MongoQueryProvider)source.Provider).Execute<double>(
                Expression.Call(
                    GetMethodInfo<IQueryable<int>, double>(MongoQueryable.StandardDeviationSample, source),
                    arguments));
        }

        public static double? StandardDeviationSample(this IQueryable<int?> source)
        {
            var arguments = new[] { source.Expression };
            return ((MongoQueryProvider)source.Provider).Execute<double?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<int?>, double?>(MongoQueryable.StandardDeviationSample, source),
                    arguments));
        }

        public static double StandardDeviationSample(this IQueryable<long> source)
        {
            var arguments = new[] { source.Expression };
            return ((MongoQueryProvider)source.Provider).Execute<double>(
                Expression.Call(
                    GetMethodInfo<IQueryable<long>, double>(MongoQueryable.StandardDeviationSample, source),
                    arguments));
        }

        public static double? StandardDeviationSample(this IQueryable<long?> source)
        {
            var arguments = new[] { source.Expression };
            return ((MongoQueryProvider)source.Provider).Execute<double?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<long?>, double?>(MongoQueryable.StandardDeviationSample, source),
                    arguments));
        }

        public static decimal StandardDeviationSample<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, decimal>> selector)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector) };
            return ((MongoQueryProvider)source.Provider).Execute<decimal>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, decimal>>, decimal>(MongoQueryable.StandardDeviationSample, source, selector),
                    arguments));
        }

        public static decimal? StandardDeviationSample<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, decimal?>> selector)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector) };
            return ((MongoQueryProvider)source.Provider).Execute<decimal?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, decimal?>>, decimal?>(MongoQueryable.StandardDeviationSample, source, selector),
                    arguments));
        }

        public static double StandardDeviationSample<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, double>> selector)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector) };
            return ((MongoQueryProvider)source.Provider).Execute<double>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, double>>, double>(MongoQueryable.StandardDeviationSample, source, selector),
                    arguments));
        }

        public static double? StandardDeviationSample<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, double?>> selector)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector) };
            return ((MongoQueryProvider)source.Provider).Execute<double?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, double?>>, double?>(MongoQueryable.StandardDeviationSample, source, selector),
                    arguments));
        }

        public static float StandardDeviationSample<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, float>> selector)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector) };
            return ((MongoQueryProvider)source.Provider).Execute<float>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, float>>, float>(MongoQueryable.StandardDeviationSample, source, selector),
                    arguments));
        }

        public static float? StandardDeviationSample<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, float?>> selector)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector) };
            return ((MongoQueryProvider)source.Provider).Execute<float?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, float?>>, float?>(MongoQueryable.StandardDeviationSample, source, selector),
                    arguments));
        }

        public static double StandardDeviationSample<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, int>> selector)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector) };
            return ((MongoQueryProvider)source.Provider).Execute<double>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, int>>, double>(MongoQueryable.StandardDeviationSample, source, selector),
                    arguments));
        }

        public static double? StandardDeviationSample<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, int?>> selector)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector) };
            return ((MongoQueryProvider)source.Provider).Execute<double?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, int?>>, double?>(MongoQueryable.StandardDeviationSample, source, selector),
                    arguments));
        }

        public static double StandardDeviationSample<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, long>> selector)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector) };
            return ((MongoQueryProvider)source.Provider).Execute<double>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, long>>, double>(MongoQueryable.StandardDeviationSample, source, selector),
                    arguments));
        }

        public static double? StandardDeviationSample<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, long?>> selector)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector) };
            return ((MongoQueryProvider)source.Provider).Execute<double?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, long?>>, double?>(MongoQueryable.StandardDeviationSample, source, selector),
                    arguments));
        }

        public static Task<decimal> StandardDeviationSampleAsync(this IQueryable<decimal> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<decimal>(
                Expression.Call(
                    GetMethodInfo<IQueryable<decimal>, CancellationToken, Task<decimal>>(MongoQueryable.StandardDeviationSampleAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<decimal?> StandardDeviationSampleAsync(this IQueryable<decimal?> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<decimal?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<decimal?>, CancellationToken, Task<decimal?>>(MongoQueryable.StandardDeviationSampleAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<double> StandardDeviationSampleAsync(this IQueryable<double> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<double>(
                Expression.Call(
                    GetMethodInfo<IQueryable<double>, CancellationToken, Task<double>>(MongoQueryable.StandardDeviationSampleAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<double?> StandardDeviationSampleAsync(this IQueryable<double?> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<double?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<double?>, CancellationToken, Task<double?>>(MongoQueryable.StandardDeviationSampleAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<float> StandardDeviationSampleAsync(this IQueryable<float> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<float>(
                Expression.Call(
                    GetMethodInfo<IQueryable<float>, CancellationToken, Task<float>>(MongoQueryable.StandardDeviationSampleAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<float?> StandardDeviationSampleAsync(this IQueryable<float?> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<float?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<float?>, CancellationToken, Task<float?>>(MongoQueryable.StandardDeviationSampleAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<double> StandardDeviationSampleAsync(this IQueryable<int> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<double>(
                Expression.Call(
                    GetMethodInfo<IQueryable<int>, CancellationToken, Task<double>>(MongoQueryable.StandardDeviationSampleAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<double?> StandardDeviationSampleAsync(this IQueryable<int?> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<double?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<int?>, CancellationToken, Task<double?>>(MongoQueryable.StandardDeviationSampleAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<double> StandardDeviationSampleAsync(this IQueryable<long> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<double>(
                Expression.Call(
                    GetMethodInfo<IQueryable<long>, CancellationToken, Task<double>>(MongoQueryable.StandardDeviationSampleAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<double?> StandardDeviationSampleAsync(this IQueryable<long?> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<double?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<long?>, CancellationToken, Task<double?>>(MongoQueryable.StandardDeviationSampleAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<decimal> StandardDeviationSampleAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, decimal>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<decimal>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, decimal>>, CancellationToken, Task<decimal>>(MongoQueryable.StandardDeviationSampleAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<decimal?> StandardDeviationSampleAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, decimal?>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<decimal?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, decimal?>>, CancellationToken, Task<decimal?>>(MongoQueryable.StandardDeviationSampleAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<double> StandardDeviationSampleAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, double>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<double>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, double>>, CancellationToken, Task<double>>(MongoQueryable.StandardDeviationSampleAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<double?> StandardDeviationSampleAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, double?>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<double?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, double?>>, CancellationToken, Task<double?>>(MongoQueryable.StandardDeviationSampleAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<float> StandardDeviationSampleAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, float>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<float>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, float>>, CancellationToken, Task<float>>(MongoQueryable.StandardDeviationSampleAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<float?> StandardDeviationSampleAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, float?>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<float?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, float?>>, CancellationToken, Task<float?>>(MongoQueryable.StandardDeviationSampleAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<double> StandardDeviationSampleAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, int>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<double>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, int>>, CancellationToken, Task<double>>(MongoQueryable.StandardDeviationSampleAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<double?> StandardDeviationSampleAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, int?>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<double?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, int?>>, CancellationToken, Task<double?>>(MongoQueryable.StandardDeviationSampleAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<double> StandardDeviationSampleAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, long>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<double>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, long>>, CancellationToken, Task<double>>(MongoQueryable.StandardDeviationSampleAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<double?> StandardDeviationSampleAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, long?>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<double?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, long?>>, CancellationToken, Task<double?>>(MongoQueryable.StandardDeviationSampleAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<decimal> SumAsync(this IQueryable<decimal> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<decimal>(
                Expression.Call(
                    GetMethodInfo<IQueryable<decimal>, CancellationToken, Task<decimal>>(MongoQueryable.SumAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<decimal?> SumAsync(this IQueryable<decimal?> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<decimal?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<decimal?>, CancellationToken, Task<decimal?>>(MongoQueryable.SumAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<double> SumAsync(this IQueryable<double> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<double>(
                Expression.Call(
                    GetMethodInfo<IQueryable<double>, CancellationToken, Task<double>>(MongoQueryable.SumAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<double?> SumAsync(this IQueryable<double?> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<double?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<double?>, CancellationToken, Task<double?>>(MongoQueryable.SumAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<float> SumAsync(this IQueryable<float> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<float>(
                Expression.Call(
                    GetMethodInfo<IQueryable<float>, CancellationToken, Task<float>>(MongoQueryable.SumAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<float?> SumAsync(this IQueryable<float?> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<float?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<float?>, CancellationToken, Task<float?>>(MongoQueryable.SumAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<int> SumAsync(this IQueryable<int> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<int>(
                Expression.Call(
                    GetMethodInfo<IQueryable<int>, CancellationToken, Task<int>>(MongoQueryable.SumAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<int?> SumAsync(this IQueryable<int?> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<int?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<int?>, CancellationToken, Task<int?>>(MongoQueryable.SumAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<long> SumAsync(this IQueryable<long> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<long>(
                Expression.Call(
                    GetMethodInfo<IQueryable<long>, CancellationToken, Task<long>>(MongoQueryable.SumAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<long?> SumAsync(this IQueryable<long?> source, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<long?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<long?>, CancellationToken, Task<long?>>(MongoQueryable.SumAsync, source, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<decimal> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, decimal>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<decimal>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, decimal>>, CancellationToken, Task<decimal>>(MongoQueryable.SumAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<decimal?> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, decimal?>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<decimal?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, decimal?>>, CancellationToken, Task<decimal?>>(MongoQueryable.SumAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<double> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, double>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<double>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, double>>, CancellationToken, Task<double>>(MongoQueryable.SumAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<double?> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, double?>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<double?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, double?>>, CancellationToken, Task<double?>>(MongoQueryable.SumAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<float> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, float>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<float>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, float>>, CancellationToken, Task<float>>(MongoQueryable.SumAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<float?> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, float?>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<float?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, float?>>, CancellationToken, Task<float?>>(MongoQueryable.SumAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<int> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, int>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<int>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, int>>, CancellationToken, Task<int>>(MongoQueryable.SumAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<int?> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, int?>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<int?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, int?>>, CancellationToken, Task<int?>>(MongoQueryable.SumAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<long> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, long>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<long>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, long>>, CancellationToken, Task<long>>(MongoQueryable.SumAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static Task<long?> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, long?>> selector, CancellationToken cancellationToken = default)
        {
            var arguments = new[] { source.Expression, Expression.Quote(selector), Expression.Constant(cancellationToken) };
            return ((MongoQueryProvider)source.Provider).ExecuteAsync<long?>(
                Expression.Call(
                    GetMethodInfo<IQueryable<TSource>, Expression<Func<TSource, long?>>, CancellationToken, Task<long?>>(MongoQueryable.SumAsync, source, selector, cancellationToken),
                    arguments),
                cancellationToken);
        }

        public static async Task<List<TSource>> ToListAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
        {
            var mongoQuery = (MongoQuery<TSource>)source;
            var cursor = await mongoQuery.ExecuteAsync().ConfigureAwait(false);
            var list = await cursor.ToListAsync(cancellationToken).ConfigureAwait(false);
            return list;
        }

        private static MethodInfo GetMethodInfo<T1, T2>(Func<T1, T2> f, T1 unused)
        {
            return f.GetMethodInfo();
        }

        private static MethodInfo GetMethodInfo<T1, T2, T3>(Func<T1, T2, T3> f, T1 unused1, T2 unused2)
        {
            return f.GetMethodInfo();
        }

        private static MethodInfo GetMethodInfo<T1, T2, T3, T4>(Func<T1, T2, T3, T4> f, T1 unused1, T2 unused2, T3 unused3)
        {
            return f.GetMethodInfo();
        }
    }
}