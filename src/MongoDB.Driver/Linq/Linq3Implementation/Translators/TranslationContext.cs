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

using System.Linq.Expressions;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Expressions;
using MongoDB.Driver.Linq.Linq3Implementation.Misc;

namespace MongoDB.Driver.Linq.Linq3Implementation.Translators
{
    internal class TranslationContext
    {
        // private fields
        private readonly NameGenerator _nameGenerator;
        private readonly SymbolTable _symbolTable;

        // constructors
        public TranslationContext()
            : this(new SymbolTable(), new NameGenerator())
        {
        }

        public TranslationContext(SymbolTable symbolTable, NameGenerator nameGenerator)
        {
            _symbolTable = Ensure.IsNotNull(symbolTable, nameof(symbolTable));
            _nameGenerator = Ensure.IsNotNull(nameGenerator, nameof(nameGenerator));
        }

        // public properties
        public NameGenerator NameGenerator => _nameGenerator;
        public SymbolTable SymbolTable => _symbolTable;

        // public methods
        public Symbol CreateExpressionSymbol(ParameterExpression parameter, IBsonSerializer serializer, bool isCurrent = false)
        {
            var parameterName = _nameGenerator.GetParameterName(parameter);
            return CreateExpressionSymbol(parameter, name: parameterName, serializer, isCurrent);
        }

        public Symbol CreateExpressionSymbol(ParameterExpression parameter, string name, IBsonSerializer serializer, bool isCurrent = false)
        {
            var varName = _nameGenerator.GetVarName(name);
            return CreateExpressionSymbol(parameter, name, varName, serializer, isCurrent);
        }

        public Symbol CreateExpressionSymbol(ParameterExpression parameter, string name, string varName, IBsonSerializer serializer, bool isCurrent = false)
        {
            var expression = AstExpression.Var(varName, isCurrent);
            return CreateExpressionSymbol(parameter, name, expression, serializer, isCurrent);
        }

        public Symbol CreateExpressionSymbol(ParameterExpression parameter, string name, AstExpression expression, IBsonSerializer serializer, bool isCurrent = false)
        {
            return new Symbol(parameter, name, expression, serializer, isCurrent);
        }

        public Symbol CreateExpressionSymbolWithVarName(ParameterExpression parameter, string varName, IBsonSerializer serializer, bool isCurrent = false)
        {
            var parameterName = _nameGenerator.GetParameterName(parameter);
            return CreateExpressionSymbol(parameter, name: parameterName, varName, serializer, isCurrent);
        }

        public Symbol CreateFilterSymbol(ParameterExpression parameter, IBsonSerializer serializer, bool isCurrent = false)
        {
            var parameterName = _nameGenerator.GetParameterName(parameter);
            return CreateFilterSymbol(parameter, name: parameterName, serializer, isCurrent);
        }

        public Symbol CreateFilterSymbol(ParameterExpression parameter, string name, IBsonSerializer serializer, bool isCurrent = false)
        {
            var rootVar = AstExpression.Var("ROOT", isCurrent);
            return new Symbol(parameter, name, expression: rootVar, serializer, isCurrent);
        }

        public override string ToString()
        {
            return $"{{ SymbolTable : {_symbolTable} }}";
        }

        public TranslationContext WithSingleSymbol(Symbol newSymbol)
        {
            var newSymbolTable = new SymbolTable(newSymbol);
            return WithSymbolTable(newSymbolTable);
        }

        public TranslationContext WithSymbol(Symbol newSymbol)
        {
            var newSymbolTable = _symbolTable.WithSymbol(newSymbol);
            return WithSymbolTable(newSymbolTable);
        }

        public TranslationContext WithSymbols(params Symbol[] newSymbols)
        {
            var newSymbolTable = _symbolTable.WithSymbols(newSymbols);
            return WithSymbolTable(newSymbolTable);
        }

        public TranslationContext WithSymbolTable(SymbolTable symbolTable)
        {
            return new TranslationContext(symbolTable, _nameGenerator);
        }
    }
}
