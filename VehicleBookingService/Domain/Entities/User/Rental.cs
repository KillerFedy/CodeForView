using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.User
{
    public class Rental : User
    {
        public string CompanyName { get; protected set; } = null!;
        public long CompanyLocationId { get; protected set; }
        public string ParkAddress { get; protected set; } = null!;

        public Rental(long id, string email, string passwordHash, string phone,
            string companyName, long locationId, string address) : base(id, email, passwordHash, phone)
        {
            CompanyName = companyName;
            CompanyLocationId = locationId;
            ParkAddress = address;
        }
    }
}
