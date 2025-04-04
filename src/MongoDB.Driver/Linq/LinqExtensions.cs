﻿/* Copyright 2016-present MongoDB Inc.
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

using MongoDB.Driver.Linq.Linq3Implementation.Misc;

namespace MongoDB.Driver.Linq
{
    /// <summary>
    /// This static class holds methods that can be used to express MongoDB specific operations in LINQ queries.
    /// </summary>
    public static class LinqExtensions
    {
        /// <summary>
        /// Injects a low level FilterDefinition{TDocument} into a LINQ where clause. Can only be used in LINQ queries.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="filter">The filter.</param>
        /// <returns>
        /// Throws an InvalidOperationException if called.
        /// </returns>
        public static bool Inject<TDocument>(this FilterDefinition<TDocument> filter)
        {
            throw CustomLinqExtensionMethodHelper.CreateNotSupportedException();
        }
    }
}
