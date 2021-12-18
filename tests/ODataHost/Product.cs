using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace ODataHost
{
    public class Product
    {
        public int Id { get; set; }
        [BsonElement("name")]
        public string Name { get; set; }
    }
}
