using MediatR;
using AppointmentSystem.Models.Models;
using AppointmentModel = AppointmentSystem.Models.Models.Appointment;

namespace AppointmentSystem.Models.Contracts.CommandRequests.Appointment
{
    public record GetDoctorBookingsCommand(int DoctorId) : IRequest<List<AppointmentModel>>;

    // Doctor changes status of one of their own appointments (Completed / Confirmed / etc.).
    // DoctorId is included so the handler can verify ownership.
    public record UpdateAppointmentStatusCommand(
        int AppointmentId,
        int DoctorId,
        AppointmentStatus NewStatus
    ) : IRequest<bool>;

    // Admin cancels an appointment. Slot becomes available again.
    public record CancelAppointmentCommand(int AppointmentId) : IRequest<bool>;

    // Resolves the Doctor entity Id from the logged-in user's ApplicationUser Id.
    public record GetDoctorByUserIdQuery(int UserId) : IRequest<int?>;

    // Lists every appointment in the system (for admin overview).
    public record GetAllAppointmentsQuery() : IRequest<List<AppointmentModel>>;

    // Patient portal — list every booking owned by the logged-in patient.
    public record GetPatientAppointmentsQuery(int PatientId) : IRequest<List<AppointmentModel>>;

    // Patient cancels one of their own bookings. PatientId is checked for ownership;
    // the slot row is freed (PatientId = null, Status = Pending) so it can be re-booked.
    public record PatientCancelAppointmentCommand(int AppointmentId, int PatientId) : IRequest<bool>;
}
