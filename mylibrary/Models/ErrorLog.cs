using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace mylibrary.Models;

public class ErrorLog
{
	[BsonId, BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
	public string ID { get; set; }
	public string ModuleName { get; set; }
	public int LineNumber { get; set; }
	public string Massage { get; set; }
	public string CreatedOn { get; set; }

}

