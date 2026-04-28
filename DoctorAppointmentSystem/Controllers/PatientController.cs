using AppointmentSystem.Models.Contracts.CommandRequests.Appointment;
using AppointmentSystem.Models.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[Authorize(Roles = "Patient")]
public class PatientController : Controller
{
    private readonly IMediator _mediator;
    private readonly UserManager<ApplicationUser> _userManager;

    public PatientController(IMediator mediator, UserManager<ApplicationUser> userManager)
    {
        _mediator = mediator;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var doctors = await _userManager.Users
            .Where(u => u.Role == UserRole.Doctor)
            .ToListAsync();
        return View(doctors);
    }

    public async Task<IActionResult> AvailableSlots(int doctorId, DateTime? date)
    {
        var targetDate = date ?? DateTime.Today;
        var slots = await _mediator.Send(new GetAvailableSlotsCommand(doctorId, targetDate));
        return View(slots);
    }

    [HttpPost]
    public async Task<IActionResult> Book(int appointmentId)
    {
        // Use '??' to handle potential nulls from FindFirstValue to fix CS8604
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0";
        var patientId = int.Parse(userIdClaim);

        // This now matches the 2-parameter constructor above
        var result = await _mediator.Send(new BookAppointmentCommand(appointmentId, patientId));

        if (result) return RedirectToAction(nameof(Index));
        return BadRequest("Could not book appointment.");
    }
}