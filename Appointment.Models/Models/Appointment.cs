using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppointmentSystem.Models.Models
{
    public enum AppointmentStatus
    {
        Pending,
        Confirmed,
        Cancelled,
        Completed
    }

    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [ForeignKey("DoctorId")]
        public virtual Doctor Doctor { get; set; } = null!;

        public int? PatientId { get; set; }

        [ForeignKey("PatientId")]
        public virtual ApplicationUser? Patient { get; set; }

        [Required]
        public DateTime AppointmentDateTime { get; set; }

        [Required]
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
