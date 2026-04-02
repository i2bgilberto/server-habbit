using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PrimeDiscipline.Domain.Entities;

public sealed class Session
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>Opaque UUID token sent by the client on every request.</summary>
    [BsonElement("token")]
    public string Token { get; set; } = string.Empty;

    [BsonElement("createdAtUtc")]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [BsonElement("expiresAtUtc")]
    public DateTime ExpiresAtUtc { get; set; }

    [BsonElement("isRevoked")]
    public bool IsRevoked { get; set; } = false;
}
