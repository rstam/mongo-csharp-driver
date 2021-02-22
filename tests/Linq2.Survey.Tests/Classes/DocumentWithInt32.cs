namespace Linq2.Survey.Tests.Classes
{
    public class DocumentWithInt32 : IHasId<int>
    {
        public int Id { get; set; }
        public int X { get; set; }
    }
}
