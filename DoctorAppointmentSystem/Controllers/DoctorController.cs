using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using AppointmentSystem.Models.Contracts.CommandRequests.Appointment;

[Authorize(Roles = "Doctor")]
public class DoctorController : Controller
{
    private readonly IMediator _mediator;

    public DoctorController(IMediator mediator) => _mediator = mediator;

    public async Task<IActionResult> Dashboard()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0";
        var doctorId = int.Parse(userIdStr);
        // Uses the corrected Command returning List<Appointment>
        var appointments = await _mediator.Send(new GetDoctorBookingsCommand(doctorId));
        return View(appointments);
    }
}