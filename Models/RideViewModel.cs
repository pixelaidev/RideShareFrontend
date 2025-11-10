
using System;

namespace RideShareFrontend.DTOs
{
    public class RideViewModel
    {
        public int RideId { get; set; }
        public string DriverName { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public DateTime DepartureTime { get; set; }
        public int AvailableSeats { get; set; }
        public decimal PricePerSeat { get; set; }
    }

}