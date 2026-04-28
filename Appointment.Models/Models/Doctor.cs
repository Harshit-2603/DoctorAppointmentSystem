using AppointmentSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppointmentSystem.Models.Models
{
    public class Doctor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Specialization { get; set; } = string.Empty;

        public string Bio { get; set; } = string.Empty;

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        public virtual ICollection<Availability> Availabilities { get; set; } = new List<Availability>();
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}