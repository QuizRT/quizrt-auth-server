using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace quizrtAuthServer.Models
{
    public interface IAuth
    {
        Task CreateUserAsync(User user);
        Task DeleteUserByIdAsync(int id);
        Task<bool> UserExistsAsync(User user);
        Task<bool> EmailExistsAsync(string email);
        Task<List<User>> GetAllUsersAsync();
        Task<User> GetUserByEmailAsync(string email);
        Task<string> LoginAsync(string email, string password);
        string HashPassword(string password);
        Dictionary<string, string> GetUserDetailsFromToken(string Token);
    }
}