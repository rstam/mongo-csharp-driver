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
    public class MqlExpressionNotSupportedException : Exception
    {
        #region static
        private static string FormatMessage(Expression expression)
        {
            return $"Expression {expression} is not supported.";
        }

        private static string FormatMessage(Expression expression, Expression containingExpression)
        {
            return $"Expression {expression} in {containingExpression} is not supported.";
        }

        private static string FormatMessage(Expression expression, string because)
        {
            return $"Expression {expression} is not supported because {because}.";
        }

        private static string FormatMessage(Expression expression, Expression containingExpression, string because)
        {
            return $"Expression {expression} in {containingExpression} is not supported because {because}.";
        }
        #endregion

        private readonly string _because;
        private readonly Expression _containingExpression;
        private readonly Expression _expression;

        public MqlExpressionNotSupportedException(Expression expression)
            : base(FormatMessage(expression))
        {
            _expression = Ensure.IsNotNull(expression, nameof(expression));
        }

        public MqlExpressionNotSupportedException(Expression expression, Expression containingExpression)
            : base(FormatMessage(expression, containingExpression))
        {
            _expression = Ensure.IsNotNull(expression, nameof(expression));
            _containingExpression = Ensure.IsNotNull(containingExpression, nameof(containingExpression));
        }

        public MqlExpressionNotSupportedException(Expression expression, string because)
            : base(FormatMessage(expression, because))
        {
            _expression = Ensure.IsNotNull(expression, nameof(expression));
            _because = Ensure.IsNotNullOrEmpty(because, nameof(because));
        }

        public MqlExpressionNotSupportedException(Expression expression, Expression containingExpression, string because)
            : base(FormatMessage(expression, containingExpression, because))
        {
            _expression = Ensure.IsNotNull(expression, nameof(expression));
            _because = Ensure.IsNotNullOrEmpty(because, nameof(because));
        }

        public string Because => _because;
        public Expression ContainingExpressoin => _containingExpression;
        public Expression Expression => _expression;
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
