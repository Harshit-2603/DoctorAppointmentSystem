using MediatR;
using AppointmentModel = AppointmentSystem.Models.Models.Appointment;

namespace AppointmentSystem.Models.Contracts.CommandRequests.Appointment
{
    public record GetDoctorBookingsCommand(int DoctorId) : IRequest<List<AppointmentModel>>;
}