using AppointmentSystem.Models;
using AppointmentSystem.Models.Models;
using MediatR;

namespace AppointmentSystem.Models.Contracts.CommandRequests.Admin
{
    public record UpdateUserCommand(
        int Id,
        string FullName,
        string Email,
        UserRole Role
    ) : IRequest<bool>;
}