using Domain.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Country
{
    public class CountryLocation : BaseEntity
    {
        public string LocationName { get; protected set; }
        public string LocationTimeZone { get; protected set; }

        public long CountryId { get; protected set; }

        public CountryLocation(long id, string locationName, string locationTimeZone, long countryId) 
            : base(id)
        {
            LocationName = locationName;
            LocationTimeZone = locationTimeZone;
            CountryId = countryId;
        }
    }
}
