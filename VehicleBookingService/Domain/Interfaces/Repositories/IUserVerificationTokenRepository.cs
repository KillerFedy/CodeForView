using Domain.Entities.User.Verification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces.Repositories
{
    public interface IUserVerificationTokenRepository
    {
        Task<UserVerificationToken> GetUserVerificationTokenAsync(long userId, string verificationCode);
        Task<UserVerificationToken> CreateUserVerificationTokenAsync(UserVerificationToken userVerificationToken);
        Task DeleteTokenAsync(long tokenId);
    }
}
