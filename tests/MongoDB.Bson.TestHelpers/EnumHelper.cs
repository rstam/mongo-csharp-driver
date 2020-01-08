﻿/* Copyright 2019-present MongoDB Inc.
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
using System.Text;

namespace MongoDB.Bson.TestHelpers
{
    public static class EnumHelper
    {
        public static IEnumerable<TEnum> GetValues<TEnum>() where TEnum : System.Enum
        {
            return Enum.GetValues(typeof(TEnum)).Cast<TEnum>();
        }

        public static IEnumerable<TEnum?> GetNullableValues<TEnum>() where TEnum : struct, System.Enum
        {
            return new TEnum?[] { null }.Concat(Enum.GetValues(typeof(TEnum)).Cast<TEnum?>());
        }
    }
}