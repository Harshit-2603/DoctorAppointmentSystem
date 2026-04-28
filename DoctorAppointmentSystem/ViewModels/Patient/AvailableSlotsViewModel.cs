using System;
using System.Collections.Generic;

namespace DoctorAppointmentSystem.ViewModels.Patient
{
    public class AvailableSlotsViewModel
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public DateTime SelectedDate { get; set; }
        // Aligns with your GetAvailableSlotsCommand returning List<DateTime>
        public List<DateTime> AvailableTimes { get; set; } = new();
    }
}