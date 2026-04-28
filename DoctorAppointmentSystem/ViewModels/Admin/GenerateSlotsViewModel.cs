namespace DoctorAppointmentSystem.ViewModels.Admin
{
    public class GenerateSlotsViewModel
    {
        public int AvailabilityId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string DayOfWeek { get; set; } = string.Empty;
    }
}