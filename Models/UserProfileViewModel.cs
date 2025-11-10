// Models/UserProfileViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace RideShareConnect.Models
{
    public class UserProfileViewModel
    {
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
    }
}