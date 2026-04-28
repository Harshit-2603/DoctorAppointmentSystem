namespace AppointmentSystem.Models.Contracts.CommandResponses.Admin
{
    public class SetAvailabilityResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}