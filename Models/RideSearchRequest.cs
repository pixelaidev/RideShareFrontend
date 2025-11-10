using System;
using System.ComponentModel.DataAnnotations;

namespace RideShareFrontend.DTOs
{
    public class RideSearchRequestDto
    {
        [Required(ErrorMessage = "Origin is required")]
        [StringLength(100, ErrorMessage = "Origin cannot exceed 100 characters")]
        public string Origin { get; set; } = string.Empty;

        [Required(ErrorMessage = "Destination is required")]
        [StringLength(100, ErrorMessage = "Destination cannot exceed 100 characters")]
        public string Destination { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date is required")]
        [DataType(DataType.Date)]
        public DateTime RideDate { get; set; } = DateTime.Today;

        [Range(1, 10, ErrorMessage = "Seats must be between 1 and 10")]
        public int Passengers { get; set; } = 1;

        public bool FlexibleDates { get; set; } = false;
    }

   
}