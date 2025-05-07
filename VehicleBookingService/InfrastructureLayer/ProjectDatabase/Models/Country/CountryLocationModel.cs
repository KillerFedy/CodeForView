using ProjectDatabase.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectDatabase.Models.Country
{
    public class CountryLocationModel : BaseModel
    {
        public string LocationName { get; set; } = null!;
        public string LocationTimeZone { get; set; } = null!;

        public long CountryId { get; set; }
        public CountryModel? Country { get; set; }
    }
}
