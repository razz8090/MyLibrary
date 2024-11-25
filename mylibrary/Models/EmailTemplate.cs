using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace mylibrary.Models;

[BsonIgnoreExtraElements]
public class EmailTemplate
{
	[BsonId, BsonRepresentation(BsonType.ObjectId), BsonElement("_id")]
	public string Key { get; set; }
	public string Template { get; set; }
}

