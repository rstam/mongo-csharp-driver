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
        private static readonly MethodInfo __ceil;
        private static readonly MethodInfo __concatArrays;
        private static readonly MethodInfo __divide;
        private static readonly MethodInfo __elemMatch;
        private static readonly MethodInfo __exists;
        private static readonly MethodInfo __exp;
        private static readonly MethodInfo __expr;
        private static readonly MethodInfo __filter;
        private static readonly MethodInfo __filterWithLimit;
        private static readonly MethodInfo __firstN;
        private static readonly MethodInfo __floor;
        private static readonly MethodInfo __in;
        private static readonly MethodInfo __jsonSchema;
        private static readonly MethodInfo __lastN;
        private static readonly MethodInfo __ln;
        private static readonly MethodInfo __log;
        private static readonly MethodInfo __log10;
        private static readonly MethodInfo __nin;
        private static readonly MethodInfo __nor;
        private static readonly MethodInfo __notExists;
        private static readonly MethodInfo __pow;
        private static readonly MethodInfo __regex;
        private static readonly MethodInfo __roundToDouble;
        private static readonly MethodInfo __roundToInteger;
        private static readonly MethodInfo __roundToIntegerWithPlace;
        private static readonly MethodInfo __size;
        private static readonly MethodInfo __sqrt;
        private static readonly MethodInfo __text;
        private static readonly MethodInfo __truncToDouble;
        private static readonly MethodInfo __truncToInteger;
        private static readonly MethodInfo __truncToIntegerWithPlace;
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
            __ceil = ReflectionInfo.Method((double value) => Mql.Ceil(value));
            __concatArrays = ReflectionInfo.Method((IEnumerable<object> first, IEnumerable<object>[] others) => Mql.ConcatArrays(first, others));
            __divide = ReflectionInfo.Method((int x, int y) => Mql.Divide(x, y));
            __elemMatch = ReflectionInfo.Method((IEnumerable<object> field, Func<object, bool> predicate) => Mql.ElemMatch(field, predicate));
            __exists = ReflectionInfo.Method((object field) => Mql.Exists(field));
            __exp = ReflectionInfo.Method((double value) => Mql.Exp(value));
            __expr = ReflectionInfo.Method((bool expr) => Mql.Expr(expr));
            __filter = ReflectionInfo.Method((IEnumerable<object> input, Func<object, bool> cond) => Mql.Filter(input, cond));
            __filterWithLimit = ReflectionInfo.Method((IEnumerable<object> input, Func<object, bool> cond, long limit) => Mql.Filter(input, cond, limit));
            __firstN = ReflectionInfo.Method((IEnumerable<object> input, long n) => Mql.FirstN(input, n));
            __floor = ReflectionInfo.Method((double value) => Mql.Floor(value));
            __in = ReflectionInfo.Method((object value, object[] values) => value.In(values));
            __jsonSchema = ReflectionInfo.Method((BsonDocument schema) => Mql.JsonSchema(schema));
            __lastN = ReflectionInfo.Method((IEnumerable<object> input, long n) => Mql.LastN(input, n));
            __ln = ReflectionInfo.Method((double value) => Mql.Ln(value));
            __log = ReflectionInfo.Method((double value, long @base) => Mql.Log(value, @base));
            __log10 = ReflectionInfo.Method((double value) => Mql.Log10(value));
            __nin = ReflectionInfo.Method((object value, object[] values) => value.Nin(values));
            __nor = ReflectionInfo.Method((bool[] clauses) => Mql.Nor(clauses));
            __notExists = ReflectionInfo.Method((object field) => Mql.NotExists(field));
            __pow = ReflectionInfo.Method((double number, double exponent) => Mql.Pow(number, exponent));
            __regex = ReflectionInfo.Method((string field, string pattern, string options) => Mql.Regex(field, pattern, options));
            __roundToDouble = ReflectionInfo.Method((double number, int place) => Mql.RoundToDouble(number, place));
            __roundToInteger= ReflectionInfo.Method((double number) => Mql.RoundToInteger(number));
            __roundToIntegerWithPlace = ReflectionInfo.Method((double number, int place) => Mql.RoundToInteger(number, place));
            __size = ReflectionInfo.Method((IEnumerable<object> field, int size) => Mql.Size(field, size));
            __sqrt = ReflectionInfo.Method((double value) => Mql.Sqrt(value));
            __text = ReflectionInfo.Method((string field, MqlTextArgs args) => Mql.Text(field, args));
            __truncToDouble = ReflectionInfo.Method((double number, int place) => Mql.TruncToDouble(number, place));
            __truncToInteger = ReflectionInfo.Method((double number) => Mql.TruncToInteger(number));
            __truncToIntegerWithPlace = ReflectionInfo.Method((double number, int place) => Mql.TruncToInteger(number, place));
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
        public static MethodInfo Ceil => __ceil;
        public static MethodInfo ConcatArrays => __concatArrays;
        public static MethodInfo Divide => __divide;
        public static MethodInfo ElemMatch => __elemMatch;
        public static MethodInfo Exists => __exists;
        public static MethodInfo Exp => __exp;
        public static MethodInfo Expr => __expr;
        public static MethodInfo Filter => __filter;
        public static MethodInfo FilterWithLimit => __filterWithLimit;
        public static MethodInfo FirstN => __firstN;
        public static MethodInfo Floor => __floor;
        public static MethodInfo In => __in;
        public static MethodInfo JsonSchema => __jsonSchema;
        public static MethodInfo LastN => __lastN;
        public static MethodInfo Ln => __ln;
        public static MethodInfo Log => __log;
        public static MethodInfo Log10 => __log10;
        public static MethodInfo Nin => __nin;
        public static MethodInfo Nor => __nor;
        public static MethodInfo NotExists => __notExists;
        public static MethodInfo Pow => __pow;
        public static MethodInfo Regex => __regex;
        public static MethodInfo RoundToDouble => __roundToDouble;
        public static MethodInfo RoundToInteger => __roundToInteger;
        public static MethodInfo RoundToIntegerWithPlace => __roundToIntegerWithPlace;
        public static MethodInfo Size => __size;
        public static MethodInfo Sqrt => __sqrt;
        public static MethodInfo Text => __text;
        public static MethodInfo TruncToDouble => __truncToDouble;
        public static MethodInfo TruncToInteger => __truncToInteger;
        public static MethodInfo TruncToIntegerWithPlace => __truncToIntegerWithPlace;
        public static MethodInfo Type => __type;
        public static MethodInfo TypeWithArray => __typeWithArray;
    }
}
