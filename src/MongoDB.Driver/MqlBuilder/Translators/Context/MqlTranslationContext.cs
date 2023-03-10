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

using System.Linq;
using System.Linq.Expressions;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Expressions;
using MongoDB.Driver.Linq.Linq3Implementation.Misc;

namespace MongoDB.Driver.MqlBuilder.Translators.Context
{
    internal class MqlTranslationContext
    {
        #region static
        public static MqlTranslationContext Create()
        {
            var symbolTable = new MqlSymbolTable();
            var nameGenerator = new NameGenerator();
            return new MqlTranslationContext(symbolTable, nameGenerator);
        }
        #endregion

        private readonly NameGenerator _nameGenerator = new NameGenerator();
        private readonly MqlSymbolTable _symbolTable;

        public MqlTranslationContext(MqlSymbolTable symbolTable, NameGenerator nameGenerator)
        {
            _symbolTable = Ensure.IsNotNull(symbolTable, nameof(symbolTable));
            _nameGenerator = Ensure.IsNotNull(nameGenerator, nameof(nameGenerator));
        }

        public NameGenerator NameGenerator => _nameGenerator;
        public MqlSymbolTable SymbolTable => _symbolTable;

        public MqlSymbol CreateRootSymbol(ParameterExpression parameter, IBsonSerializer rootSerializer)
        {
            var ast = AstExpression.Var("ROOT", isCurrent: true);
            var name = _nameGenerator.GetParameterName(parameter);
            return new MqlSymbol(parameter, name, ast, rootSerializer, isCurrent: true);
        }

        public MqlSymbol CreateSymbol(ParameterExpression parameter, IBsonSerializer serializer, bool isCurrent = false)
        {
            var parameterName = _nameGenerator.GetParameterName(parameter);
            return CreateSymbol(parameter, name: parameterName, serializer, isCurrent);
        }

        public MqlSymbol CreateSymbol(ParameterExpression parameter, string name, IBsonSerializer serializer, bool isCurrent = false)
        {
            var varName = _nameGenerator.GetVarName(name);
            return CreateSymbol(parameter, name, varName, serializer, isCurrent);
        }

        public MqlSymbol CreateSymbol(ParameterExpression parameter, string name, string varName, IBsonSerializer serializer, bool isCurrent = false)
        {
            var varAst = AstExpression.Var(varName, isCurrent);
            return CreateSymbol(parameter, name, varAst, serializer, isCurrent);
        }

        public MqlSymbol CreateSymbol(ParameterExpression parameter, AstExpression ast, IBsonSerializer serializer, bool isCurrent = false)
        {
            var parameterName = _nameGenerator.GetParameterName(parameter);
            return CreateSymbol(parameter, name: parameterName, ast, serializer, isCurrent);
        }

        public MqlSymbol CreateSymbol(ParameterExpression parameter, string name, AstExpression ast, IBsonSerializer serializer, bool isCurrent = false)
        {
            return new MqlSymbol(parameter, name, ast, serializer, isCurrent);
        }

        public MqlSymbol CreateSymbolWithVarName(ParameterExpression parameter, string varName, IBsonSerializer serializer, bool isCurrent = false)
        {
            var parameterName = _nameGenerator.GetParameterName(parameter);
            return CreateSymbol(parameter, name: parameterName, varName, serializer, isCurrent);
        }

        public (MqlTranslationContext context, string varName) WithParameterSymbol(LambdaExpression lambdaExpression, IBsonSerializer parameterSerializer)
        {
            var parameter = lambdaExpression.Parameters.Single();
            var newSymbol = CreateSymbol(parameter, parameterSerializer);
            var newContext = WithSymbol(newSymbol);
            return (newContext, newSymbol.Var.Name);
        }

        public MqlTranslationContext WithSingleSymbol(MqlSymbol newSymbol)
        {
            var newSymbolTable = new MqlSymbolTable(newSymbol);
            return WithSymbolTable(newSymbolTable);
        }

        public MqlTranslationContext WithSymbol(MqlSymbol newSymbol)
        {
            var newSymbolTable = _symbolTable.WithSymbol(newSymbol);
            return new MqlTranslationContext(newSymbolTable, _nameGenerator);
        }

        public MqlTranslationContext WithSymbolTable(MqlSymbolTable symbolTable)
        {
            return new MqlTranslationContext(symbolTable, _nameGenerator);
        }
    }
}
