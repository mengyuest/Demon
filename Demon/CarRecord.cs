using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
namespace Demon
{
    class CarRecord
    {
        public ObjectId id { get; set; }

        //[BsonElement("VEHICLE ID")]
        public int carId { get; set; }

        //[BsonElement("LATITUDE")]
        public double latitude { get; set; }

       // [BsonElement("LONGITUDE")]
        public double longitude { get; set; }

       // [BsonElement("LOCAL TIME")]
        public String currentTime { get; set; }

    }
}
