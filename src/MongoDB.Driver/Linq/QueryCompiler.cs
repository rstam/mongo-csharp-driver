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

using System;
using System.Linq;
using System.Linq.Expressions;

namespace MongoDB.Driver.Linq;

/// <summary>
///
/// </summary>
public static class QueryCompiler
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="expression"></param>
    /// <typeparam name="TParameter"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static CompiledQuery<TParameter, TOutput> CompileQuery<TParameter, TOutput>(Expression<Func<TParameter, IQueryable<TOutput>>> expression)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="expression"></param>
    /// <typeparam name="TParameter"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static CompiledScalarQuery<TParameter, TResult> CompileScalarQuery<TParameter, TResult>(Expression<Func<TParameter, TResult>> expression)
    {
        throw new NotImplementedException();
    }
}
