using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.User
{
    public class Client : User
    {
        public string Name { get; protected set; } = null!;
        public string Surname { get; protected set; } = null!;
        public DateTime? BirthDate { get; protected set; }

        public Client(long id, string email, string passwordHash, string phone,
            string name, string surname, DateTime? birthDate = null)
            : base(id, email, passwordHash, phone)
        {
            Name = name;
            Surname = surname;
            BirthDate = birthDate;
        }
    }
}
