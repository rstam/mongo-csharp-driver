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
using System.Collections;
using System.Collections.Generic;

namespace MongoDB.Driver.Linq;

/// <summary>
///
/// </summary>
/// <typeparam name="TResult"></typeparam>
public abstract class CompiledQueryBase<TResult>
{

}

/// <summary>
///
/// </summary>
/// <typeparam name="TResult"></typeparam>
public class CompiledQuery<TResult> : CompiledQueryBase<TResult>
{
    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public IAsyncCursor<TResult> Execute()
    {
        throw new NotImplementedException();
    }
}

/// <summary>
///
/// </summary>
/// <typeparam name="TParameter"></typeparam>
/// <typeparam name="TResult"></typeparam>
public class CompiledQueryBase<TParameter, TResult> : CompiledQueryBase<TResult>
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="parameter"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public IAsyncCursor<TResult> Execute(TParameter parameter)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
///
/// </summary>
/// <typeparam name="TParameter1"></typeparam>
/// <typeparam name="TParameter2"></typeparam>
/// <typeparam name="TResult"></typeparam>
public class CompiledQueryBase<TParameter1, TParameter2, TResult> : CompiledQueryBase<TResult>
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="parameter1"></param>
    /// <param name="parameter2"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public IAsyncCursor<TResult> Execute(TParameter1 parameter1, TParameter2 parameter2)
    {
        throw new NotImplementedException();
    }
}
