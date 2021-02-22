namespace Linq2.Survey.Tests.Classes
{
    public interface IHasId<TId>
    {
        TId Id { get; set; }
    }
}
