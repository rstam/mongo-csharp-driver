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
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.MqlBuilder.Translators.ExpressionToFilterTranslators;

namespace MongoDB.Driver.MqlBuilder
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class MqlFilter<TDocument>
    {
        private readonly IBsonSerializer<TDocument> _documentSerializer;
        private readonly Expression<Func<TDocument, bool>> _predicate;

        public MqlFilter(IBsonSerializer<TDocument> documentSerializer, Expression<Func<TDocument, bool>> predicate)
        {
            _predicate = predicate;
            _documentSerializer = documentSerializer;
        }

        public IBsonSerializer<TDocument> DocumentSerializer => _documentSerializer;
        public Expression<Func<TDocument, bool>> Predicate => _predicate;

        public static implicit operator FilterDefinition<TDocument>(MqlFilter<TDocument> filter)
        {
            var astFilter = MqlFilterTranslator.Translate(filter);
            var renderedFilter = astFilter.Render();
            var filterDocument = (BsonDocument)renderedFilter;
            return new BsonDocumentFilterDefinition<TDocument>(filterDocument);
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
