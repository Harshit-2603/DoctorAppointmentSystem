using AppointmentSystem.Data.Data;
using AppointmentSystem.Models.Contracts.CommandRequests.Admin;
using AppointmentSystem.Models.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Numerics;

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
            if (result.Succeeded)
            {
                if (user.Role == UserRole.Doctor)
                {
                    _context.Doctors.Add(new Doctor { UserId = user.Id, Specialization = "General" });
                    await _context.SaveChangesAsync(cancellationToken);
                }
                return true;
            }
            return false;
        }
    }
}