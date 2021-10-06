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
* 
*/

using System;

namespace MongoDB.Driver.Linq
{
    /// <summary>
    /// This static class holds methods that can be used to express MongoDB specific field related operations in LINQ queries.
    /// </summary>
    public static class FieldExtensions
    {
        /// <summary>
        /// Checks whether a field exists.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="field">The field.</param>
        /// <returns>True if the field exists, false if not.</returns>
        public static bool Exists<TField>(this TField field)
        {
            throw new NotSupportedException("This method is not functional. It is only usable in conjunction with MongoDB.");
        }
    }
}
