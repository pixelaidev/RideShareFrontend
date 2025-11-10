// Models/DriverProfileDto.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace RideShareFrontend.Models.DTOs
{
    public class DriverProfileDto
    {
        // ----- Personal Information -----
        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [StringLength(255, ErrorMessage = "Address cannot exceed 255 characters")]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Display(Name = "Profile Picture")]
        public string ProfilePicture { get; set; }

        public bool IsNewProfile { get; set; } = false;

        // ----- Driver-Specific Information -----
        [Required(ErrorMessage = "Driver's license number is required")]
        [StringLength(50, ErrorMessage = "License number cannot exceed 50 characters")]
        [Display(Name = "Driver's License Number")]
        public string LicenseNumber { get; set; }

       [Required(ErrorMessage = "License expiry date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "License Expiry Date")]
        public DateTime LicenseExpiryDate { get; set; }

        [Display(Name = "License Image")]
        public string LicenseImageUrl { get; set; }

        [Required(ErrorMessage = "Years of driving experience is required")]
        [Range(0, 50, ErrorMessage = "Years of experience must be between 0 and 50")]
        [Display(Name = "Years of Driving Experience")]
        public int YearsOfExperience { get; set; }

        // ----- Emergency Contact -----
        [Required(ErrorMessage = "Emergency contact name is required")]
        [StringLength(100, ErrorMessage = "Emergency contact name cannot exceed 100 characters")]
        [Display(Name = "Emergency Contact Name")]
        public string EmergencyContactName { get; set; }

        [Required(ErrorMessage = "Emergency contact phone is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [Display(Name = "Emergency Contact Phone")]
        public string EmergencyContactPhone { get; set; }


        public bool isverfied { get; set; } // Indicates if the driver is verified

        public bool IsVerified { get; set; }

    }
}
