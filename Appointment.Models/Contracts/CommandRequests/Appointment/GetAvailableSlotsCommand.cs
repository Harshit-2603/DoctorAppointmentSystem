using MediatR;
using AppointmentModel = AppointmentSystem.Models.Models.Appointment;

namespace AppointmentSystem.Models.Contracts.CommandRequests.Appointment
{
    public record GetAvailableSlotsCommand(int DoctorId, DateTime Date) : IRequest<List<AppointmentModel>>;
}
