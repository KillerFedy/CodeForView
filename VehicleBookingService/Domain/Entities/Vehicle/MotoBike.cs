using Domain.Common.Result;
using Domain.Entities.Vehicle.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Vehicle
{
    public class MotoBike : Vehicle
    {
        public bool IsABS { get; protected set; }

        public MotoBike(long id, long vehicleModelId, int releaseYear, long priceCategoryId, int mileAge,
            int minimalRentDays, VehicleColor vehicleColor, bool isABS,
            string? description = null) : base(id, vehicleModelId,
                releaseYear, priceCategoryId, mileAge, minimalRentDays, vehicleColor, description)
        {
            IsABS = isABS;
        }
    }
}
