namespace Linq2.Survey.Tests.Classes
{
    public class DocumentWithNullableInt32 : IHasId<int>
    {
        public int Id { get; set; }
        public int? X { get; set; }
    }
}
