using AppointmentSystem.Data.Data;
using AppointmentSystem.Models.Contracts.CommandRequests.Appointment;
using AppointmentSystem.Models.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AppointmentModel = AppointmentSystem.Models.Models.Appointment;

namespace AppointmentSystem.Services.Handlers.AppointmentFeatures
{
    /// <summary>
    /// Returns every booking that has actually been claimed by a patient for a given doctor.
    /// Pre-generated empty slots (PatientId == null) are excluded.
    /// </summary>
    public class GetDoctorBookingsHandler : IRequestHandler<GetDoctorBookingsCommand, List<AppointmentModel>>
    {
        private readonly ApplicationDbContext _context;

        public GetDoctorBookingsHandler(ApplicationDbContext context) => _context = context;

        public async Task<List<AppointmentModel>> Handle(GetDoctorBookingsCommand request, CancellationToken cancellationToken)
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == request.DoctorId && a.PatientId != null)
                .OrderBy(a => a.AppointmentDateTime)
                .ToListAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Doctor-only: change the status of one of their own bookings (mark Completed, etc.).
    /// </summary>
    public class UpdateAppointmentStatusHandler : IRequestHandler<UpdateAppointmentStatusCommand, bool>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UpdateAppointmentStatusHandler> _logger;

        public UpdateAppointmentStatusHandler(ApplicationDbContext context, ILogger<UpdateAppointmentStatusHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> Handle(UpdateAppointmentStatusCommand request, CancellationToken cancellationToken)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == request.AppointmentId, cancellationToken);

            if (appointment == null)
            {
                _logger.LogWarning("Status change failed: appointment {AppointmentId} not found.", request.AppointmentId);
                return false;
            }

            // Doctors may only touch their own bookings.
            if (appointment.DoctorId != request.DoctorId)
            {
                _logger.LogWarning(
                    "Status change rejected: doctor {DoctorId} tried to modify appointment {AppointmentId} owned by doctor {OwnerDoctorId}.",
                    request.DoctorId, request.AppointmentId, appointment.DoctorId);
                return false;
            }

            // Doctor cannot modify slots that haven't been booked, nor cancel appointments themselves.
            if (appointment.PatientId == null)
            {
                _logger.LogWarning("Status change rejected: appointment {AppointmentId} has no patient yet.", request.AppointmentId);
                return false;
            }

            if (request.NewStatus == AppointmentStatus.Cancelled || request.NewStatus == AppointmentStatus.Pending)
            {
                _logger.LogWarning(
                    "Status change rejected: doctors cannot move appointment {AppointmentId} to {NewStatus}.",
                    request.AppointmentId, request.NewStatus);
                return false;
            }

            appointment.Status = request.NewStatus;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    /// <summary>
    /// Admin-only: cancel an appointment and free up the underlying slot so another patient can book it.
    /// </summary>
    public class CancelAppointmentHandler : IRequestHandler<CancelAppointmentCommand, bool>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CancelAppointmentHandler> _logger;

        public CancelAppointmentHandler(ApplicationDbContext context, ILogger<CancelAppointmentHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == request.AppointmentId, cancellationToken);

            if (appointment == null)
            {
                _logger.LogWarning("Cancel failed: appointment {AppointmentId} not found.", request.AppointmentId);
                return false;
            }

            // Free the slot for re-booking by detaching the patient and resetting it to Pending.
            appointment.PatientId = null;
            appointment.Status = AppointmentStatus.Pending;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    /// <summary>
    /// Maps the logged-in user (ApplicationUser.Id) to the corresponding Doctor.Id.
    /// </summary>
    public class GetDoctorByUserIdHandler : IRequestHandler<GetDoctorByUserIdQuery, int?>
    {
        private readonly ApplicationDbContext _context;

        public GetDoctorByUserIdHandler(ApplicationDbContext context) => _context = context;

        public async Task<int?> Handle(GetDoctorByUserIdQuery request, CancellationToken cancellationToken)
        {
            var doctor = await _context.Doctors
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.UserId == request.UserId, cancellationToken);

            return doctor?.Id;
        }
    }

    /// <summary>
    /// Admin overview: all appointments in the system, with patient + doctor info eagerly loaded.
    /// </summary>
    public class GetAllAppointmentsHandler : IRequestHandler<GetAllAppointmentsQuery, List<AppointmentModel>>
    {
        private readonly ApplicationDbContext _context;

        public GetAllAppointmentsHandler(ApplicationDbContext context) => _context = context;

        public async Task<List<AppointmentModel>> Handle(GetAllAppointmentsQuery request, CancellationToken cancellationToken)
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Where(a => a.PatientId != null)
                .OrderByDescending(a => a.AppointmentDateTime)
                .ToListAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Patient portal — every booking owned by the logged-in patient (upcoming + past),
    /// with the booked doctor eagerly loaded.
    /// </summary>
    public class GetPatientAppointmentsHandler : IRequestHandler<GetPatientAppointmentsQuery, List<AppointmentModel>>
    {
        private readonly ApplicationDbContext _context;

        public GetPatientAppointmentsHandler(ApplicationDbContext context) => _context = context;

        public async Task<List<AppointmentModel>> Handle(GetPatientAppointmentsQuery request, CancellationToken cancellationToken)
        {
            return await _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Where(a => a.PatientId == request.PatientId)
                .OrderByDescending(a => a.AppointmentDateTime)
                .ToListAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Patient cancels one of their own bookings. Ownership is verified first; if the slot
    /// is in the future and not already Completed/Cancelled, the row is freed
    /// (PatientId = null, Status = Pending) so another patient can re-book it.
    /// </summary>
    public class PatientCancelAppointmentHandler : IRequestHandler<PatientCancelAppointmentCommand, bool>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PatientCancelAppointmentHandler> _logger;

        public PatientCancelAppointmentHandler(ApplicationDbContext context, ILogger<PatientCancelAppointmentHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> Handle(PatientCancelAppointmentCommand request, CancellationToken cancellationToken)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == request.AppointmentId, cancellationToken);

            if (appointment == null)
            {
                _logger.LogWarning("PatientCancel failed: appointment {AppointmentId} not found.", request.AppointmentId);
                return false;
            }

            if (appointment.PatientId != request.PatientId)
            {
                _logger.LogWarning(
                    "PatientCancel rejected: patient {PatientId} tried to cancel appointment {AppointmentId} owned by {OwnerId}.",
                    request.PatientId, request.AppointmentId, appointment.PatientId);
                return false;
            }

            if (appointment.Status == AppointmentStatus.Completed || appointment.Status == AppointmentStatus.Cancelled)
            {
                _logger.LogWarning(
                    "PatientCancel rejected: appointment {AppointmentId} is already {Status}.",
                    request.AppointmentId, appointment.Status);
                return false;
            }

            if (appointment.AppointmentDateTime <= DateTime.Now)
            {
                _logger.LogWarning(
                    "PatientCancel rejected: appointment {AppointmentId} is in the past.",
                    request.AppointmentId);
                return false;
            }

            appointment.PatientId = null;
            appointment.Status = AppointmentStatus.Pending;

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Patient {PatientId} cancelled appointment {AppointmentId}.",
                request.PatientId, request.AppointmentId);
            return true;
        }
    }
}
