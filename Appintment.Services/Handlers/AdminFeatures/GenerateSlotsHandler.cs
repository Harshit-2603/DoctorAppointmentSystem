using AppointmentSystem.Data.Data;
using AppointmentSystem.Models.Contracts.CommandRequests.Admin;
using AppointmentSystem.Models.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AppointmentModel = AppointmentSystem.Models.Models.Appointment;

namespace AppointmentSystem.Services.Handlers.AdminFeatures
{
    /// <summary>
    /// Walks the Availability window and creates one Pending Appointment row per slot,
    /// where slot length = Availability.SlotSizeInMinutes. Existing slots for the same
    /// doctor + datetime are skipped so this can be re-run safely.
    /// </summary>
    public class GenerateSlotsHandler : IRequestHandler<GenerateSlotsCommand, bool>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GenerateSlotsHandler> _logger;

        public GenerateSlotsHandler(ApplicationDbContext context, ILogger<GenerateSlotsHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> Handle(GenerateSlotsCommand request, CancellationToken cancellationToken)
        {
            var availability = await _context.Availabilities
                .FirstOrDefaultAsync(a => a.Id == request.AvailabilityId, cancellationToken);

            if (availability == null)
            {
                _logger.LogWarning("GenerateSlots failed: availability {AvailabilityId} not found.", request.AvailabilityId);
                return false;
            }

            if (availability.SlotSizeInMinutes <= 0)
            {
                _logger.LogWarning("GenerateSlots failed: availability {AvailabilityId} has non-positive slot size.", request.AvailabilityId);
                return false;
            }

            // Pull existing slot times for this doctor across the window, so we don't double-create.
            var existing = await _context.Appointments
                .Where(a => a.DoctorId == availability.DoctorId
                            && a.AppointmentDateTime >= availability.StartTime
                            && a.AppointmentDateTime < availability.EndTime)
                .Select(a => a.AppointmentDateTime)
                .ToListAsync(cancellationToken);

            var existingSet = new HashSet<DateTime>(existing);

            var current = availability.StartTime;
            var created = 0;

            while (current < availability.EndTime)
            {
                if (!existingSet.Contains(current))
                {
                    _context.Appointments.Add(new AppointmentModel
                    {
                        DoctorId = availability.DoctorId,
                        AppointmentDateTime = current,
                        Status = AppointmentStatus.Pending,
                        PatientId = null
                    });
                    created++;
                }
                current = current.AddMinutes(availability.SlotSizeInMinutes);
            }

            if (created > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation("GenerateSlots: created {Count} slot(s) for availability {AvailabilityId}.", created, request.AvailabilityId);
            return true;
        }
    }
}
