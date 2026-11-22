using System.Collections.Generic;
using System.Threading.Tasks;
using MyWebAPI.Models;
using MyWebAPI.Repositories;

namespace MyWebAPI.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public Task<List<User>> GetAsync() => _userRepository.GetAsync();
    public Task<User?> GetAsync(string id) => _userRepository.GetAsync(id);
    public Task<User?> GetByEmailAsync(string email) => _userRepository.GetByEmailAsync(email);
    public Task CreateAsync(User user) => _userRepository.CreateAsync(user);
    public Task UpdateAsync(User user) => _userRepository.UpdateAsync(user);
    public Task DeleteAsync(string id) => _userRepository.DeleteAsync(id);
}