using AppointmentSystem.Models.Models;
using Microsoft.AspNetCore.Identity;

namespace AppointmentSystem.Models.Models
{
    public enum UserRole
    {
        Admin,
        Doctor,
        Patient
    }

    public class ApplicationUser : IdentityUser<int>
    {
        public string FullName { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public virtual Doctor? DoctorProfile { get; set; }
    }
}