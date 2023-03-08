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

using System.Reflection;
using MongoDB.Bson;
using MongoDB.Driver.Linq.Linq3Implementation.Reflection;

namespace MongoDB.Driver.MqlApi.Translators
{
    internal static class MqlMethod
    {
        // private static fields
        private static readonly MethodInfo __exists;
        private static readonly MethodInfo __expr;
        private static readonly MethodInfo __in;
        private static readonly MethodInfo __nin;
        private static readonly MethodInfo __nor;
        private static readonly MethodInfo __notExists;
        private static readonly MethodInfo __type;
        private static readonly MethodInfo __typeWithArray;

        // static constructor
        static MqlMethod()
        {
            __exists = ReflectionInfo.Method((object field) => Mql.Exists(field));
            __expr = ReflectionInfo.Method((bool expr) => Mql.Expr(expr));
            __in = ReflectionInfo.Method((object value, object[] values) => value.In(values));
            __nin = ReflectionInfo.Method((object value, object[] values) => value.Nin(values));
            __nor = ReflectionInfo.Method((bool[] clauses) => Mql.Nor(clauses));
            __notExists = ReflectionInfo.Method((object field) => Mql.NotExists(field));
            __type = ReflectionInfo.Method((object field, BsonType type) => Mql.Type(field, type));
            __typeWithArray = ReflectionInfo.Method((object field, BsonType[] types) => Mql.Type(field, types));
        }

        // public properties
        public static MethodInfo Exists => __exists;
        public static MethodInfo Expr => __expr;
        public static MethodInfo In => __in;
        public static MethodInfo Nin => __nin;
        public static MethodInfo Nor => __nor;
        public static MethodInfo NotExists => __notExists;
        public static MethodInfo Type => __type;
        public static MethodInfo TypeWithArray => __typeWithArray;
    }
}
