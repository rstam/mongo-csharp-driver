﻿/* Copyright 2010-present MongoDB Inc.
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

using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Linq.Linq3Implementation.Ast.Filters
{
    internal sealed class AstAllFilterOperation : AstFilterOperation
    {
        private readonly IReadOnlyList<BsonValue> _values;

        public AstAllFilterOperation(IEnumerable<BsonValue> values)
        {
            _values = Ensure.IsNotNull(values, nameof(values)).ToList().AsReadOnly();
        }

        public override AstNodeType NodeType => AstNodeType.AllFilterOperation;
        public IReadOnlyList<BsonValue> Values => _values;

        public override BsonValue Render()
        {
            return new BsonDocument("$all", new BsonArray(_values));
        }
    }
}