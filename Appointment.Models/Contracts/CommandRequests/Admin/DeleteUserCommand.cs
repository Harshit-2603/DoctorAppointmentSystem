using MediatR;

namespace AppointmentSystem.Models.Contracts.CommandRequests.Admin
{
    public record DeleteUserCommand(int Id) : IRequest<bool>;
}