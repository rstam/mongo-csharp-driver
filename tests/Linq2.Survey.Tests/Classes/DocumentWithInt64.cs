namespace Linq2.Survey.Tests.Classes
{
    public class DocumentWithInt64 : IHasId<int>
    {
        public int Id { get; set; }
        public long X { get; set; }
    }
}
