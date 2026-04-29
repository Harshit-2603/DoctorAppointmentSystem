using AppointmentSystem.Data.Data;
using AppointmentSystem.Models.Contracts.CommandRequests.Appointment;
using AppointmentSystem.Models.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AppointmentModel = AppointmentSystem.Models.Models.Appointment;

namespace AppointmentSystem.Services.Handlers.AppointmentFeatures
{
    /// <summary>
    /// Returns the still-bookable slot rows (status = Pending, no patient yet) for a doctor on a given day.
    /// Slots themselves are pre-generated rows in the Appointments table by <see cref="AdminFeatures.GenerateSlotsHandler"/>.
    /// </summary>
    public class GetAvailableSlotsHandler : IRequestHandler<GetAvailableSlotsCommand, List<AppointmentModel>>
    {
        private readonly ApplicationDbContext _context;

        public GetAvailableSlotsHandler(ApplicationDbContext context) => _context = context;

        public async Task<List<AppointmentModel>> Handle(GetAvailableSlotsCommand request, CancellationToken cancellationToken)
        {
            var dayStart = request.Date.Date;
            var dayEnd = dayStart.AddDays(1);

            return await _context.Appointments
                .Where(a => a.DoctorId == request.DoctorId
                            && a.AppointmentDateTime >= dayStart
                            && a.AppointmentDateTime < dayEnd
                            && a.Status == AppointmentStatus.Pending
                            && a.PatientId == null)
                .OrderBy(a => a.AppointmentDateTime)
                .ToListAsync(cancellationToken);
        }
    }
}
