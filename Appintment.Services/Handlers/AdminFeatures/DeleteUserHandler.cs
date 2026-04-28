using AppointmentSystem.Data.Data;
using AppointmentSystem.Models.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using AppointmentSystem.Models.Contracts.CommandRequests.Admin;

namespace AppointmentSystem.Services.Handlers.AdminFeatures
{
    public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public DeleteUserHandler(UserManager<ApplicationUser> userManager) => _userManager = userManager;

        public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.Id.ToString());
            if (user == null) return false;

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }
    }
}