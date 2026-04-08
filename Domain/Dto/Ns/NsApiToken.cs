using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MyDotnet.Domain.Dto.Ns
{
    public class NsApiToken
    {
        [BsonId]
        public ObjectId id { get; set; }
        public string name { get; set; }
        public List<string> roles {get; set; }
        public string notes { get; set; }
        public string created_at { get; set; }
    }
}
