using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using MyWebAPI.Documents;
using MyWebAPI.Models;

namespace MyWebAPI.Repositories;

public class UserRepository : IUserRepository
{
	private readonly IMongoCollection<UserDocument> _collection;

	public UserRepository(IMongoClient client)
	{
		var database = client.GetDatabase("mydatabase");
		_collection = database.GetCollection<UserDocument>("Users");
	}

	public async Task<List<User>> GetAsync()
	{
		var documents = await _collection.Find(_ => true).ToListAsync();
		return documents.Select(ToModel).ToList();
	}

	public async Task<User?> GetAsync(string id)
	{
		var document = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
		return document is null ? null : ToModel(document);
	}

	public async Task<User?> GetByEmailAsync(string email)
	{
		var document = await _collection.Find(x => x.Email == email).FirstOrDefaultAsync();
		return document is null ? null : ToModel(document);
	}

	public async Task CreateAsync(User user) =>
		await _collection.InsertOneAsync(ToDocument(user));

	public async Task UpdateAsync(User user) =>
		await _collection.ReplaceOneAsync(x => x.Id == user.Id, ToDocument(user));

	public async Task DeleteAsync(string id) =>
		await _collection.DeleteOneAsync(x => x.Id == id);

	private static User ToModel(UserDocument document) => new()
	{
		Id = document.Id,
		Username = document.Username,
		Email = document.Email,
		Password = document.Password,
		RefreshToken = document.RefreshToken,
		RefreshTokenExpiryTime = document.RefreshTokenExpiryTime,
		Role = document.Role
	};

	private static UserDocument ToDocument(User user) => new()
	{
		Id = user.Id,
		Username = user.Username,
		Email = user.Email,
		Password = user.Password,
		RefreshToken = user.RefreshToken,
		RefreshTokenExpiryTime = user.RefreshTokenExpiryTime,
		Role = user.Role
	};
}