using ProjectDatabase.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectDatabase.Models.User
{
    public class UserModel : BaseModelWithStateDates
    {
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public bool IsVerified { get; set; }
        public long? ClientInfoId { get; set; }
        public long? RentalInfoInfoId { get; set; }

        public ClientInfoModel? ClientInfo { get; set; }
        public RentalInfoModel? RentalInfo { get; set; }
    }
}
