using System;

namespace DoctorAppointmentSystem.ViewModels.Patient
{
    public class BookAppointmentViewModel
    {
        public int AppointmentId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public DateTime AppointmentTime { get; set; }
    }
}