using MediatR;

namespace AppointmentSystem.Models.Contracts.CommandRequests.Appointment
{
    public record GetAvailableSlotsCommand(int DoctorId, DateTime Date) : IRequest<List<DateTime>>;
}