using MongoDB.Bson;

namespace Linq2.Survey.Tests.Classes
{
    public class DocumentWithTwoBsonDocuments : IHasId<int>
    {
        public int Id { get; set; }
        public BsonDocument X { get; set; }
        public BsonDocument Y { get; set; }
    }
}
