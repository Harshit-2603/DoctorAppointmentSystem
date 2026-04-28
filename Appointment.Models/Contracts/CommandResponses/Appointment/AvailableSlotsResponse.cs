using System;
using System.Collections.Generic;

namespace AppointmentSystem.Models.Contracts.CommandResponses.Appointment
{
    public class AvailableSlotsResponse
    {
        public int DoctorId { get; set; }
        public List<DateTime> AvailableTimes { get; set; } = new();
    }
}