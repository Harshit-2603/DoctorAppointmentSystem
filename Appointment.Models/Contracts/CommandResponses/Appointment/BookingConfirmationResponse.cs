using System;

namespace AppointmentSystem.Models.Contracts.CommandResponses.Appointment
{
    public class BookingConfirmationResponse
    {
        public bool IsSuccess { get; set; }
        public int AppointmentId { get; set; }
        public DateTime ScheduledTime { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}