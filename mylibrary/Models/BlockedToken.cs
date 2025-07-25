using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace mylibrary.Models;

public class BlockedToken
{
	[BsonId, BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
	public string ID { get; set; }
	public string Token { get; set; }
	public DateTime CreatedOn { get; set; }
}

