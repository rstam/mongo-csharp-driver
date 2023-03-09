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
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDB.Driver.MqlBuilder
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static class Mql
    {
        public static int Abs(int value)
        {
            throw new InvalidOperationException();
        }

        public static double Acos(double value)
        {
            throw new InvalidOperationException();
        }

        public static double Acosh(double value)
        {
            throw new InvalidOperationException();
        }

        public static TValue[] AddToSetAccumulator<TValue>(TValue value)
        {
            throw new InvalidOperationException();
        }

        public static bool All<TValue>(this IEnumerable<TValue> field, params TValue[] values)
        {
            throw new InvalidOperationException();
        }

        public static bool AllElementsTrue<TItem>(this IEnumerable<TItem> array)
        {
            throw new InvalidOperationException();
        }

        public static bool AnyElementTrue<TItem>(this IEnumerable<TItem> array)
        {
            throw new InvalidOperationException();
        }

        public static MqlSortOrder Ascending(object field)
        {
            throw new InvalidOperationException();
        }

        public static double Asin(double value)
        {
            throw new InvalidOperationException();
        }

        public static double Asinh(double value)
        {
            throw new InvalidOperationException();
        }

        public static double Atan(double value)
        {
            throw new InvalidOperationException();
        }

        public static double Atanh(double value)
        {
            throw new InvalidOperationException();
        }

        public static double Atan2(double y, double x)
        {
            throw new InvalidOperationException();
        }

        public static double Avg(IEnumerable<int> value)
        {
            throw new InvalidOperationException();
        }

        public static double AvgAccumulator(int value)
        {
            throw new InvalidOperationException();
        }

        public static bool BinarySize(object value)
        {
            throw new InvalidOperationException();
        }

        public static bool BitsAllClear(this int value, int mask)
        {
            throw new InvalidOperationException();
        }

        public static bool BitsAllSet(this int value, int mask)
        {
            throw new InvalidOperationException();
        }

        public static bool BitsAnyClear(this int value, int mask)
        {
            throw new InvalidOperationException();
        }

        public static bool BitsAnySet(this int value, int mask)
        {
            throw new InvalidOperationException();
        }

        public static bool BsonSize(object value)
        {
            throw new InvalidOperationException();
        }

        public static MqlCase<TResult> Case<TResult>(bool predicate, TResult result)
        {
            throw new InvalidOperationException();
        }

        public static long Ceil(double value)
        {
            throw new InvalidOperationException();
        }

        public static int Cmp<TValue>(TValue first, TValue second)
        {
            throw new InvalidOperationException();
        }

        public static IEnumerable<TItem> ConcatArrays<TItem>(this IEnumerable<TItem> first, params IEnumerable<TItem>[] others)
        {
            throw new InvalidOperationException();
        }

        public static TTo Convert<TTo>(object value)
        {
            throw new InvalidOperationException();
        }

        public static TTo Convert<TTo>(object value, TTo onNull)
        {
            throw new InvalidOperationException();
        }

        public static TTo Convert<TTo>(object value, TTo onNull, TTo onError)
        {
            throw new InvalidOperationException();
        }

        public static double Cos(double value)
        {
            throw new InvalidOperationException();
        }

        public static double Cosh(double value)
        {
            throw new InvalidOperationException();
        }

        public static long CountAccumulator()
        {
            throw new InvalidOperationException();
        }

        public static MqlCase<TResult> Default<TResult>(TResult result)
        {
            throw new InvalidOperationException();
        }

        public static double DegreesToRadians(double value)
        {
            throw new InvalidOperationException();
        }

        public static MqlSortOrder Descending(object field)
        {
            throw new InvalidOperationException();
        }

        public static double Divide(int x, int y)
        {
            throw new InvalidOperationException();
        }

        public static bool ElemMatch<TValue>(this IEnumerable<TValue> field, Func<TValue, bool> predicate)
        {
            throw new InvalidOperationException();
        }

        public static bool Exists<TField>(TField field)
        {
            throw new InvalidOperationException();
        }

        public static double Exp(double value)
        {
            throw new InvalidOperationException();
        }

        public static bool Expr(bool expr)
        {
            throw new InvalidOperationException();
        }

        public static MqlFilter<TDocument> Filter<TDocument>(IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> predicate)
        {
            return new MqlFilter<TDocument>(collection.DocumentSerializer, predicate);
        }

        public static IEnumerable<TItem> Filter<TItem>(this IEnumerable<TItem> input, Func<TItem, bool> cond)
        {
            throw new InvalidOperationException();
        }

        public static IEnumerable<TItem> Filter<TItem>(this IEnumerable<TItem> input, Func<TItem, bool> cond, long limit)
        {
            throw new InvalidOperationException();
        }

        public static TValue FirstAccumulator<TValue>(TValue value)
        {
            throw new InvalidOperationException();
        }

        public static IEnumerable<TItem> FirstN<TItem>(this IEnumerable<TItem> input, long n)
        {
            throw new InvalidOperationException();
        }

        public static IEnumerable<TValue> FirstNAccumulator<TValue>(TValue value, long n)
        {
            throw new InvalidOperationException();
        }

        public static long Floor(double value)
        {
            throw new InvalidOperationException();
        }

        public static bool In<TValue>(this TValue value, params TValue[] values)
        {
            throw new InvalidOperationException();
        }

        public static long IndexOfArray<TItem>(this IEnumerable<TItem> array, TItem value)
        {
            throw new InvalidOperationException();
        }

        public static long IndexOfArray<TItem>(this IEnumerable<TItem> array, TItem value, long start)
        {
            throw new InvalidOperationException();
        }

        public static long IndexOfArray<TItem>(this IEnumerable<TItem> array, TItem value, long start, long end)
        {
            throw new InvalidOperationException();
        }

        // IndexOfBytes should probably be moved here

        public static long IndexOfCP(this string value, string substring)
        {
            throw new InvalidOperationException();
        }

        public static long IndexOfCP(this string value, string substring, long start)
        {
            throw new InvalidOperationException();
        }

        public static long IndexOfCP(this string value, string substring, long start, long end)
        {
            throw new InvalidOperationException();
        }

        public static bool IsNumber(object value)
        {
            throw new InvalidOperationException();
        }

        public static bool IsArray(object value)
        {
            throw new InvalidOperationException();
        }

        public static bool JsonSchema(BsonDocument schema)
        {
            throw new InvalidOperationException();
        }

        public static TValue LastAccumulator<TValue>(TValue value)
        {
            throw new InvalidOperationException();
        }

        public static IEnumerable<TItem> LastN<TItem>(this IEnumerable<TItem> input, long n)
        {
            throw new InvalidOperationException();
        }

        public static IEnumerable<TValue> LastNAccumulator<TValue>(TValue value, long n)
        {
            throw new InvalidOperationException();
        }

        public static TValue Literal<TValue>(TValue value)
        {
            throw new InvalidOperationException();
        }

        public static double Ln(double value)
        {
            throw new InvalidOperationException();
        }

        public static double Log(double value, long @base)
        {
            throw new InvalidOperationException();
        }

        public static double Log10(double value)
        {
            throw new InvalidOperationException();
        }

        public static string LTrim(this string value)
        {
            throw new InvalidOperationException();
        }

        public static string LTrim(this string value, string chars)
        {
            throw new InvalidOperationException();
        }

        public static IEnumerable<TMapped> Map<TItem, TMapped>(this IEnumerable<TItem> input, Func<TItem, TMapped> function)
        {
            throw new InvalidOperationException();
        }

        public static TValue Max<TValue>(IEnumerable<TValue> array)
        {
            throw new InvalidOperationException();
        }

        public static TValue MaxAccumulator<TValue>(TValue value)
        {
            throw new InvalidOperationException();
        }

        public static IEnumerable<TItem> MaxN<TItem>(this IEnumerable<TItem> input, long n)
        {
            throw new InvalidOperationException();
        }

        public static IEnumerable<TValue> MaxNAccumulator<TValue>(TValue value, long n)
        {
            throw new InvalidOperationException();
        }

        public static double MetaTextScore()
        {
            throw new InvalidOperationException();
        }

        public static TValue Min<TValue>(IEnumerable<TValue> array)
        {
            throw new InvalidOperationException();
        }

        public static TValue MinAccumulator<TValue>(TValue value)
        {
            throw new InvalidOperationException();
        }

        public static IEnumerable<TItem> MinN<TItem>(this IEnumerable<TItem> input, long n)
        {
            throw new InvalidOperationException();
        }

        public static IEnumerable<TValue> MinNAccumulator<TValue>(TValue value, long n)
        {
            throw new InvalidOperationException();
        }

        public static long Mod(long dividend, long divisor)
        {
            throw new InvalidOperationException();
        }

        public static bool Mod(int field, int divisor, int remainder)
        {
            throw new InvalidOperationException();
        }

        public static bool Nin<TValue>(this TValue value, params TValue[] values)
        {
            throw new InvalidOperationException();
        }

        public static bool Nor(params bool[] clauses)
        {
            throw new InvalidOperationException();
        }

        public static bool NotExists<TField>(TField field)
        {
            throw new InvalidOperationException();
        }

        public static IEnumerable<ObjectToArrayElement> ObjectToArray<TDocument>(TDocument document)
        {
            throw new InvalidOperationException();
        }

        public static MqlPipeline<TDocument, TDocument> Pipeline<TDocument>(IMongoCollection<TDocument> collection)
        {
            var inputSerializer = collection.DocumentSerializer;
            return new MqlPipeline<TDocument, TDocument>(inputSerializer, inputSerializer);
        }

        public static MqlPipeline<NoPipelineInput, NoPipelineInput> Pipeline(IMongoDatabase database)
        {
            var inputSerializer = NoPipelineInputSerializer.Instance;
            return new MqlPipeline<NoPipelineInput, NoPipelineInput>(inputSerializer, inputSerializer);
        }

        public static double Pow(double number, double exponent)
        {
            throw new InvalidOperationException();
        }

        public static TValue[] PushAccumulator<TValue>(TValue value)
        {
            throw new InvalidOperationException();
        }

        public static double RadiansToDegrees(double value)
        {
            throw new InvalidOperationException();
        }

        public static double Rand()
        {
            throw new InvalidOperationException();
        }

        public static IEnumerable<long> Range(long start, long end)
        {
            throw new InvalidOperationException();
        }

        public static IEnumerable<long> Range(long start, long end, long step)
        {
            throw new InvalidOperationException();
        }

        public static TAccumulator Reduce<TItem, TAccumulator>(this IEnumerable<TItem> input, TAccumulator initialValue, Func<TAccumulator, TItem, TAccumulator> @in)
        {
            throw new InvalidOperationException();
        }

        public static bool Regex(string field, string pattern, string options)
        {
            throw new InvalidOperationException();
        }

        public static string ReplaceOne(this string value, string find, string replacemet)
        {
            throw new InvalidOperationException();
        }

        public static string ReplaceAll(this string value, string find, string replacemet)
        {
            throw new InvalidOperationException();
        }

        public static IEnumerable<TItem> ReverseArray<TItem>(this IEnumerable<TItem> input)
        {
            throw new InvalidOperationException();
        }

        public static double RoundToDouble(double number, int place)
        {
            throw new InvalidOperationException();
        }

        public static long RoundToInteger(double number)
        {
            throw new InvalidOperationException();
        }

        public static long RoundToInteger(double number, int place)
        {
            throw new InvalidOperationException();
        }

        public static string RTrim(this string value)
        {
            throw new InvalidOperationException();
        }

        public static string RTrim(this string value, string chars)
        {
            throw new InvalidOperationException();
        }

        public static bool SampleRate(double rate)
        {
            throw new InvalidOperationException();
        }

        public static IEnumerable<TItem> SetDifference<TItem>(this IEnumerable<TItem> set1, IEnumerable<TItem> set2)
        {
            throw new InvalidOperationException();
        }

        public static bool SetEquals<TItem>(this IEnumerable<TItem> set1, IEnumerable<TItem> set2)
        {
            throw new InvalidOperationException();
        }

        public static IEnumerable<TItem> SetIntersection<TItem>(this IEnumerable<TItem> set1, IEnumerable<TItem> set2)
        {
            throw new InvalidOperationException();
        }

        public static bool SetIsSubset<TItem>(this IEnumerable<TItem> set1, IEnumerable<TItem> set2)
        {
            throw new InvalidOperationException();
        }

        public static IEnumerable<TItem> SetUnion<TItem>(this IEnumerable<TItem> set1, IEnumerable<TItem> set2)
        {
            throw new InvalidOperationException();
        }

        public static double Sin(double value)
        {
            throw new InvalidOperationException();
        }

        public static double Sinh(double value)
        {
            throw new InvalidOperationException();
        }

        public static long Size<TItem>(this IEnumerable<TItem> array)
        {
            throw new InvalidOperationException();
        }

        public static bool Size<TValue>(this IEnumerable<TValue> field, int size)
        {
            throw new InvalidOperationException();
        }

        public static IEnumerable<TItem> Slice<TItem>(this IEnumerable<TItem> input, long n)
        {
            throw new InvalidOperationException();
        }

        public static IEnumerable<TItem> Slice<TItem>(this IEnumerable<TItem> input, long position, long n)
        {
            throw new InvalidOperationException();
        }

        public static double Sqrt(double value)
        {
            throw new InvalidOperationException();
        }

        public static double StdDevPop(IEnumerable<int> value)
        {
            throw new InvalidOperationException();
        }

        public static double StdDevPopAccumulator(double value)
        {
            throw new InvalidOperationException();
        }

        public static double StdDevSamp(IEnumerable<int> value)
        {
            throw new InvalidOperationException();
        }

        public static double StdDevSampAccumulator(double value)
        {
            throw new InvalidOperationException();
        }

        public static int StrCaseCmp(string first, string second)
        {
            throw new InvalidOperationException();
        }

        // TODO: move StrLenBytes here?

        public static long StrLenCP(this string value)
        {
            throw new InvalidOperationException();
        }

        public static string SubstrBytes(string value, int index, int count)
        {
            throw new InvalidOperationException();
        }

        public static string SubstrCP(string value, int index, int count)
        {
            throw new InvalidOperationException();
        }

        public static decimal SumAccumulator(decimal value)
        {
            throw new InvalidOperationException();
        }

        public static long Sum(IEnumerable<int> value)
        {
            throw new InvalidOperationException();
        }

        public static long SumAccumulator(long value)
        {
            throw new InvalidOperationException();
        }

        public static TResult Switch<TResult>(params MqlCase<TResult>[] cases)
        {
            throw new InvalidOperationException();
        }

        public static double Tan(double value)
        {
            throw new InvalidOperationException();
        }

        public static double Tanh(double value)
        {
            throw new InvalidOperationException();
        }

        public static bool Text(string search, MqlTextArgs args)
        {
            throw new InvalidOperationException();
        }

        public static bool ToBool(object value)
        {
            throw new InvalidOperationException();
        }

        public static DateTime ToDate(object value)
        {
            throw new InvalidOperationException();
        }

        public static decimal ToDecimal(object value)
        {
            throw new InvalidOperationException();
        }

        public static double ToDouble(object value)
        {
            throw new InvalidOperationException();
        }

        public static int ToInt(object value)
        {
            throw new InvalidOperationException();
        }

        public static long ToLong(object value)
        {
            throw new InvalidOperationException();
        }

        public static ObjectId ToObjectId(object value)
        {
            throw new InvalidOperationException();
        }

        public static string ToString(object value)
        {
            throw new InvalidOperationException();
        }

        public static string Trim(this string value)
        {
            throw new InvalidOperationException();
        }

        public static string Trim(this string value, string chars)
        {
            throw new InvalidOperationException();
        }

        public static double TruncToDouble(double number, int place)
        {
            throw new InvalidOperationException();
        }

        public static long TruncToInteger(double number)
        {
            throw new InvalidOperationException();
        }

        public static long TruncToInteger(double number, int place)
        {
            throw new InvalidOperationException();
        }

        public static bool Type<TField>(TField field, BsonType type)
        {
            throw new InvalidOperationException();
        }

        public static bool Type<TField>(TField field, params BsonType[] types)
        {
            throw new InvalidOperationException();
        }

        public static string Type(object value)
        {
            throw new InvalidOperationException();
        }

        public static MqlUpdate<TDocument> Update<TDocument>(IMongoCollection<TDocument> collection)
        {
            return new MqlUpdate<TDocument>(collection.DocumentSerializer);
        }

        // Unfortunately not named Zip because of conflicts with LINQ on some target frameworks
        public static IEnumerable<(TItem1, TItem2)> ZipMql<TItem1, TItem2>(this IEnumerable<TItem1> array1, IEnumerable<TItem2> array2)
        {
            throw new InvalidOperationException();
        }

        public static IEnumerable<(TItem1, TItem2)> Zip<TItem1, TItem2>(this IEnumerable<TItem1> array1, IEnumerable<TItem2> array2, bool useLongestLength)
        {
            throw new InvalidOperationException();
        }

        public static IEnumerable<(TItem1, TItem2)> Zip<TItem1, TItem2>(this IEnumerable<TItem1> array1, IEnumerable<TItem2> array2, bool useLongestLength, TItem1 default1, TItem2 default2)
        {
            throw new InvalidOperationException();
        }
    }

    public class MqlCase<TResult>
    {
        public bool Predicate { get; set; }
        public TResult Result { get; set; }
    }

    public class MqlHighlightOptions
    {
        public int? MaxCharsToExamine { get; set; }
        public int? MaxNumPassages { get; set; }
        public string Path { get; set; }
    }

    public class MqlSearchArgs
    {
        public int? Count { get; set; }
        public MqlHighlightOptions Highlight { get; set; }
        public string Index { get; set; }
        public bool? ReturnStoredSource { get; set; }
    }

    public abstract class MqlSearchOperation
    {
    }

    public class MqlSearchOperator : MqlSearchOperation
    {
    }

    public class MqlSearchCollector : MqlSearchOperation
    {
    }

    public class MqlTextArgs
    {
        public string Language { get; set; }
        public bool CaseSensitive { get; set; }
        public bool DiacriticSensitive { get; set; }
    }

    public class ObjectToArrayElement
    {
        [BsonElement("k")] public string Name { get; set; }
        [BsonElement("v")] public BsonValue Value { get; set;}
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
