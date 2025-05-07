using Domain.Entities.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces.Security
{
    public interface ITokenProvider
    {
        /// <summary>
        /// Создание токена авторизации
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        string CreateAuthToken(User user);
    }
}
