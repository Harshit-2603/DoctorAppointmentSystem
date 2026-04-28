using System.ComponentModel.DataAnnotations;

namespace DoctorAppointmentSystem.ViewModels.Admin
{
    public class SetAvailabilityViewModel
    {
        [Required]
        public int DoctorId { get; set; }

        [Required, DataType(DataType.Time), Display(Name = "Start Time")]
        public DateTime StartTime { get; set; }

        [Required, DataType(DataType.Time), Display(Name = "End Time")]
        public DateTime EndTime { get; set; }

        [Required, Range(15, 120), Display(Name = "Slot Duration (Minutes)")]
        public int SlotSizeInMinutes { get; set; } = 30;
    }
}