namespace Linq2.Survey.Tests.Classes
{
    public class DocumentWithSingle : IHasId<int>
    {
        public int Id { get; set; }
        public float X { get; set; }
    }
}
