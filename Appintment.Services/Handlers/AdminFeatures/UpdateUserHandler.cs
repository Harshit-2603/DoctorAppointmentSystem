using AppointmentSystem.Models.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using AppointmentSystem.Models.Contracts.CommandRequests.Admin;

namespace AppointmentSystem.Services.Handlers.AdminFeatures
{
    public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UpdateUserHandler(UserManager<ApplicationUser> userManager) => _userManager = userManager;

        public async Task<bool> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.Id.ToString());
            if (user == null) return false;

            user.FullName = request.FullName;
            user.Email = request.Email;
            user.UserName = request.Email;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }
    }
}