namespace Linq2.Survey.Tests.Classes
{
    public class DocumentWithNullableInt64 : IHasId<int>
    {
        public int Id { get; set; }
        public long? X { get; set; }
    }
}
