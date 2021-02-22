using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Linq2.Survey.Tests.Classes
{
    public class DocumentWithNullableDecimal : IHasId<int>
    {
        public int Id { get; set; }
        [BsonRepresentation(BsonType.Decimal128)] public decimal? X { get; set; }
    }
}
