using AppointmentSystem.Data.Data;
using AppointmentSystem.Models;
using AppointmentSystem.Models.Contracts.CommandRequests.Admin;
using AppointmentSystem.Models.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AppointmentSystem.Services.Handlers.AdminFeatures
{
    public class CreateUserHandler : IRequestHandler<CreateUserCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public CreateUserHandler(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<bool> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName,
                Role = request.Role
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded) return false;

            // Map enum to exact role name string matching what was seeded
            var roleName = request.Role switch
            {
                UserRole.Admin => "Admin",
                UserRole.Doctor => "Doctor",
                UserRole.Patient => "Patient",
                _ => "Patient"
            };

            await _userManager.AddToRoleAsync(user, roleName);

            if (request.Role == UserRole.Doctor)
            {
                _context.Doctors.Add(new Doctor
                {
                    UserId = user.Id,
                    Specialization = request.Specialization ?? "General",
                    Bio = string.Empty
                });
                await _context.SaveChangesAsync(cancellationToken);
            }

            return true;
        }
    }
}