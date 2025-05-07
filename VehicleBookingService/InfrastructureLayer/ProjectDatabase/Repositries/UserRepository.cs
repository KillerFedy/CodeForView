using Domain.Entities.User;
using Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using ProjectDatabase.Common;
using ProjectDatabase.Converters;
using ProjectDatabase.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ProjectDatabase.Repositries
{
    public class UserRepository(Context context) : BaseRepositoryWithStateDates<UserModel>(context), 
        IUserRepository
    {

        public async Task<User> CreateUserAsync(User user)
        {
            UserModel userModel = new UserModel()
            { 
                Email = user.Email,
                PasswordHash = user.PasswordHash,
                Phone = user.Phone,
            };  
            switch(user)
            {
                case Client client:
                    var clientInfoModel = new ClientInfoModel()
                    { 
                        Name = client.Name,
                        Surname = client.Surname,
                        BirthDate = client.BirthDate,
                    };
                    userModel.ClientInfo = clientInfoModel;
                    break;
                case Rental rental:
                    var rentalInfoModel = new RentalInfoModel()
                    {
                        CompanyName = rental.CompanyName,
                        ParkAddress = rental.ParkAddress,
                        CompanyLocationId = rental.CompanyLocationId,
                    };
                    userModel.RentalInfo = rentalInfoModel;
                    break;
            }
            userModel = await InsertAsync(userModel);
            return userModel.ToDomain();
        }

        public async Task<User?> GetUserAsync(long id, bool isVerified = true)
        {
            var user = await GetItemByQueryAsync(x => x.Id == id && !x.IsDeleted && x.IsVerified == isVerified,
                include: x => x.Include(x => x.ClientInfo).Include(x => x.RentalInfo), asNoTracking: true);
            return user?.ToDomain();
        }

        public async Task<User?> GetUserByEmailAsync<T>(string email, bool isVerified = true) where T : User
        {
            Expression<Func<UserModel, bool>> userExpression = x => x.Email == email;
            Expression<Func<UserModel, bool>>? userTypeExpression = null;
            Func<IQueryable<UserModel>, IIncludableQueryable<UserModel, object?>>? include = null;
            if (typeof(T) == typeof(Client))
            {
                userTypeExpression = x => x.ClientInfoId != null;
                include = query => query.Include(x => x.ClientInfo);
            }
            else if (typeof(T) == typeof(Rental))
            {
                userTypeExpression = x => x.RentalInfoInfoId != null;
                include = query => query.Include(x => x.RentalInfo);
            }
            Expression.And(userExpression, userTypeExpression);
            var user = await GetItemByQueryAsync(userTypeExpression, include: include, asNoTracking: true);
            return user?.ToDomain();
        }

        public async Task UpdateUserAsync(User user)
        {
            var userModel = await GetItemByQueryAsync(x => x.Id == user.Id, include: x => x.Include(
                x => x.ClientInfo).Include(x => x.RentalInfo));
            userModel.Email = user.Email;
            userModel.PasswordHash = user.PasswordHash;
            userModel.Phone = user.Phone;
            userModel.IsVerified = user.IsVerified;

            switch (user)
            {
                case Client client:
                    userModel.ClientInfo.Name = client.Name;
                    userModel.ClientInfo.Surname = client.Surname;
                    userModel.ClientInfo.BirthDate = client.BirthDate;
                    break;
                case Rental rental:
                    userModel.RentalInfo.CompanyName = rental.CompanyName;
                    userModel.RentalInfo.ParkAddress = rental.ParkAddress;
                    userModel.RentalInfo.CompanyLocationId = rental.CompanyLocationId;
                    break;
            }
            userModel = await UpdateAsync(userModel);
        }
    }
}
