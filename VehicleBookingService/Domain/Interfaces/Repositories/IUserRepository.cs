using Domain.Entities.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetUserAsync(long id, bool isVerified = true);
        Task<User?> GetUserByEmailAsync<T>(string email, bool isVerified = true) where T : User;
        Task<User> CreateUserAsync(User user);
        Task UpdateUserAsync(User user);
    }
}
