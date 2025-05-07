using Domain.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.User.Verification
{
    public class UserVerificationToken : BaseEntity
    {
        public long UserId { get; protected set; }
        public DateTime ExpireDateUtc { get; protected set; }
        public string VerificationCode { get; protected set; }

        public UserVerificationToken(long id, long userId, string verificationCode,
            DateTime expireDateUtc) : base(id)
        {
            UserId = userId;
            ExpireDateUtc = expireDateUtc;
            VerificationCode = verificationCode;
        }
    }
}
