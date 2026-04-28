namespace AppointmentSystem.Models.Contracts.CommandResponses.Admin
{
    public class GenerateSlotsResponse
    {
        public bool IsSuccess { get; set; }
        public int SlotsCreated { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}