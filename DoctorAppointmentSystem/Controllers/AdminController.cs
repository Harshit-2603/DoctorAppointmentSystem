using AppointmentSystem.Models.Contracts.CommandRequests.Admin;
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

    public AdminController(IMediator mediator, UserManager<ApplicationUser> userManager)
    {
        _mediator = mediator;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.ToListAsync();
        return View(users);
    }

    [HttpGet]
    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserCommand command)
    {
        var result = await _mediator.Send(command);
        if (result) return RedirectToAction(nameof(Index));
        ModelState.AddModelError("", "User creation failed.");
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(UpdateUserCommand command)
    {
        // Matches command property 'UserId' as discussed earlier
        var result = await _mediator.Send(command);
        if (result) return RedirectToAction(nameof(Index));
        return View();
    }

    [HttpGet]
    public IActionResult SetAvailability(int id)
    {
        ViewBag.DoctorId = id;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SetAvailability(SetAvailabilityCommand command)
    {
        var result = await _mediator.Send(command);
        return RedirectToAction(nameof(Index));
    }
}