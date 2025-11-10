using System;
using System.ComponentModel.DataAnnotations;

namespace RideShareFrontend.Models.DTOs
{
    public class VehicleRegistrationViewModel
    {
        [Required(ErrorMessage = "Vehicle type is required")]
        public string VehicleType { get; set; }

      

        [Required(ErrorMessage = "License plate is required")]
        [StringLength(15, ErrorMessage = "License plate cannot exceed 15 characters")]
        [RegularExpression(@"^[A-Z0-9-]+$", ErrorMessage = "License plate can only contain uppercase letters, numbers, and hyphens")]
        public string LicensePlate { get; set; }

        [Required(ErrorMessage = "Insurance number is required")]
        [StringLength(50, ErrorMessage = "Insurance number cannot exceed 50 characters")]
        public string InsuranceNumber { get; set; }

      [Required(ErrorMessage = "Registration expiry date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Registration Expiry")]
        // [FutureDateExcludingToday(ErrorMessage = "Registration expiry must be after today")]
        public DateTime RegistrationExpiry { get; set; }

        [Required(ErrorMessage = "RC document is required")]
        public string RCDocumentBase64 { get; set; }

        [Required(ErrorMessage = "Insurance document is required")]
        public string InsuranceDocumentBase64 { get; set; }
    }
}