using AppointmentSystem.Data;
using AppointmentSystem.Data.Data;
using AppointmentSystem.Models.Contracts.CommandRequests.Appointment;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AppointmentModel = AppointmentSystem.Models.Models.Appointment;

namespace AppointmentSystem.Services.Handlers.AppointmentFeatures
{
    public class GetDoctorBookingsHandler : IRequestHandler<GetDoctorBookingsCommand, List<AppointmentModel>>
    {
        private readonly ApplicationDbContext _context;

        public GetDoctorBookingsHandler(ApplicationDbContext context) => _context = context;

        public async Task<List<AppointmentModel>> Handle(GetDoctorBookingsCommand request, CancellationToken cancellationToken)
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == request.DoctorId)
                .OrderBy(a => a.AppointmentDateTime)
                .ToListAsync(cancellationToken);
        }
    }
}