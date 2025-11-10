using System;
using System.ComponentModel.DataAnnotations;

namespace RideShareFrontend.DTOs
{
    public class RideBookingDto
    {
        public int BookingId { get; set; }
        public int RideId { get; set; }
        public int PassengerId { get; set; }
        public int SeatsBooked { get; set; }
        public decimal TotalAmount { get; set; }
        public string BookingStatus { get; set; }
        public DateTime BookingTime { get; set; }
        public string PickupPoint { get; set; }
        public string DropPoint { get; set; }
    }
}