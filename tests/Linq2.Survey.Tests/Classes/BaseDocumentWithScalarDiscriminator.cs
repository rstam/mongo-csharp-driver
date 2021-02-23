using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Conventions;

namespace Linq2.Survey.Tests.Classes
{
    [BsonDiscriminator(Required = true)]
    [BsonKnownTypes(typeof(DerivedDocumentWithScalarDiscriminator))]
    public class BaseDocumentWithScalarDiscriminator : IHasId<int>
    {
        static BaseDocumentWithScalarDiscriminator()
        {
            BsonSerializer.RegisterDiscriminatorConvention(typeof(BaseDocumentWithScalarDiscriminator), new ScalarDiscriminatorConvention("_t"));
        }

        public int Id { get; set; }
        public int X { get; set; }
    }
}
