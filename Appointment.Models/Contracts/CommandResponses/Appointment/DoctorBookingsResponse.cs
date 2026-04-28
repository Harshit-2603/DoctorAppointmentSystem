using System.Collections.Generic;
using AppointmentModel = AppointmentSystem.Models.Models.Appointment;

namespace AppointmentSystem.Models.Contracts.CommandResponses.Appointment
{
    public class DoctorBookingsResponse
    {
        public int DoctorId { get; set; }
        public List<AppointmentModel> Bookings { get; set; } = new();
    }
}