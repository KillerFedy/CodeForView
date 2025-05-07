using ProjectDatabase.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectDatabase.Models.User
{
    public class UserVerificationTokenModel : BaseModelWithStateDates
    {
        public long UserId { get; set; }
        public DateTime ExpireDateUtc { get; set; }
        public string VerificationCode { get; set; } = null!;

        public UserModel? User { get; set; }
    }
}
