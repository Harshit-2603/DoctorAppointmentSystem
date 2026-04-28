using System.ComponentModel.DataAnnotations;
using AppointmentSystem.Models.Models;

namespace DoctorAppointmentSystem.ViewModels.Admin
{
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Full Name is required")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a user role")]
        public UserRole Role { get; set; }

    }
}