using MongoDB.Bson;

namespace Linq2.Survey.Tests.Classes
{
    public class DocumentWithTwoStrings : IHasId<int>
    {
        public int Id { get; set; }
        public string X { get; set; }
        public string Y { get; set; }
    }
}
