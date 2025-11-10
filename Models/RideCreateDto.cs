using System;
using System.ComponentModel.DataAnnotations;

namespace RideShareFrontend.DTOs
{
    public class RideCreateDto
    {
        [Required(ErrorMessage = "Origin is required")]
        public string Origin { get; set; } = string.Empty;

        [Required(ErrorMessage = "Destination is required")]
        public string Destination { get; set; } = string.Empty;

        [Required(ErrorMessage = "Departure time is required")]
        public DateTime DepartureTime { get; set; } = DateTime.Now.AddHours(1);

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 10000, ErrorMessage = "Price must be between 0.01 and 10,000")]
        public decimal PricePerSeat { get; set; }

        [Required(ErrorMessage = "Available seats is required")]
        [Range(1, 10, ErrorMessage = "Seats must be between 1 and 10")]
        public int AvailableSeats { get; set; }

        public string? RoutePoints { get; set; }
    }


    
    

}