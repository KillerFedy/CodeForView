using ProjectDatabase.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectDatabase.Models.User
{
    public class ClientInfoModel : BaseModel
    {
        public string Name { get; set; } = null!;
        public string Surname { get; set; } = null!;
        public DateTime? BirthDate { get; set; }
        public long UserId { get; set; }

        public UserModel? User { get; set; }
    }
}
