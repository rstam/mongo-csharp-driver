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

namespace MongoDB.Driver.MqlApi.Translators.Context
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

        public MqlTranslationContext WithSymbol(MqlSymbol newSymbol)
        {
            var newSymbolTable = _symbolTable.WithSymbol(newSymbol);
            return new MqlTranslationContext(newSymbolTable, _nameGenerator);
        }
    }
}
