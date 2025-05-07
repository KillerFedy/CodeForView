using Domain.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Country
{
    public class Country : BaseEntity
    {
        public string CountryName { get; protected set; }

        public Country(long id, string countryName) : base(id)
        {
            CountryName = countryName;
        }
    }
}
