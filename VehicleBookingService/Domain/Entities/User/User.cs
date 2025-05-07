using Domain.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.User
{
    public abstract class User : BaseEntity
    {
        public string Email { get; protected set; } = null!;
        public string PasswordHash { get; protected set; } = null!;
        public string Phone { get; protected set; } = null!;
        public bool IsVerified { get; protected set; }

        public User(long id, string email, string passwordHash, string phone) : base(id)
        {
            Email = email;
            PasswordHash = passwordHash;
            Phone = phone;
        }

        public void VerifyUser() => IsVerified = true;
    }
}
