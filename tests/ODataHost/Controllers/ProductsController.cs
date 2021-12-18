using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace ODataHost.Controllers
{
    public class ProductsController : ODataController
    {
        private MongoClient _client;
        private IMongoDatabase _db;
        private IMongoCollection<Product> _coll;

        public ProductsController()
        {
            var clientSettings = MongoClientSettings.FromConnectionString("mongodb://localhost");
            clientSettings.LinqProvider = LinqProvider.V3;
            _client = new MongoClient(clientSettings);
            _db = _client.GetDatabase("test");
            _coll = _db.GetCollection<Product>("products");
        }

        [EnableQuery]
        public IQueryable<Product> Get()
        {
            return _coll.AsQueryable();
        }
    }
}
