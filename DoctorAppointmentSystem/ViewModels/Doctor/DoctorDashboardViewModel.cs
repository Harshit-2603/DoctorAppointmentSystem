using System.Collections.Generic;
using AppointmentModel = AppointmentSystem.Models.Models.Appointment;

namespace DoctorAppointmentSystem.ViewModels.Doctor
{
    public class DoctorDashboardViewModel
    {
        public string DoctorName { get; set; } = string.Empty;
        // Using the alias to avoid namespace errors
        public List<AppointmentModel> UpcomingAppointments { get; set; } = new();
    }
}