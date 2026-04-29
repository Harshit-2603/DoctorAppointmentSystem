using AppointmentSystem.Data.Data;
using AppointmentSystem.Models.Contracts.CommandRequests.Admin;
using AppointmentSystem.Models.Contracts.CommandRequests.Appointment;
using AppointmentSystem.Models.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IMediator _mediator;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IMediator mediator,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db,
        ILogger<AdminController> logger)
    {
        _mediator = mediator;
        _userManager = userManager;
        _db = db;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.OrderBy(u => u.FullName).ToListAsync();

        var appointments = await _mediator.Send(new GetAllAppointmentsQuery());
        ViewBag.Appointments = appointments;

        return View(users);
    }

    // ─── Create ────────────────────────────────────────────
    [HttpGet]
    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserCommand command)
    {
        if (!ModelState.IsValid) return View(command);

        var ok = await _mediator.Send(command);
        if (ok) return RedirectToAction(nameof(Index));

        ModelState.AddModelError("", "User creation failed. Password must be ≥6 chars and contain uppercase, digit, and special character.");
        return View(command);
    }

    // ─── Edit ──────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound();
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateUserCommand command)
    {
        var ok = await _mediator.Send(command);
        if (ok) return RedirectToAction(nameof(Index));

        ModelState.AddModelError("", "Update failed.");
        var user = await _userManager.FindByIdAsync(command.Id.ToString());
        return View(user);
    }

    // ─── Delete ────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound();
        return View(user);
    }

    [HttpPost, ActionName("DeleteConfirmed")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _mediator.Send(new DeleteUserCommand(id));
        return RedirectToAction(nameof(Index));
    }

    // ─── Set Doctor Availability ───────────────────────────
    [HttpGet]
    public async Task<IActionResult> SetAvailability(int id)
    {
        var doctor = await _db.Doctors
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.UserId == id);

        if (doctor == null)
        {
            TempData["Error"] = "Selected user is not a doctor.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.DoctorId = doctor.Id;
        ViewBag.DoctorName = doctor.User?.FullName;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetAvailability(SetAvailabilityCommand command)
    {
        if (command.EndTime <= command.StartTime)
        {
            ModelState.AddModelError("", "End time must be after start time.");
            ViewBag.DoctorId = command.DoctorId;
            return View();
        }

        await _mediator.Send(command);
        TempData["Info"] = "Availability saved. You can now generate slots.";
        return RedirectToAction(nameof(GenerateSlots), new { id = command.DoctorId });
    }

    // ─── Generate Slots from Availability ──────────────────
    [HttpGet]
    public async Task<IActionResult> GenerateSlots(int id)
    {
        var doctor = await _db.Doctors
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.UserId == id || d.Id == id);

        if (doctor == null)
        {
            TempData["Error"] = "Doctor not found.";
            return RedirectToAction(nameof(Index));
        }

        var availabilities = await _db.Availabilities
            .Where(a => a.DoctorId == doctor.Id)
            .OrderByDescending(a => a.StartTime)
            .ToListAsync();

        ViewBag.DoctorId = doctor.Id;
        ViewBag.DoctorName = doctor.User?.FullName;
        return View(availabilities);
    }

    [HttpPost, ActionName("GenerateSlots")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateSlotsPost(int availabilityId, int doctorId)
    {
        await _mediator.Send(new GenerateSlotsCommand(availabilityId));
        TempData["Info"] = "Slots generated successfully.";
        return RedirectToAction(nameof(GenerateSlots), new { id = doctorId });
    }

    // ─── Cancel an Appointment ─────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelAppointment(int appointmentId)
    {
        var ok = await _mediator.Send(new CancelAppointmentCommand(appointmentId));
        TempData[ok ? "Info" : "Error"] = ok ? "Appointment cancelled." : "Could not cancel appointment.";
        return RedirectToAction(nameof(Index));
    }
}
