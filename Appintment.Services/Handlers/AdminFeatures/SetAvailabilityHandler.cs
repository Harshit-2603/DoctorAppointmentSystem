using AppointmentSystem.Data;
using AppointmentSystem.Data.Data;
using AppointmentSystem.Models.Contracts.CommandRequests.Admin;
using AppointmentSystem.Models.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Services.Handlers.AdminFeatures
{
    public class SetAvailabilityHandler : IRequestHandler<SetAvailabilityCommand, bool>
    {
        private readonly ApplicationDbContext _context;

        public SetAvailabilityHandler(ApplicationDbContext context) => _context = context;

        public async Task<bool> Handle(SetAvailabilityCommand request, CancellationToken cancellationToken)
        {
            var existing = await _context.Availabilities
                .FirstOrDefaultAsync(a => a.DoctorId == request.DoctorId
                    && a.StartTime == request.StartTime, cancellationToken);

            if (existing != null)
            {
                existing.StartTime = request.StartTime;
                existing.EndTime = request.EndTime;
                existing.SlotSizeInMinutes = request.SlotSizeInMinutes;
            }
            else
            {
                _context.Availabilities.Add(new Availability
                {
                    DoctorId = request.DoctorId,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    SlotSizeInMinutes = request.SlotSizeInMinutes,
                    IsActive = true
                });
            }

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}