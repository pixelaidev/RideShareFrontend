using System;
using System.ComponentModel.DataAnnotations;

namespace RideShareFrontend.DTOs
{
    public class RideBookingCreateViewModel
    {
        [Required]
        public int RideId { get; set; }

        [Required]
        public int SeatsBooked { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        public string BookingStatus { get; set; } = "Pending";

        public DateTime BookingTime { get; set; } = DateTime.Now;

        public DateTime? CancelledAt { get; set; }

        public string? CancellationReason { get; set; }

        public string PickupPoint { get; set; }

        public string DropPoint { get; set; }

        public string PassengerNotes { get; set; }
    }
}
