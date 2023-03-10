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
using System.Collections.Generic;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Driver.Linq.Linq3Implementation.Reflection;

namespace MongoDB.Driver.MqlBuilder.Translators
{
    internal static class MqlMethod
    {
        // private static fields
        private static readonly MethodInfo __abs;
        private static readonly MethodInfo __all;
        private static readonly MethodInfo __bitsAllClear;
        private static readonly MethodInfo __bitsAllSet;
        private static readonly MethodInfo __bitsAnyClear;
        private static readonly MethodInfo __bitsAnySet;
        private static readonly MethodInfo __elemMatch;
        private static readonly MethodInfo __exists;
        private static readonly MethodInfo __expr;
        private static readonly MethodInfo __in;
        private static readonly MethodInfo __jsonSchema;
        private static readonly MethodInfo __nin;
        private static readonly MethodInfo __nor;
        private static readonly MethodInfo __notExists;
        private static readonly MethodInfo __regex;
        private static readonly MethodInfo __size;
        private static readonly MethodInfo __text;
        private static readonly MethodInfo __type;
        private static readonly MethodInfo __typeWithArray;

        // static constructor
        static MqlMethod()
        {
            __abs = ReflectionInfo.Method((int value) => Mql.Abs(value));
            __all = ReflectionInfo.Method((IEnumerable<object> field, object[] values) => Mql.All(field, values));
            __bitsAllClear = ReflectionInfo.Method((int value, int mask) => Mql.BitsAllClear(value, mask));
            __bitsAllSet = ReflectionInfo.Method((int value, int mask) => Mql.BitsAllSet(value, mask));
            __bitsAnyClear = ReflectionInfo.Method((int value, int mask) => Mql.BitsAnyClear(value, mask));
            __bitsAnySet = ReflectionInfo.Method((int value, int mask) => Mql.BitsAnySet(value, mask));
            __elemMatch = ReflectionInfo.Method((IEnumerable<object> field, Func<object, bool> predicate) => Mql.ElemMatch(field, predicate));
            __exists = ReflectionInfo.Method((object field) => Mql.Exists(field));
            __expr = ReflectionInfo.Method((bool expr) => Mql.Expr(expr));
            __in = ReflectionInfo.Method((object value, object[] values) => value.In(values));
            __jsonSchema = ReflectionInfo.Method((BsonDocument schema) => Mql.JsonSchema(schema));
            __nin = ReflectionInfo.Method((object value, object[] values) => value.Nin(values));
            __nor = ReflectionInfo.Method((bool[] clauses) => Mql.Nor(clauses));
            __notExists = ReflectionInfo.Method((object field) => Mql.NotExists(field));
            __regex = ReflectionInfo.Method((string field, string pattern, string options) => Mql.Regex(field, pattern, options));
            __size = ReflectionInfo.Method((IEnumerable<object> field, int size) => Mql.Size(field, size));
            __text = ReflectionInfo.Method((string field, MqlTextArgs args) => Mql.Text(field, args));
            __type = ReflectionInfo.Method((object field, BsonType type) => Mql.Type(field, type));
            __typeWithArray = ReflectionInfo.Method((object field, BsonType[] types) => Mql.Type(field, types));
        }

        // public properties
        public static MethodInfo Abs => __abs;
        public static MethodInfo All => __all;
        public static MethodInfo BitsAllClear => __bitsAllClear;
        public static MethodInfo BitsAllSet => __bitsAllSet;
        public static MethodInfo BitsAnyClear => __bitsAnyClear;
        public static MethodInfo BitsAnySet => __bitsAnySet;
        public static MethodInfo ElemMatch => __elemMatch;
        public static MethodInfo Exists => __exists;
        public static MethodInfo Expr => __expr;
        public static MethodInfo In => __in;
        public static MethodInfo JsonSchema => __jsonSchema;
        public static MethodInfo Nin => __nin;
        public static MethodInfo Nor => __nor;
        public static MethodInfo NotExists => __notExists;
        public static MethodInfo Size => __size;
        public static MethodInfo Text => __text;
        public static MethodInfo Type => __type;
        public static MethodInfo Regex => __regex;
        public static MethodInfo TypeWithArray => __typeWithArray;
    }
}
