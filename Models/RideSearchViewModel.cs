using System;

namespace RideShareFrontend.DTOs
{
    public class RideSearchResultDto
    {
        public string RideId { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        
        public DateTime DepartureTime { get; set; }
        
        public DateTime EstimatedArrivalTime { get; set; }
        
   
        public decimal PricePerSeat { get; set; }
        
        public int AvailableSeats { get; set; }
        
        public string VehicleInfo { get; set; } = string.Empty;
        
        public string DriverName { get; set; } = string.Empty;
        

        public decimal DriverRating { get; set; }
        
        public bool IsVerifiedDriver { get; set; }
        public bool HasAirConditioning { get; set; }
        public string[] Amenities { get; set; } = Array.Empty<string>();
    }
}