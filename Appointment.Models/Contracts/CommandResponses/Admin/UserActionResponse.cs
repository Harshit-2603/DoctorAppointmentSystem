namespace AppointmentSystem.Models.Contracts.CommandResponses.Admin
{
    public class UserActionResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? UserId { get; set; }
    }
}