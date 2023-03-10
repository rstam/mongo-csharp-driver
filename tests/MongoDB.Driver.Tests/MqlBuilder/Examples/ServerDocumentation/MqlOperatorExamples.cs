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
using FluentAssertions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Driver.MqlBuilder;
using Xunit;

namespace MongoDB.Driver.Tests.MqlBuilder.Examples.ServerDocumentation
{
    public class MqlOperatorExamples : MqlIntegrationTest
    {
        [Fact]
        public void Operator_examples()
        {
            var collection = CreateCollection();
            var database = collection.Database;

            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#arithmetic-expression-operators
            _ = Mql.Pipeline(collection).Project(x => Mql.Abs(x.X)); // [{ $project : { _v : { $abs : '$X' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.X + x.Y); // [{ $project : { _v : { $add : ['$X', '$Y'] }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.Ceil(x.D)); // [{ $project : { _v : { $ceil : '$D' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.X / x.Y); // [{ $project : { _v : { $divide : ['$X', '$Y'] }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.Divide(x.X, x.Y)); // [{ $project : { _v : { $divide : ['$X', '$Y'] }, _id : 0 } }] // more accurate result type than "/"
            _ = Mql.Pipeline(collection).Project(x => Mql.Exp(x.X)); // [{ $project : { _v : { $exp : '$X' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.Floor(x.D)); // [{ $project : { _v : { $floor : '$D' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.Ln(x.D)); // [{ $project : { _v : { $ln : '$D' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.Log(x.D, 10)); // [{ $project : { _v : { $log : ['$D', 10] }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.Log10(x.D)); // [{ $project : { _v : { $log10 : '$D' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.X % x.Y); // [{ $project : { _v : { $mod : ['$X', '$Y'] }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.X * x.Y); // [{ $project : { _v : { $multiply : ['$X', '$Y'] }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.Pow(x.D, x.X)); // [{ $project : { _v : { $pow : ['$D', '$X'] }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.RoundToInteger(x.D)); // [{ $project : { _v : { $round : ['$D', 0] }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.RoundToInteger(x.D, -2)); // [{ $project : { _v : { $round : ['$D', -2] }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.RoundToDouble(x.D, 2)); // [{ $project : { _v : { $round : ['$D', 2] }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.Sqrt(x.D)); // [{ $project : { _v : { $sqrt : '$D' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.X - x.Y); // [{ $project : { _v : { $subtract : ['$X', '$Y'] }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.TruncToInteger(x.D)); // [{ $project : { _v : { $trunc : ['$D', 0] }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.TruncToInteger(x.D, -2)); // [{ $project : { _v : { $trunc : ['$D', -2] }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.TruncToDouble(x.D, 2)); // [{ $project : { _v : { $trunc : ['$D', 2] }, _id : 0 } }]

            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#array-expression-operators
            _ = Mql.Pipeline(collection).Project(x => x.A[x.X]); // [{ $project : { _v : { $arrayElemAt : ['$A', '$X'] }, _id : 0 } }]
            // TODO: $arrayToObject
            _ = Mql.Pipeline(collection).Project(x => x.A.ConcatArrays(x.B)); // [{ $project : { _v : { $concatArrays : ['$A', '$B'] }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.A.Filter(i => i > 0)); // [{ $project : { _v : { $filter : { input : '$A', cond : { $gt : ['$$i', 0] }, as : 'i' } }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.A.Filter(i => i > 0, 100)); // [{ $project : { _v : { $filter : { input : '$A', cond : { $gt : ['$$i', 0] }, as : 'i', limit : 100 } }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.A.First()); // [{ $project : { _v : { $first : '$A' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.A.FirstN(100)); // [{ $project : { _v : { $first : { n : 100, input : '$A' } }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.X.In(x.A)); // [{ $project : { _v : { $in : ['$X', '$A'] }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.A.IndexOfArray(x.X)); // [{ $project : { _v : { $indexOfArray : ['$A', '$X'] }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.A.IndexOfArray(x.X, 1)); // [{ $project : { _v : { $indexOfArray : ['$A', '$X', 1] }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.A.IndexOfArray(x.X, 1, 9)); // [{ $project : { _v : { $indexOfArray : ['$A', '$X', 1, 9] }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.IsArray(x.A)); // [{ $project : { _v : { $isArray : '$A' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.A.Last()); // [{ $project : { _v : { $last : '$A' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.A.LastN(100)); // [{ $project : { _v : { $lastN : { n : 100, input : '$A' } }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.A.Map(i => i * 2)); // [{ $project : { _v : { $map : { input : '$A', a : 'i', in : { $multiply : ['$$i', 2] } } }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.A.MaxN(100)); // [{ $project : { _v : { $maxN : { n : 100, input : '$A' } }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.A.MinN(100)); // [{ $project : { _v : { $minN : { n : 100, input : '$A' } }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.ObjectToArray(x)); // [{ $project : { _v : { $objectToArray : '$$ROOT' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.Range(1, 10)); // [{ $project : { _v : { $range : [1, 10] }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.Range(1, 10, 2)); // [{ $project : { _v : { $range : [1, 10, 2] }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.A.Reduce(0, (value, item) => value + item)); // [{ $project : { _v : { $reduce : { input : '$A', initialValue : 0, in : { $add : ['$$value', '$$this'] } } }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.A.ReverseArray()); // [{ $project : { _v : { $reduce : '$A' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.A.Size()); // [{ $project : { _v : { $size : '$A' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.A.Slice(10)); // [{ $project : { _v : { $slice : ['$A', 10] }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.A.Slice(5, 10)); // [{ $project : { _v : { $slice : ['$A', 5, 10] }, _id : 0 } }]
            // TODO: $sortArray
            _ = Mql.Pipeline(collection).Project(x => x.A.ZipMql(x.B)); // [{ $project : { _v : { $zip : { inputs : ['$A', '$B' } }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.A.Zip(x.B, true)); // [{ $project : { _v : { $zip : { inputs : ['$A', '$B' }, useLongestLength : true }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.A.Zip(x.B, true, 0, 0)); // [{ $project : { _v : { $zip : { inputs : ['$A', '$B' }, useLongestLength : true, defaults : [0, 0] }, _id : 0 } }]

            //https://www.mongodb.com/docs/manual/reference/operator/aggregation/#boolean-expression-operators
            _ = Mql.Pipeline(collection).Project(x => x.X == 0 && x.Y == 0); // [{ $project : { _v : { $and : [{ $eq : ['$X', 0] }, { $eq : ['$Y', 0] }] }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => !(x.X == 0)); // [{ $project : { _v : { $not : { $eq : ['$X', 0] } }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.X == 0 || x.Y == 0); // [{ $project : { _v : { $or : [{ $eq : ['$X', 0] }, { $eq : ['$Y', 0] }] }, _id : 0 } }]

            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#comparison-expression-operators
            _ = Mql.Pipeline(collection).Project(x => Mql.Cmp(x.X, x.Y)); // [{ $project : { _v : { $cmp : ['$X', '$Y'] }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.X == x.Y); // [{ $project : { _v : { $eq : ['$X', '$Y'] }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.X > x.Y); // [{ $project : { _v : { $gt : ['$X', '$Y'] }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.X >= x.Y); // [{ $project : { _v : { $gte : ['$X', '$Y'] }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.X < x.Y); // [{ $project : { _v : { $lt : ['$X', '$Y'] }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.X <= x.Y); // [{ $project : { _v : { $lte : ['$X', '$Y'] }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.X != x.Y); // [{ $project : { _v : { $ne : ['$X', '$Y'] }, _id : 0 } }]

            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#conditional-expression-operators
            _ = Mql.Pipeline(collection).Project(x => x.X != 0 ? x.X : x.Y); // [{ $project : { _v : { $cond : { if : { $ne : ['$X', 0] }, then : '$X', else : '$Y' } }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.S ?? "missing"); // [{ $project : { _v : { $ifNull : ['$S', 'missing'] } }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.Switch(Mql.Case(x.X != 0, x.X), Mql.Case(x.Y != 0, x.Y), Mql.Default(-1))); // [{ $project : { _v : { $switch : { branches : [{ case : { $ne : ['$X', 0] }, then : '$X' }, { case : { $ne : ['$Y', 0] }, then : '$Y' }], default : -1 } }, _id : 0 } }]

            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#custom-aggregation-expression-operators
            // TODO: $accumulator
            // TODO: $function

            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#data-size-operators
            _ = Mql.Pipeline(collection).Project(x => Mql.BinarySize(x.S)); // [{ $project : { _v : { $binarySize : '$S' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.BsonSize(x.E)); // [{ $project : { _v : { $bsonSize : '$E' }, _id : 0 } }]

            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#date-expression-operators
            // TODO: all

            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#literal-expression-operator
            _ = Mql.Pipeline(collection).Project(x => Mql.Literal("$X")); // [{ $project : { _v : { $literal : '$X' }, _id : 0 } }]

            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#miscellaneous-operators
            // TODO: $getField
            _ = Mql.Pipeline(collection).Project(x => Mql.Rand()); // [{ $project : { _v : { $rand : {} }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Match(x => Mql.SampleRate(0.33)); // { $match : { $sampleRate : 0.33 } }

            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#object-expression-operators
            // TODO: $mergeObjects
            _ = Mql.Pipeline(collection).Project(x => Mql.ObjectToArray(x)); // [{ $project : { _v : { $objectToArray : '$$ROOT' }, _id : 0 } }]
            // TODO: $setField

            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#set-expression-operators
            _ = Mql.Pipeline(collection).Project(x => x.A.AllElementsTrue()); // [{ $project : { _v : { $allElementsTrue : '$A' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.A.AnyElementTrue()); // [{ $project : { _v : { $anyElementTrue : '$A' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.A.SetDifference(x.B)); // [{ $project : { _v : { $setDifference : ['$A', '$B'] }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.A.SetEquals(x.B)); // [{ $project : { _v : { $setEquals : ['$A', '$B'] }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.A.SetIntersection(x.B)); // [{ $project : { _v : { $setIntersection : ['$A', '$B'] }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.A.SetIsSubset(x.B)); // [{ $project : { _v : { $setIsSubset : ['$A', '$B'] }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.A.SetUnion(x.B)); // [{ $project : { _v : { $setUnion : ['$A', '$B'] }, _id : 0 } }]

            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#string-expression-operators
            _ = Mql.Pipeline(collection).Project(x => x.S + "x"); // [{ $project : { _v : { $concat : ['$S', 'x'] }, _id : 0 } }]
            // TODO: $dateFromString
            // TODO: $dateToString
            _ = Mql.Pipeline(collection).Project(x => x.S.IndexOfBytes("x")); // [{ $project : { _v : { $indexOfBytes : ['$S', 'x'] }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.S.IndexOfBytes("x", 5)); // [{ $project : { _v : { $indexOfBytes : ['$S', 'x', 5] }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.S.IndexOfBytes("x", 5, 10)); // [{ $project : { _v : { $indexOfBytes : ['$S', 'x', 5, 10] }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.S.IndexOfCP("x")); // [{ $project : { _v : { $indexOfCP : ['$S', 'x'] }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.S.IndexOfCP("x", 5)); // [{ $project : { _v : { $indexOfCP : ['$S', 'x', 5] }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.S.IndexOfCP("x", 5, 10)); // [{ $project : { _v : { $indexOfCP : ['$S', 'x', 5, 10] }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.S.LTrim()); // [{ $project : { _v : { $ltrim : { input : '$S' } }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.S.LTrim("x")); // [{ $project : { _v : { $ltrim : { input : '$S', chars : 'x' } }, _id : 0 } }]
            // TODO: $regexFind
            // TODO: $regexFindAll
            // TODO: $regexMatch
            _ = Mql.Pipeline(collection).Project(x => x.S.ReplaceOne("x", "y")); // [{ $project : { _v : { $replaceOne : { input : '$S', find : 'x', replacement : 'y' } }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.S.ReplaceAll("x", "y")); // [{ $project : { _v : { $replaceAll : { input : '$S', find : 'x', replacement : 'y' } }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.S.RTrim()); // [{ $project : { _v : { $rtrim : { input : '$S' } }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.S.RTrim("x")); // [{ $project : { _v : { $rtrim : { input : '$S', chars : 'x' } }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.S.StrLenBytes()); // [{ $project : { _v : { $strLenBytes : '$S' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.S.StrLenCP()); // [{ $project : { _v : { $strLenCP : '$S' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.StrCaseCmp(x.S, "x")); // [{ $project : { _v : { $strcasecmp : ['$S', 'x'] }, _id : 0 } }]
            // $substr is deprecated
            _ = Mql.Pipeline(collection).Project(x => Mql.SubstrBytes(x.S, 1, 2)); // [{ $project : { _v : { $substrBytes : ['$S', 1, 2] }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.SubstrCP(x.S, 1, 2)); // [{ $project : { _v : { $substrCP : ['$S', 1, 2] }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.S.ToLower()); // [{ $project : { _v : { $toLower : '$S' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.X.ToString()); // [{ $project : { _v : { $toString : '$X' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.S.Trim()); // [{ $project : { _v : { $trim : { input : '$S' } }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.S.Trim("x")); // [{ $project : { _v : { $trim : { input : '$S', chars : 'x' } }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => x.S.ToUpper()); // [{ $project : { _v : { $toUpper : '$S' }, _id : 0 } }]

            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#text-expression-operator
            _ = Mql.Pipeline(collection).Project(x => Mql.MetaTextScore()); // [{ $project : { _v : { $meta : 'textScore' }, _id : 0 } }]

            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#timestamp-expression-operators
            // TODO: $tsIncremeent
            // TODO: $tsSecond

            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#trigonometry-expression-operators
            _ = Mql.Pipeline(collection).Project(x => Mql.Sin(x.X)); // [{ $project : { _v : { $sin : '$X' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.Cos(x.X)); // [{ $project : { _v : { $cos : '$X' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.Tan(x.X)); // [{ $project : { _v : { $tan : '$X' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.Asin(x.X)); // [{ $project : { _v : { $asin : '$X' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.Acos(x.X)); // [{ $project : { _v : { $acos : '$X' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.Atan(x.X)); // [{ $project : { _v : { $atan : '$X' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.Atan2(x.Y, x.X)); // [{ $project : { _v : { $atan2 : ['$Y', '$X'] }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.Asinh(x.X)); // [{ $project : { _v : { $asinh : '$X' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.Acosh(x.X)); // [{ $project : { _v : { $acosh : '$X' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.Atanh(x.X)); // [{ $project : { _v : { $atanh : '$X' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.Sinh(x.X)); // [{ $project : { _v : { $sinh : '$X' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.Cosh(x.X)); // [{ $project : { _v : { $cosh : '$X' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.Tanh(x.X)); // [{ $project : { _v : { $tanh : '$X' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.DegreesToRadians(x.X)); // [{ $project : { _v : { $degreesToRadians : '$X' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.RadiansToDegrees(x.X)); // [{ $project : { _v : { $radiansToDegrees : '$X' }, _id : 0 } }]

            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#type-expression-operators
            _ = Mql.Pipeline(collection).Project(x => Mql.Convert<string>(x.X)); // [{ $project : { _v : { $convert : { input : '$X', to : 'string' } }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.Convert(x.X, "null")); // [{ $project : { _v : { $convert : { input : '$X', to : 'string', onNull : 'null' } }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.Convert(x.X, "null", "error")); // [{ $project : { _v : { $convert : { input : '$X', to : 'string', onNull : 'null', onError : 'error' } }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.IsNumber(x.X)); // [{ $project : { _v : { $isNumber : '$X' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.ToBool(x.X)); // [{ $project : { _v : { $toString : '$X' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.ToDate(x.X)); // [{ $project : { _v : { $toDate : '$X' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.ToDecimal(x.X)); // [{ $project : { _v : { $toDecimal : '$X' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.ToDouble(x.X)); // [{ $project : { _v : { $toDouble : '$X' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.ToInt(x.X)); // [{ $project : { _v : { $toInt : '$X' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.ToLong(x.X)); // [{ $project : { _v : { $toLong : '$X' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.ToObjectId(x.X)); // [{ $project : { _v : { $toObjectId : '$X' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.ToString(x.X)); // [{ $project : { _v : { $toString : '$X' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.Type(x.X)); // [{ $project : { _v : { $type : '$X' }, _id : 0 } }]

            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#accumulators---group---bucket---bucketauto---setwindowfields-
            // TODO: $accumulator
            _ = Mql.Pipeline(collection).Group(x => new { _id = x.X, R = Mql.AddToSetAccumulator(x.Y) }); // [{ $group : { _id : '$X', R : { $addToSet : '$Y' } } } }]
            _ = Mql.Pipeline(collection).Group(x => new { _id = x.X, R = Mql.AvgAccumulator(x.Y) }); // [{ $group : { _id : '$X', R : { $avg : '$Y' } } } }]
            // TODO: $bottom
            // TODO: $bottomN
            _ = Mql.Pipeline(collection).Group(x => new { _id = x.X, R = Mql.CountAccumulator() }); // [{ $group : { _id : '$X', R : { $count : { } } } } }]
            _ = Mql.Pipeline(collection).Group(x => new { _id = x.X, R = Mql.FirstAccumulator(x.Y) }); // [{ $group : { _id : '$X', R : { $first : '$Y' } } } }]
            _ = Mql.Pipeline(collection).Group(x => new { _id = x.X, R = Mql.FirstNAccumulator(x.Y, 10) }); // [{ $group : { _id : '$X', R : { $firstN : { input : '$Y', n : 10 } } } } }]
            _ = Mql.Pipeline(collection).Group(x => new { _id = x.X, R = Mql.LastAccumulator(x.Y) }); // [{ $group : { _id : '$X', R : { $last : '$Y' } } } }]
            _ = Mql.Pipeline(collection).Group(x => new { _id = x.X, R = Mql.LastNAccumulator(x.Y, 10) }); // [{ $group : { _id : '$X', R : { $lastN : { input : '$Y', n : 10 } } } } }]
            _ = Mql.Pipeline(collection).Group(x => new { _id = x.X, R = Mql.MaxAccumulator(x.Y) }); // [{ $group : { _id : '$X', R : { $max : '$Y' } } } }]
            _ = Mql.Pipeline(collection).Group(x => new { _id = x.X, R = Mql.MaxNAccumulator(x.Y, 10) }); // [{ $group : { _id : '$X', R : { $maxN : { input : '$Y', n : 10 } } } } }]
            // TODO: $mergeObjects
            _ = Mql.Pipeline(collection).Group(x => new { _id = x.X, R = Mql.MinAccumulator(x.Y) }); // [{ $group : { _id : '$X', R : { $min : '$Y' } } } }]
            _ = Mql.Pipeline(collection).Group(x => new { _id = x.X, R = Mql.MinNAccumulator(x.Y, 10) }); // [{ $group : { _id : '$X', R : { $minN : { input : '$Y', n : 10 } } } } }]
            _ = Mql.Pipeline(collection).Group(x => new { _id = x.X, R = Mql.PushAccumulator(x.Y) }); // [{ $group : { _id : '$X', R : { $push : '$Y' } } } }]
            _ = Mql.Pipeline(collection).Group(x => new { _id = x.X, R = Mql.StdDevPopAccumulator(x.Y) }); // [{ $group : { _id : '$X', R : { $stdDevPop : '$Y' } } } }]
            _ = Mql.Pipeline(collection).Group(x => new { _id = x.X, R = Mql.StdDevSampAccumulator(x.Y) }); // [{ $group : { _id : '$X', R : { $stdDevSamp : '$Y' } } } }]
            _ = Mql.Pipeline(collection).Group(x => new { _id = x.X, R = Mql.SumAccumulator(x.Y) }); // [{ $group : { _id : '$X', R : { $sum : '$Y' } } } }]
            // TODO: $top
            // TODO: $topN

            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#accumulators--in-other-stages-
            _ = Mql.Pipeline(collection).Project(x => Mql.Avg(x.A)); // [{ $project : { _v : { $avg : '$A' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.Max(x.A)); // [{ $project : { _v : { $max : '$A' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.Min(x.A)); // [{ $project : { _v : { $min : '$A' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.StdDevPop(x.A)); // [{ $project : { _v : { $stdDevPop : '$A' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.StdDevSamp(x.A)); // [{ $project : { _v : { $stdDevPop : '$A' }, _id : 0 } }]
            _ = Mql.Pipeline(collection).Project(x => Mql.Sum(x.A)); // [{ $project : { _v : { $stdDevPop : '$A' }, _id : 0 } }]

            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#variable-expression-operators
            // TODO: $let

            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#window-operators
            // TODO: all
        }

