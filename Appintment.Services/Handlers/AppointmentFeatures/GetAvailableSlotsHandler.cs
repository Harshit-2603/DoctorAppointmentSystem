using AppointmentSystem.Data;
using AppointmentSystem.Data.Data;
using AppointmentSystem.Models.Contracts.CommandRequests.Appointment;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Services.Handlers.AppointmentFeatures
{
    public class GetAvailableSlotsHandler : IRequestHandler<GetAvailableSlotsCommand, List<DateTime>>
    {
        private readonly ApplicationDbContext _context;

        public GetAvailableSlotsHandler(ApplicationDbContext context) => _context = context;

        public async Task<List<DateTime>> Handle(GetAvailableSlotsCommand request, CancellationToken cancellationToken)
        {
            // Returns available slot DateTimes for a doctor on a given date
            // A slot is available if no confirmed booking exists for it
            var bookedSlots = await _context.Appointments
                .Where(a => a.DoctorId == request.DoctorId
                    && a.AppointmentDateTime.Date == request.Date.Date
                    && a.Status == AppointmentSystem.Models.Models.AppointmentStatus.Confirmed)
                .Select(a => a.AppointmentDateTime)
                .ToListAsync(cancellationToken);

            var availabilities = await _context.Availabilities
                .Where(a => a.DoctorId == request.DoctorId && a.IsActive)
                .ToListAsync(cancellationToken);

            var availableSlots = new List<DateTime>();

            foreach (var availability in availabilities)
            {
                var current = request.Date.Date
                    .AddHours(availability.StartTime.Hour)
                    .AddMinutes(availability.StartTime.Minute);

                var end = request.Date.Date
                    .AddHours(availability.EndTime.Hour)
                    .AddMinutes(availability.EndTime.Minute);

                while (current < end)
                {
                    if (!bookedSlots.Contains(current))
                        availableSlots.Add(current);

                    current = current.AddMinutes(availability.SlotSizeInMinutes);
                }
            }

            return availableSlots;
        }
    }
}