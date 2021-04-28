﻿/* Copyright 2016-present MongoDB Inc.
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

using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Linq.Expressions;
using MongoDB.Driver.Linq2.Expressions;

namespace MongoDB.Driver.Linq2.Processors.EmbeddedPipeline.MethodCallBinders
{
    internal class AsQueryableBinder : IMethodCallBinder<EmbeddedPipelineBindingContext>
    {
        public static IEnumerable<MethodInfo> GetSupportedMethods()
        {
            yield return MethodHelper.GetMethodDefinition(() => Queryable.AsQueryable(null));
            yield return MethodHelper.GetMethodDefinition(() => Queryable.AsQueryable<object>(null));
        }

        public Expression Bind(
            PipelineExpression pipeline,
            EmbeddedPipelineBindingContext bindingContext,
            MethodCallExpression node,
            IEnumerable<Expression> arguments)
        {
            return pipeline;
        }
    }
}