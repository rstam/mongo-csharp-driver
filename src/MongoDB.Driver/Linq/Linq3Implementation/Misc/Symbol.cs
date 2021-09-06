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

namespace MongoDB.Driver.Linq.Linq3Implementation.Misc
{
    internal class Symbol
    {
        // private fields
        private readonly AstExpression _expression;
        private readonly bool _isCurrent;
        private readonly string _name;
        private readonly ParameterExpression _parameter;
        private readonly IBsonSerializer _serializer;

        // constructors
        public Symbol(ParameterExpression parameter, string name, AstExpression expression, IBsonSerializer serializer, bool isCurrent)
        {
            _parameter = Ensure.IsNotNull(parameter, nameof(parameter));
            _name = Ensure.IsNotNullOrEmpty(name, nameof(name));
            _expression = expression; // can be null
            _serializer = Ensure.IsNotNull(serializer, nameof(serializer));
            _isCurrent = isCurrent;
        }

        // public properties
        public AstExpression Expression => _expression;
        public bool IsCurrent => _isCurrent;
        public string Name => _name;
        public ParameterExpression Parameter => _parameter;
        public IBsonSerializer Serializer => _serializer;
        public AstVarExpression Var => (AstVarExpression)_expression;

        // public methods
        public Symbol AsNotCurrent()
        {
            if (_isCurrent)
            {
                if (_expression is AstVarExpression varExpression)
                {
                    return new Symbol(_parameter, _name, varExpression.AsNotCurrent(), _serializer, isCurrent: false);
                }
                else
                {
                    return new Symbol(_parameter, _name, _expression, _serializer, isCurrent: false);
                }
            }

            return this;
        }

        public override string ToString() => _expression?.Render().AsString ?? _parameter.Name ?? "<noname>";
    }
}
