using MediatR;

namespace AppointmentSystem.Models.Contracts.CommandRequests.Admin
{
    // Returns the number of slots generated
    public record GenerateSlotsCommand(int AvailabilityId) : IRequest<bool>;
}