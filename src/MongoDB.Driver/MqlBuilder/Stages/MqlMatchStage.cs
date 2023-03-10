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
using System.Linq.Expressions;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.MqlBuilder
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public interface IMqlMatchStage
    {
        LambdaExpression Predicate { get; }
    }

    public class MqlMatchStage<TInput> : MqlStage<TInput, TInput>, IMqlMatchStage
    {
        private readonly Expression<Func<TInput, bool>> _predicate;

        public MqlMatchStage(Expression<Func<TInput, bool>> predicate)
        {
            _predicate = Ensure.IsNotNull(predicate, nameof(predicate));
        }

        public Expression<Func<TInput, bool>> Projection => _predicate;
        public override MqlStageType StageType => MqlStageType.Match;

        LambdaExpression IMqlMatchStage.Predicate => _predicate;
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
