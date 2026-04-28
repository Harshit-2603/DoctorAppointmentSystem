using System.ComponentModel.DataAnnotations;
using AppointmentSystem.Models.Models;

namespace DoctorAppointmentSystem.ViewModels.Admin
{
    public class UpdateUserViewModel
    {
        [Required]
        public int Id { get; set; } // Matches the 'Id' in your UpdateUserCommand

        [Required, Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }
    }
}