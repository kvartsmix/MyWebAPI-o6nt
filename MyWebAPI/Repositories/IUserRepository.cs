using System.Collections.Generic;
using System.Threading.Tasks;
using MyWebAPI.Models;

namespace MyWebAPI.Repositories;

public interface IUserRepository
{
    Task<List<User>> GetAsync();
    Task<User?> GetAsync(string id);
    Task<User?> GetByEmailAsync(string email);
    Task CreateAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(string id);
}