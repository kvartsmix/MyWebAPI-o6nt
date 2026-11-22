using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MyWebAPI.Models;

namespace MyWebAPI.Documents;

public class UserDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("username")]
    public string Username { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("passwordHash")]
    public string Password { get; set; } = string.Empty;

    [BsonElement("role")]
    public Roles Role { get; set; } = Roles.User;

    [BsonElement("refreshToken")]
    public string? RefreshToken { get; set; }

    [BsonElement("refreshTokenExpiryTime")]
    public DateTime? RefreshTokenExpiryTime { get; set; }
}