using Domain.Entities.Country;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces.Repositories
{
    public interface ICountryRepository
    {
        Task<List<Country>> GetCountriesAsync();
    }
}
