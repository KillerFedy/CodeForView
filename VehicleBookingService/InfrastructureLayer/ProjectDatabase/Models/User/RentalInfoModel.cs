using ProjectDatabase.Common;
using ProjectDatabase.Models.Country;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectDatabase.Models.User
{
    public class RentalInfoModel : BaseModel
    {
        public string CompanyName { get; set; } = null!;
        public string ParkAddress { get; set; } = null!;

        public long UserId { get; set; }
        public long CompanyLocationId { get; set; }

        public UserModel? User { get; set; }
        public CountryLocationModel? CompanyLocation { get; set; }
    }
}
