﻿/* Copyright 2020-present MongoDB Inc.
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
using MongoDB.Bson;

namespace MongoDB.Driver.Tests.Specifications.unified_test_format.UnifiedTestOperations
{
    public class OperationResult
    {
        private Exception _exception;
        private BsonValue _result;

        public OperationResult(Exception exception)
        {
            _exception = exception;
        }

        public OperationResult(BsonValue result)
        {
            _result = result;
        }

        public Exception Exception => _exception;
        public BsonValue Result => _result;
    }
}
