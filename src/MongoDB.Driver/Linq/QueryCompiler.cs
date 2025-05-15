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
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Expressions;
using MongoDB.Driver.Linq.Linq3Implementation.Misc;
using MongoDB.Driver.Linq.Linq3Implementation.Translators;
using MongoDB.Driver.Linq.Linq3Implementation.Translators.ExpressionToExecutableQueryTranslators;
using MongoDB.Driver.Linq.Linq3Implementation.Translators.ExpressionToPipelineTranslators;

namespace MongoDB.Driver.Linq;

/// <summary>
///
/// </summary>
public static class QueryCompiler
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="expression"></param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static CompiledQuery<TResult> CompileQuery<TResult>(Expression<Func<IQueryable<TResult>>> expression)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="translationOptions"></param>
    /// <typeparam name="TParameter"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static CompiledQueryBase<TParameter, TResult> CompileQuery<TParameter, TResult>(
        Expression<Func<TParameter, IQueryable<TResult>>> expression,
        ExpressionTranslationOptions translationOptions = null)
    {
        var body = PartialEvaluator.EvaluatePartially(expression.Body);

        var context = TranslationContext.Create(translationOptions);
        context = AddParameters(context, expression.Parameters);
        var pipeline = ExpressionToPipelineTranslator.Translate(context, body);

        throw new NotImplementedException();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="expression"></param>
    /// <typeparam name="TParameter1"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TParameter2"></typeparam>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static CompiledQueryBase<TParameter1, TParameter2, TResult> CompileQuery<TParameter1, TParameter2, TResult>(Expression<Func<TParameter1, TParameter2, IQueryable<TResult>>> expression)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="expression"></param>
    /// <typeparam name="TParameter"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static CompiledScalarQuery<TParameter, TResult> CompileScalarQuery<TParameter, TResult>(Expression<Func<TParameter, TResult>> expression)
    {
        throw new NotImplementedException();
    }

    private static TranslationContext AddParameters(TranslationContext context, ReadOnlyCollection<ParameterExpression> parameters)
    {
        var symbols = new Symbol[parameters.Count];
        for (var i = 0; i < parameters.Count; i++)
        {
            var parameter = parameters[i];
            var name = parameter.Name; // TODO: make safe name
            var ast = AstExpression.QueryParameter(parameter, name);
            var symbol = new Symbol(parameter, name, ast, serializer: null, isCurrent: false);
            symbols[i] = symbol;
        }

        return context.WithSymbols(symbols);
    }
}
