using ProjectDatabase.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectDatabase.Models.Country
{
    public class CountryModel : BaseModel
    {
        public string CountryName { get; set; } = null!;
    }
}
