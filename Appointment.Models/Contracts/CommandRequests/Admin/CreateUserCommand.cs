using AppointmentSystem.Models;
using AppointmentSystem.Models.Models;
using MediatR;

namespace AppointmentSystem.Models.Contracts.CommandRequests.Admin
{
    public record CreateUserCommand(
        string FullName,
        string Email,
        string Password,
        UserRole Role,
        string? Specialization = null
    ) : IRequest<bool>;
}