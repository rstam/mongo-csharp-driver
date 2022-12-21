using System;
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace MongoDB.Driver.MqlApi
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class MqlFilter<TDocument>
    {
        private readonly Expression<Func<TDocument, bool>> _predicate;
        private readonly IBsonSerializer<TDocument> _documentSerializer;

        public MqlFilter(IBsonSerializer<TDocument> documentSerializer, Expression<Func<TDocument, bool>> predicate)
        {
            _predicate = predicate;
            _documentSerializer = documentSerializer;
        }

        public BsonDocument Translate()
        {
            throw new NotImplementedException();
        }

        public static implicit operator FilterDefinition<TDocument>(MqlFilter<TDocument> filter)
        {
            var translation = filter.Translate();
            return new BsonDocumentFilterDefinition<TDocument>(translation);
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
