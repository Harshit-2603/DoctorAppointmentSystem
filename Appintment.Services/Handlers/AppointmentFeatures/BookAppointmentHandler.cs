using AppointmentSystem.Data.Data;
using AppointmentSystem.Models.Contracts.CommandRequests.Appointment;
using AppointmentSystem.Models.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AppointmentSystem.Services.Handlers.AppointmentFeatures
{
    /// <summary>
    /// Books one of the pre-generated Pending slots for the given patient.
    /// Enforces the three business rules:
    ///   1) one patient may only have one (non-cancelled) appointment per doctor;
    ///   2) one slot may only be booked by one patient (race-safe via concurrency token);
    ///   3) one patient may only have one (non-cancelled) appointment per calendar day.
    /// </summary>
    public class BookAppointmentHandler : IRequestHandler<BookAppointmentCommand, bool>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BookAppointmentHandler> _logger;

        public BookAppointmentHandler(ApplicationDbContext context, ILogger<BookAppointmentHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> Handle(BookAppointmentCommand request, CancellationToken cancellationToken)
        {
            // Re-load the slot fresh so we get the latest state.
            var slot = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == request.AppointmentId, cancellationToken);

            if (slot == null)
            {
                _logger.LogWarning("Booking failed: slot {SlotId} does not exist.", request.AppointmentId);
                return false;
            }

            // Rule 2: slot must still be free.
            if (slot.PatientId != null || slot.Status != AppointmentStatus.Pending)
            {
                _logger.LogWarning("Booking failed: slot {SlotId} is no longer available.", request.AppointmentId);
                return false;
            }

            // Rule 1: this patient cannot already hold an active appointment with this doctor.
            var alreadyWithDoctor = await _context.Appointments.AnyAsync(a =>
                    a.DoctorId == slot.DoctorId
                    && a.PatientId == request.PatientId
                    && a.Status != AppointmentStatus.Cancelled
                    && a.Status != AppointmentStatus.Completed,
                cancellationToken);

            if (alreadyWithDoctor)
            {
                _logger.LogWarning(
                    "Booking failed: patient {PatientId} already has an active appointment with doctor {DoctorId}.",
                    request.PatientId, slot.DoctorId);
                return false;
            }

            // Rule 3: this patient cannot already have any active appointment on the same calendar day.
            var dayStart = slot.AppointmentDateTime.Date;
            var dayEnd = dayStart.AddDays(1);

            var alreadyOnSameDay = await _context.Appointments.AnyAsync(a =>
                    a.PatientId == request.PatientId
                    && a.AppointmentDateTime >= dayStart
                    && a.AppointmentDateTime < dayEnd
                    && a.Status != AppointmentStatus.Cancelled
                    && a.Status != AppointmentStatus.Completed,
                cancellationToken);

            if (alreadyOnSameDay)
            {
                _logger.LogWarning(
                    "Booking failed: patient {PatientId} already has an active appointment on {Day:yyyy-MM-dd}.",
                    request.PatientId, dayStart);
                return false;
            }

            // Claim the slot.
            slot.PatientId = request.PatientId;
            slot.Status = AppointmentStatus.Confirmed;

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Another request grabbed the slot first.
                _logger.LogWarning(ex,
                    "Booking failed: concurrency conflict on slot {SlotId} for patient {PatientId}.",
                    request.AppointmentId, request.PatientId);
                return false;
            }
        }
    }
}