        [Fact]
        public void Abs_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#arithmetic-expression-operators
            var collection = CreateCollection();
            Assert(
                Mql.Pipeline(collection).Project(x => Mql.Abs(x.X)),
                "{ $project : { _v : { $abs : '$X' }, _id : 0 } }");
        }

        [Fact]
        public void Add_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#arithmetic-expression-operators
            var collection = CreateCollection();
            Assert(
                Mql.Pipeline(collection).Project(x => x.X + x.Y),
                "{ $project : { _v : { $add : ['$X', '$Y'] }, _id : 0 } }");
        }

        [Fact]
        public void Ceil_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#arithmetic-expression-operators
            var collection = CreateCollection();
            Assert(
                Mql.Pipeline(collection).Project(x => Mql.Ceil(x.D)),
                "{ $project : { _v : { $ceil : '$D' }, _id : 0 } }");
        }

        [Fact]
        public void Divide_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#arithmetic-expression-operators
            var collection = CreateCollection();
            Assert(
                Mql.Pipeline(collection).Project(x => x.X / x.Y),
                "{ $project : { _v : { $divide : ['$X', '$Y'] }, _id : 0 } }");
        }

        [Fact]
        public void Exp_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#arithmetic-expression-operators
            var collection = CreateCollection();
            Assert(
                Mql.Pipeline(collection).Project(x => Mql.Exp(x.X)),
                "{ $project : { _v : { $exp : '$X' }, _id : 0 } }");
        }

        [Fact]
        public void Floor_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#arithmetic-expression-operators
            var collection = CreateCollection();
            Assert(
                Mql.Pipeline(collection).Project(x => Mql.Floor(x.D)),
                "{ $project : { _v : { $floor : '$D' }, _id : 0 } }");
        }

        [Fact]
        public void Ln_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#arithmetic-expression-operators
            var collection = CreateCollection();
            Assert(
                Mql.Pipeline(collection).Project(x => Mql.Ln(x.D)),
                "{ $project : { _v : { $ln : '$D' }, _id : 0 } }");
        }

        [Fact]
        public void Log_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#arithmetic-expression-operators
            var collection = CreateCollection();
            Assert(
                Mql.Pipeline(collection).Project(x => Mql.Log(x.D, 10)),
                "{ $project : { _v : { $log : ['$D', { $numberLong : 10 }] }, _id : 0 } }");
        }

        [Fact]
        public void Log10_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#arithmetic-expression-operators
            var collection = CreateCollection();
            Assert(
                Mql.Pipeline(collection).Project(x => Mql.Log10(x.D)),
                "{ $project : { _v : { $log10 : '$D' }, _id : 0 } }");
        }

        [Fact]
        public void Mod_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#arithmetic-expression-operators
            var collection = CreateCollection();
            Assert(
                Mql.Pipeline(collection).Project(x => x.X % x.Y),
                "{ $project : { _v : { $mod : ['$X', '$Y'] }, _id : 0 } }");
        }

        [Fact]
        public void MqlDivide_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#arithmetic-expression-operators
            var collection = CreateCollection();
            Assert(
                Mql.Pipeline(collection).Project(x => Mql.Divide(x.X, x.Y)),
                "{ $project : { _v : { $divide : ['$X', '$Y'] }, _id : 0 } }");
        }

        [Fact]
        public void Multiply_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#arithmetic-expression-operators
            var collection = CreateCollection();
            Assert(
                Mql.Pipeline(collection).Project(x => x.X * x.Y),
                "{ $project : { _v : { $multiply : ['$X', '$Y'] }, _id : 0 } }");
        }

        [Fact]
        public void Pow_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#arithmetic-expression-operators
            var collection = CreateCollection();
            Assert(
                Mql.Pipeline(collection).Project(x => Mql.Pow(x.D, x.X)),
                "{ $project : { _v : { $pow : ['$D', '$X'] }, _id : 0 } }");
        }

        [Fact]
        public void RoundToDouble_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#arithmetic-expression-operators
            var collection = CreateCollection();
            Assert(
                Mql.Pipeline(collection).Project(x => Mql.RoundToDouble(x.D, 2)),
                "{ $project : { _v : { $round : ['$D', 2] }, _id : 0 } }");
        }

        [Fact]
        public void RoundToInteger_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#arithmetic-expression-operators
            var collection = CreateCollection();
            Assert(
                Mql.Pipeline(collection).Project(x => Mql.RoundToInteger(x.D)),
                "{ $project : { _v : { $round : '$D' }, _id : 0 } }");
        }

        [Fact]
        public void RoundToInteger_with_place_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#arithmetic-expression-operators
            var collection = CreateCollection();
            Assert(
                Mql.Pipeline(collection).Project(x => Mql.RoundToInteger(x.D, -2)),
                "{ $project : { _v : { $round : ['$D', -2] }, _id : 0 } }");
        }

        [Fact]
        public void Sqrt_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#arithmetic-expression-operators
            var collection = CreateCollection();
            Assert(
                Mql.Pipeline(collection).Project(x => Mql.Sqrt(x.D)),
                "{ $project : { _v : { $sqrt : '$D' }, _id : 0 } }");
        }

        [Fact]
        public void Subtract_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#arithmetic-expression-operators
            var collection = CreateCollection();
            Assert(
                Mql.Pipeline(collection).Project(x => x.X - x.Y),
                "{ $project : { _v : { $subtract : ['$X', '$Y'] }, _id : 0 } }");
        }

        [Fact]
        public void TruncToDouble_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#arithmetic-expression-operators
            var collection = CreateCollection();
            Assert(
                Mql.Pipeline(collection).Project(x => Mql.TruncToDouble(x.D, 2)),
                "{ $project : { _v : { $trunc : ['$D', 2] }, _id : 0 } }");
        }

        [Fact]
        public void TruncToInteger_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#arithmetic-expression-operators
            var collection = CreateCollection();
            Assert(
                Mql.Pipeline(collection).Project(x => Mql.TruncToInteger(x.D)),
                "{ $project : { _v : { $trunc : '$D' }, _id : 0 } }");
        }

        [Fact]
        public void TruncToInteger_with_place_Example()
        {
            // https://www.mongodb.com/docs/manual/reference/operator/aggregation/#arithmetic-expression-operators
            var collection = CreateCollection();
            Assert(
                Mql.Pipeline(collection).Project(x => Mql.TruncToInteger(x.D, -2)),
                "{ $project : { _v : { $trunc : ['$D', -2] }, _id : 0 } }");
        }

        private void Assert<TInput, TOutput>(MqlPipeline<TInput, TOutput> pipeline, params string[] expectedStages)
        {
            var stages = TranslatePipeline(pipeline);
            AssertStages(stages, expectedStages);
        }

        private IMongoCollection<C> CreateCollection()
        {
            var collection = GetCollection<C>();

            CreateCollection(
                collection,
                new C { Id = 1, X = 1, Y = 1 },
                new C { Id = 2, X = 2, Y = 2 });

            return collection;
        }

        public class C
        {
            public int Id { get; set; }
            public int[] A { get; set; }
            public int[] B { get; set; }
            public C E { get; set; }
            public double D { get; set; }
            public string S { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
        }

        public class D
        {
            public int X { get; set; }
        }
    }
}
