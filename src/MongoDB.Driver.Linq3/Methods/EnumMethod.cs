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
using System.Reflection;

namespace MongoDB.Driver.Linq3.Methods
{
    public static class EnumMethod
    {
        // private static fields
        private static readonly MethodInfo __hasFlag;

        // static constructor
        static EnumMethod()
        {
            __hasFlag = new Func<Enum, bool>(new E().HasFlag).Method;
        }

        // public properties
        public static MethodInfo HasFlag => __hasFlag;

        private enum E { A = 0 };
    }
}