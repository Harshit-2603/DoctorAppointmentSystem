using MediatR;

namespace AppointmentSystem.Models.Contracts.CommandRequests.Admin
{
    public record SetAvailabilityCommand(
        int DoctorId,
        DateTime StartTime,
        DateTime EndTime,
        int SlotSizeInMinutes
    ) : IRequest<bool>;
}