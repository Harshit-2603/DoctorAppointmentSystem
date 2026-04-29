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
            .OrderBy(u => u.FullName)
            .ToListAsync();

        var patientId = GetCurrentUserId();
        if (patientId.HasValue)
        {
            var myAppointments = await _mediator.Send(new GetPatientAppointmentsQuery(patientId.Value));
            ViewBag.MyAppointments = myAppointments;
        }
        else
        {
            ViewBag.MyAppointments = new List<AppointmentSystem.Models.Models.Appointment>();
        }

        return View(doctors);
    }

    public async Task<IActionResult> AvailableSlots(int doctorId, DateTime? date)
    {
        var targetDate = date ?? DateTime.Today;

        // doctorId arrives here as the doctor's *user* Id (Patient/Index lists ApplicationUsers).
        // Resolve to the Doctor entity Id before querying slots.
        var resolvedDoctorId = await _mediator.Send(new GetDoctorByUserIdQuery(doctorId));
        if (resolvedDoctorId == null)
        {
            TempData["Error"] = "Doctor not found.";
            return RedirectToAction(nameof(Index));
        }

        var slots = await _mediator.Send(new GetAvailableSlotsCommand(resolvedDoctorId.Value, targetDate));

        ViewBag.DoctorUserId = doctorId;
        ViewBag.SelectedDate = targetDate;
        return View(slots);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(int appointmentId)
    {
        var patientId = GetCurrentUserId();
        if (!patientId.HasValue)
        {
            TempData["Error"] = "Could not identify the current user.";
            return RedirectToAction(nameof(Index));
        }

        var ok = await _mediator.Send(new BookAppointmentCommand(appointmentId, patientId.Value));

        if (ok)
        {
            TempData["Info"] = "Appointment booked successfully.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Error"] = "Could not book appointment. The slot may already be taken, or you already have an active booking with this doctor or for that day.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelMyAppointment(int appointmentId)
    {
        var patientId = GetCurrentUserId();
        if (!patientId.HasValue)
        {
            TempData["Error"] = "Could not identify the current user.";
            return RedirectToAction(nameof(Index));
        }

        var ok = await _mediator.Send(new PatientCancelAppointmentCommand(appointmentId, patientId.Value));
        TempData[ok ? "Info" : "Error"] = ok
            ? "Your appointment was cancelled."
            : "Could not cancel this appointment.";

        return RedirectToAction(nameof(Index));
    }

    private int? GetCurrentUserId()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdStr, out var id) ? id : null;
    }
}
