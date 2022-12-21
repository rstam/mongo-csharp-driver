namespace MongoDB.Driver.Tests.MqlApi
{
    public abstract class MqlIntegrationTest
    {
        public void CreateCollection<TDocument>(
            IMongoCollection<TDocument> collection,
            params TDocument[] documents)
        {
            collection.Database.DropCollection(collection.CollectionNamespace.CollectionName);
            collection.InsertMany(documents);
        }

        public IMongoCollection<TDocument> GetCollection<TDocument>()
        {
            var client = DriverTestConfiguration.Client;
            var database = client.GetDatabase(DriverTestConfiguration.DatabaseNamespace.DatabaseName);
            return database.GetCollection<TDocument>(DriverTestConfiguration.CollectionNamespace.CollectionName);
        }
    }
}
