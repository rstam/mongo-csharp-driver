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

namespace MongoDB.Driver.Linq.Linq3Implementation.Ast.Expressions
{
    internal enum AstAccumulatorOperator
    {
        AddToSet,
        Avg,
        First,
        Last,
        Max,
        MergeObjects,
        Min,
        Push,
        StdDevPop,
        StdDevSamp,
        Sum
    }

    internal static class AstAccumulatorOperatorExtensions
    {
        public static string Render(this AstAccumulatorOperator @operator)
        {
            switch (@operator)
            {
                case AstAccumulatorOperator.AddToSet: return "$addToSet";
                case AstAccumulatorOperator.Avg: return "$avg";
                case AstAccumulatorOperator.First: return "$first";
                case AstAccumulatorOperator.Last: return "$last";
                case AstAccumulatorOperator.Max: return "$max";
                case AstAccumulatorOperator.MergeObjects: return "$mergeObjects";
                case AstAccumulatorOperator.Min: return "$min";
                case AstAccumulatorOperator.Push: return "$push";
                case AstAccumulatorOperator.StdDevPop: return "$stdDevPop";
                case AstAccumulatorOperator.StdDevSamp: return "$stdDevSamp";
                case AstAccumulatorOperator.Sum: return "$sum";
                default: throw new InvalidOperationException($"Unexpected accumulator operator: {@operator}.");
            }
        }
    }
}
