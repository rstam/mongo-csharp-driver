using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Linq2.Survey.Tests.Classes
{
    public class DocumentWithDecimal : IHasId<int>
    {
        public int Id { get; set; }
        [BsonRepresentation(BsonType.Decimal128)] public decimal X { get; set; }
    }
}
