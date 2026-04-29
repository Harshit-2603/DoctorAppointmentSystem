using AppointmentSystem.Data.Data;
using AppointmentSystem.Models.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AppointmentSystem.Models.Contracts.CommandRequests.Admin;

namespace AppointmentSystem.Services.Handlers.AdminFeatures
{
    public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public DeleteUserHandler(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            // Step 1: Find the user
            var user = await _userManager.FindByIdAsync(request.Id.ToString());
            if (user == null) return false;

            // Step 2: Delete appointments where this user is the Patient
            var patientAppointments = _context.Appointments
                .Where(a => a.PatientId == request.Id);
            _context.Appointments.RemoveRange(patientAppointments);

            // Step 3: If Doctor — delete their appointments first
            // (Availabilities auto-delete via Cascade, Doctor profile also Cascades)
            var doctorProfile = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == request.Id, cancellationToken);

            if (doctorProfile != null)
            {
                // Delete appointments assigned to this doctor
                var doctorAppointments = _context.Appointments
                    .Where(a => a.DoctorId == doctorProfile.Id);
                _context.Appointments.RemoveRange(doctorAppointments);

                // Delete availabilities for this doctor
                var availabilities = _context.Availabilities
                    .Where(a => a.DoctorId == doctorProfile.Id);
                _context.Availabilities.RemoveRange(availabilities);

                // Delete doctor profile
                // (This also cascades to Availabilities but we deleted them above to be safe)
                _context.Doctors.Remove(doctorProfile);
            }

            // Step 4: Remove from AspNetUserRoles
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Any())
                await _userManager.RemoveFromRolesAsync(user, roles);

            // Step 5: Save all related deletions
            await _context.SaveChangesAsync(cancellationToken);

            // Step 6: Now safely delete the user
            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }
    }
}