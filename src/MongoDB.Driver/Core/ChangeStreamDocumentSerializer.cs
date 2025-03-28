﻿/* Copyright 2017-present MongoDB Inc.
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
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver
{
    /// <summary>
    /// A serializer for ChangeStreamDocument instances.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public class ChangeStreamDocumentSerializer<TDocument> : BsonDocumentBackedClassSerializer<ChangeStreamDocument<TDocument>>
    {
        // private fields
        private readonly IBsonSerializer<TDocument> _documentSerializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeStreamDocumentSerializer{TDocument}"/> class.
        /// </summary>
        /// <param name="documentSerializer">The document serializer.</param>
        public ChangeStreamDocumentSerializer(
            IBsonSerializer<TDocument> documentSerializer)
        {
            _documentSerializer = Ensure.IsNotNull(documentSerializer, nameof(documentSerializer));

            RegisterMember("ClusterTime", "clusterTime", BsonTimestampSerializer.Instance);
            RegisterMember("CollectionNamespace", "ns", ChangeStreamDocumentCollectionNamespaceSerializer.Instance);
            RegisterMember("CollectionUuid", "collectionUUID", GuidSerializer.StandardInstance);
            RegisterMember("DatabaseNamespace", "ns", ChangeStreamDocumentDatabaseNamespaceSerializer.Instance);
            RegisterMember("DocumentKey", "documentKey", BsonDocumentSerializer.Instance);
            RegisterMember("FullDocument", "fullDocument", _documentSerializer);
            RegisterMember("FullDocumentBeforeChange", "fullDocumentBeforeChange", _documentSerializer);
            RegisterMember("NamespaceType", "nsType", ChangeStreamNamespaceTypeSerializer.Instance);
            RegisterMember("OperationDescription", "operationDescription", BsonDocumentSerializer.Instance);
            RegisterMember("OperationType", "operationType", ChangeStreamOperationTypeSerializer.Instance);
            RegisterMember("RenameTo", "to", ChangeStreamDocumentCollectionNamespaceSerializer.Instance);
            RegisterMember("ResumeToken", "_id", BsonDocumentSerializer.Instance);
            RegisterMember("SplitEvent", "splitEvent", ChangeStreamSplitEventSerializer.Instance);
            RegisterMember("UpdateDescription", "updateDescription", ChangeStreamUpdateDescriptionSerializer.Instance);
            RegisterMember("WallTime", "wallTime", DateTimeSerializer.UtcInstance);
        }

        // public methods
        /// <inheritdoc />
        public override ChangeStreamDocument<TDocument> Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            context = context.With(b => b.AllowDuplicateElementNames = true);
            return base.Deserialize(context, args);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(obj, null)) { return false; }
            if (object.ReferenceEquals(this, obj)) { return true; }
            return
                base.Equals(obj) &&
                obj is ChangeStreamDocumentSerializer<TDocument> other &&
                object.Equals(_documentSerializer, other._documentSerializer);
        }

        /// <inheritdoc/>
        public override int GetHashCode() => 0;

        // protected methods
        /// <inheritdoc />
        protected override ChangeStreamDocument<TDocument> CreateInstance(BsonDocument backingDocument)
        {
            return new ChangeStreamDocument<TDocument>(backingDocument, _documentSerializer);
        }
    }
}
