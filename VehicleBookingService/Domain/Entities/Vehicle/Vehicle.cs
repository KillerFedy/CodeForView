using Domain.Common.Entities;
using Domain.Entities.Vehicle.Enums;


namespace Domain.Entities.Vehicle
{
    public abstract class Vehicle : BaseEntity
    {
        public long VehicleModelId { get; protected set; }
        public int ReleaseYear { get; protected set; }
        public long PriceCategoryId { get; protected set; }
        public int MileAge { get; protected set; }
        public int MinimalRentDays { get; protected set; }
        public VehicleColor VehicleColor { get; protected set; }
        public VehicleStatus VehicleStatus { get; protected set; }
        public string? Description { get; protected set; }

        public Vehicle(long id, long vehicleModelId, int releaseYear, long priceCategoryId, 
            int mileAge, int minimalRentDays, 
            VehicleColor vehicleColor, string? description = null) : base(id)
        {
            VehicleModelId = vehicleModelId;
            ReleaseYear = releaseYear;
            PriceCategoryId = priceCategoryId;
            MinimalRentDays = minimalRentDays;
            MileAge = mileAge;
            VehicleColor = vehicleColor;
            Description = description;
            VehicleStatus = VehicleStatus.Free;
        }
    }
}
