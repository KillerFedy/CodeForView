using Domain.Entities.Vehicle.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Vehicle
{
    public class Car : Vehicle
    {
        public VehicleTrasmission VehicleTrasmission { get; protected set; }
        public FuelType FuelType { get; protected set; }
        public CarType CarType { get; protected set; }

        public Car(long id, long vehicleModelId, int releaseYear, long priceCategoryId, int mileAge,
            int minimalRentDays, VehicleColor vehicleColor, VehicleTrasmission vehicleTrasmission,
            FuelType fuelType, CarType carType, string? description = null) : base(id, vehicleModelId,
                releaseYear, priceCategoryId, mileAge, minimalRentDays, vehicleColor, description)
        {
            VehicleTrasmission = vehicleTrasmission;
            FuelType = fuelType;
            CarType = carType;
        }
    }
}
