using AppointmentSystem.Data;
using AppointmentSystem.Data.Data;
using AppointmentSystem.Models.Contracts.CommandRequests.Admin;
using AppointmentSystem.Models.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AppointmentModel = AppointmentSystem.Models.Models.Appointment;

namespace AppointmentSystem.Services.Handlers.AdminFeatures
{
    public class GenerateSlotsHandler : IRequestHandler<GenerateSlotsCommand, bool>
    {
        private readonly ApplicationDbContext _context;

        public GenerateSlotsHandler(ApplicationDbContext context) => _context = context;

        public async Task<bool> Handle(GenerateSlotsCommand request, CancellationToken cancellationToken)
        {
            var availability = await _context.Availabilities
                .FirstOrDefaultAsync(a => a.Id == request.AvailabilityId, cancellationToken);

            if (availability == null) return false;

            // StartTime and EndTime are DateTime so use them directly
            var currentTime = availability.StartTime;
            var endTime = availability.EndTime;

            while (currentTime < endTime)
            {
                _context.Appointments.Add(new AppointmentModel
                {
                    DoctorId = availability.DoctorId,
                    AppointmentDateTime = currentTime,
                    Status = AppointmentStatus.Pending
                });

                currentTime = currentTime.AddMinutes(availability.SlotSizeInMinutes);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}