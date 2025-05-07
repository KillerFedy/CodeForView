using Domain.Common.DomainErrors;
using Domain.Common.Result;
using Domain.Entities.Country;
using Domain.Entities.User;
using Domain.Entities.User.Verification;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DomainHandlers
{
    public class UserAuthorizeHandler(IUserRepository userRepository, 
        IUserVerificationTokenRepository userVerificationTokenRepository, 
        ICountryLocationRepository countryLocationRepository,
        IPasswordHasher passwordHasher)
    {
        private const string AllowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private const int TokenLength = 6;

        public async Task<Result<User>> RegistrateUser<T>(T userToRegister) where T : User
        {
            var user = await userRepository.GetUserByEmailAsync<T>(userToRegister.Email);
            if (user != null) return Result.Failure<User>(DomainErrors.User.AlreadyRegistered);
            if (user is Rental rental)
            {
                var countryLocation = await countryLocationRepository.GetCountryLocationAsync(rental.CompanyLocationId);
                if (countryLocation == null) return Result.Failure<User>(DomainErrors.CountryLocation.LocationNotFound);
            }
            user = await userRepository.CreateUserAsync(userToRegister);
            return user;
        }

        public async Task<Result<User>> AuthorizeUser<T>(string email, string inputPassword) where T : User
        {
            var user = await userRepository.GetUserByEmailAsync<T>(email);
            if(user == null) return Result.Failure<User>(DomainErrors.User.VerifyUserNotFound);
            if (!passwordHasher.Verify(inputPassword, user.PasswordHash))
            {
                return Result.Failure<User>(DomainErrors.User.NotValidPassword);
            }
            return user;
        }

        public async Task<Result<User>> VerificateUser(long userId, string verificationCode)
        {
            var user = await userRepository.GetUserAsync(userId, false);
            if (user == null) return Result.Failure<User>(DomainErrors.User.NotVerifyUserNotFound);
            var userVerificationToken = await userVerificationTokenRepository.GetUserVerificationTokenAsync(userId,
                verificationCode);
            if (userVerificationToken == null) 
            {
                return Result.Failure<User>(DomainErrors.UserVerificationToken.VerificationTokenNotFound);
            }
            else
            {
                if (DateTime.UtcNow > userVerificationToken.ExpireDateUtc)
                {
                    await userVerificationTokenRepository.DeleteTokenAsync(userVerificationToken.Id);
                    return Result.Failure<User>(DomainErrors.UserVerificationToken.VerificationTokenExpired);
                }
                await userVerificationTokenRepository.DeleteTokenAsync(userVerificationToken.Id);
                user.VerifyUser();
                await userRepository.UpdateUserAsync(user);
            }
            return user;
        }

        public async Task<UserVerificationToken> GenerateVerificationToken(long userId)
        {
            var verificationCode = GenerateRandomCode();
            var userVerificationToken = new UserVerificationToken(0, userId, verificationCode, DateTime.UtcNow 
                + TimeSpan.FromMinutes(2));
            userVerificationToken = await userVerificationTokenRepository.CreateUserVerificationTokenAsync(
                userVerificationToken);
            return userVerificationToken;
        }

        private string GenerateRandomCode()
        {
            var randomBytes = new byte[TokenLength];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            var characters = AllowedCharacters.ToCharArray();
            var code = new char[TokenLength];

            for (int i = 0; i < TokenLength; i++)
            {
                // Используем случайный байт для выбора символа из разрешенных
                int index = randomBytes[i] % characters.Length;
                code[i] = characters[index];
            }

            return new string(code);
        }
    }
}
