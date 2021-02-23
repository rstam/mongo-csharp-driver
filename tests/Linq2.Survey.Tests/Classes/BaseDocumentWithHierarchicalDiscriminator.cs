using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Conventions;

namespace Linq2.Survey.Tests.Classes
{
    [BsonDiscriminator(Required = true, RootClass = true)]
    [BsonKnownTypes(typeof(DerivedDocumentWithHierarchicalDiscriminator))]
    public class BaseDocumentWithHierarchicalDiscriminator : IHasId<int>
    {
        static BaseDocumentWithHierarchicalDiscriminator()
        {
            BsonSerializer.RegisterDiscriminatorConvention(typeof(BaseDocumentWithHierarchicalDiscriminator), new HierarchicalDiscriminatorConvention("_t"));
        }

        public int Id { get; set; }
        public int X { get; set; }
    }
}
