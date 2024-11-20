using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace mylibrary.Models.CommonModel;

[BsonIgnoreExtraElements]
public class ModelExtension
{
	[BsonRepresentation(BsonType.ObjectId)]
	public string CreatedBy { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string UpdatedBy { get; set; }

	public DateTime CreatedOn { get; set; }

	public Nullable<DateTime> UpdatedOn { get; set; }

	public Status Status { get; set; }
}

public enum Status
{
	Pending,
	Active,
	DeActive,
	Delete
}

