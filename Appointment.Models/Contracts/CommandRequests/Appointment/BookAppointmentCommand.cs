using MediatR;

namespace AppointmentSystem.Models.Contracts.CommandRequests.Appointment
{
    // Simplified to only require the Slot ID and the Patient ID
    public record BookAppointmentCommand(
        int AppointmentId,
        int PatientId
    ) : IRequest<bool>;
}