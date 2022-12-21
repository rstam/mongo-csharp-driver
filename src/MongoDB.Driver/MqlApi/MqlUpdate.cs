using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace MongoDB.Driver.MqlApi
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class MqlUpdate<TDocument>
    {
        private readonly IBsonSerializer<TDocument> _documentSerializer;

        public MqlUpdate(IBsonSerializer<TDocument> documentSerializer)
        {
            _documentSerializer = documentSerializer;
        }

        public BsonDocument Translate()
        {
            throw new NotImplementedException();
        }

        public static implicit operator UpdateDefinition<TDocument>(MqlUpdate<TDocument> update)
        {
            var translation = update.Translate();
            return new BsonDocumentUpdateDefinition<TDocument>(translation);
        }
    }

    public static class MqlUpdateExtensions
    {
        public static MqlUpdate<TDocument> AddToSet<TDocument, TItem>(this MqlUpdate<TDocument> update, Expression<Func<TDocument, IEnumerable<TItem>>> field, TItem item)
        {
            throw new NotImplementedException();
        }

        public static MqlUpdate<TDocument> BitAnd<TDocument>(this MqlUpdate<TDocument> update, Expression<Func<TDocument, int>> field, int mask)
        {
            throw new NotImplementedException();
        }

        public static MqlUpdate<TDocument> BitOr<TDocument>(this MqlUpdate<TDocument> update, Expression<Func<TDocument, int>> field, int mask)
        {
            throw new NotImplementedException();
        }

        public static MqlUpdate<TDocument> BitXor<TDocument>(this MqlUpdate<TDocument> update, Expression<Func<TDocument, int>> field, int mask)
        {
            throw new NotImplementedException();
        }

        public static MqlUpdate<TDocument> CurrentDate<TDocument>(this MqlUpdate<TDocument> update, Expression<Func<TDocument, DateTime>> dateTimeField)
        {
            throw new NotImplementedException();
        }

        public static MqlUpdate<TDocument> Inc<TDocument, TValue>(this MqlUpdate<TDocument> update, Expression<Func<TDocument, TValue>> field, TValue amount)
        {
            throw new NotImplementedException();
        }

        public static MqlUpdate<TDocument> Max<TDocument, TValue>(this MqlUpdate<TDocument> update, Expression<Func<TDocument, TValue>> field, TValue value)
        {
            throw new NotImplementedException();
        }

        public static MqlUpdate<TDocument> Min<TDocument, TValue>(this MqlUpdate<TDocument> update, Expression<Func<TDocument, TValue>> field, TValue value)
        {
            throw new NotImplementedException();
        }

        public static MqlUpdate<TDocument> Mul<TDocument, TValue>(this MqlUpdate<TDocument> update, Expression<Func<TDocument, TValue>> field, TValue value)
        {
            throw new NotImplementedException();
        }

        public static MqlUpdate<TDocument> PopFirst<TDocument, TItem>(this MqlUpdate<TDocument> update, Expression<Func<TDocument, IEnumerable<TItem>>> field)
        {
            throw new NotImplementedException();
        }

        public static MqlUpdate<TDocument> PopLast<TDocument, TItem>(this MqlUpdate<TDocument> update, Expression<Func<TDocument, IEnumerable<TItem>>> field)
        {
            throw new NotImplementedException();
        }

        public static MqlUpdate<TDocument> Pull<TDocument, TItem>(this MqlUpdate<TDocument> update, Expression<Func<TDocument, IEnumerable<TItem>>> field, TItem item)
        {
            throw new NotImplementedException();
        }

        public static MqlUpdate<TDocument> Pull<TDocument, TItem>(this MqlUpdate<TDocument> update, Expression<Func<TDocument, IEnumerable<TItem>>> field, Expression<Func<TItem, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public static MqlUpdate<TDocument> PullAll<TDocument, TItem>(this MqlUpdate<TDocument> update, Expression<Func<TDocument, IEnumerable<TItem>>> field, params TItem[] items)
        {
            throw new NotImplementedException();
        }

        public static MqlUpdate<TDocument> Push<TDocument, TItem>(this MqlUpdate<TDocument> update, Expression<Func<TDocument, IEnumerable<TItem>>> field, TItem item)
        {
            throw new NotImplementedException();
        }

        public static MqlUpdate<TDocument> Rename<TDocument>(this MqlUpdate<TDocument> update, string oldName, string newName)
        {
            throw new NotImplementedException();
        }

        public static MqlUpdate<TDocument> Set<TDocument, TField>(this MqlUpdate<TDocument> update, Expression<Func<TDocument, TField>> field, TField value)
        {
            throw new NotImplementedException();
        }

        public static MqlUpdate<TDocument> SetOnInsert<TDocument, TField>(this MqlUpdate<TDocument> update, Expression<Func<TDocument, TField>> field, TField value)
        {
            throw new NotImplementedException();
        }

        public static MqlUpdate<TDocument> UnsetField<TDocument>(this MqlUpdate<TDocument> update, string fieldName)
        {
            throw new NotImplementedException();
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
