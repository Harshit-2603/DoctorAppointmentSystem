using AppointmentSystem.Models.Contracts.CommandRequests.Appointment;
using AppointmentSystem.Models.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize(Roles = "Doctor")]
public class DoctorController : Controller
{
    private readonly IMediator _mediator;

    public DoctorController(IMediator mediator) => _mediator = mediator;

    public async Task<IActionResult> Dashboard()
    {
        var doctorId = await ResolveDoctorIdAsync();
        if (doctorId == null) return Forbid();

        var appointments = await _mediator.Send(new GetDoctorBookingsCommand(doctorId.Value));
        return View(appointments);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int appointmentId, AppointmentStatus newStatus)
    {
        var doctorId = await ResolveDoctorIdAsync();
        if (doctorId == null) return Forbid();

        var ok = await _mediator.Send(new UpdateAppointmentStatusCommand(appointmentId, doctorId.Value, newStatus));
        TempData[ok ? "Info" : "Error"] = ok ? "Status updated." : "Could not update status.";
        return RedirectToAction(nameof(Dashboard));
    }

    private async Task<int?> ResolveDoctorIdAsync()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out var userId)) return null;

        return await _mediator.Send(new GetDoctorByUserIdQuery(userId));
    }
}
