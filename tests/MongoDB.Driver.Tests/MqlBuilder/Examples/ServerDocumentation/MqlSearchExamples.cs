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
using Xunit;

namespace MongoDB.Driver.Tests.MqlBuilder.Examples.ServerDocumentation
{
    public class MqlSearchExamples : MqlIntegrationTest
    {
        [Fact]
        public void Search_examples()
        {
            var collection = CreateCollection();

            // var stage = MqlStage.Search();
        }

        private IMongoCollection<C> CreateCollection()
        {
            var collection = GetCollection<C>();

            CreateCollection(
                collection,
                new C { Id = 1, X = 1, Y = 1 },
                new C { Id = 2, X = 2, Y = 2 });

            return collection;
        }

        public class C
        {
            public int Id { get; set; }
            public int[] A { get; set; }
            public DateTime D { get; set; }
            public string S { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
        }
    }
}
