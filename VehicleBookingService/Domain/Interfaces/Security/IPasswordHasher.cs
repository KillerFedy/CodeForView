using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces.Security
{
    public interface IPasswordHasher
    {
        /// <summary>
        /// Преобразование пароля в хэш
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public string Hash(string password);
        /// <summary>
        /// Сравнение пароля с хэшем пароля
        /// </summary>
        /// <param name="password"></param>
        /// <param name="passwordHash"></param>
        /// <returns></returns>
        public bool Verify(string password, string passwordHash);
    }
}
