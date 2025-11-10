using System;
using System.ComponentModel.DataAnnotations;

namespace RideShareFrontend.DTOs
{
    public class RideDto
    {
        public int RideId { get; set; }
        public int DriverId { get; set; }
        public int VehicleId { get; set; }
        public string DriverName { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public int AvailableSeats { get; set; }
        public decimal PricePerSeat { get; set; }
        public string RideType { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public bool IsRecurring { get; set; }
    }
}