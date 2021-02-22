namespace Linq2.Survey.Tests.Classes
{
    public class DocumentWithDouble : IHasId<int>
    {
        public int Id { get; set; }
        public double X { get; set; }
    }
}
