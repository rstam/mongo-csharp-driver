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

using MongoDB.Bson;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Linq3.Ast.Stages
{
    public sealed class AstListSessionsStage : AstPipelineStage
    {
        private readonly BsonDocument _options;

        public AstListSessionsStage(BsonDocument options)
        {
            _options = Ensure.IsNotNull(options, nameof(options));
        }

        public override AstNodeType NodeType => AstNodeType.ListSessionsStage;
        public BsonDocument Options => _options;

        public override BsonValue Render()
        {
            return new BsonDocument("$listSessions", _options);
        }
    }
}