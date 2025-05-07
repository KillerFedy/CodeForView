using Domain.Entities.User;
using ProjectDatabase.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectDatabase.Converters
{
    public static class UserToDomainConverter
    {
        public static User ToDomain(this UserModel userModel)
        {
            User user;
            if (userModel.ClientInfo != null)
            {
                user = new Client(userModel.Id, userModel.Email, userModel.PasswordHash, userModel.Phone,
                    userModel.ClientInfo.Name, userModel.ClientInfo.Surname, userModel.ClientInfo.BirthDate);
                return user;
            }
            user = new Rental(userModel.Id, userModel.Email, userModel.PasswordHash, userModel.Phone,
                userModel.RentalInfo.CompanyName, userModel.RentalInfo.CompanyLocationId,
                userModel.RentalInfo.ParkAddress);
            return user;
        }
    }
}
