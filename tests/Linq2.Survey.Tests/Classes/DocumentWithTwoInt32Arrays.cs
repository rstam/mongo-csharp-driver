namespace Linq2.Survey.Tests.Classes
{
    public class DocumentWithTwoInt32Arrays : IHasId<int>
    {
        public int Id { get; set; }
        public int[] X { get; set; }
        public int[] Y { get; set; }
    }
}
