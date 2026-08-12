using System.ComponentModel.DataAnnotations;
using Domain.Entities;
using Domain.Interfaces;
using Identity.Commands.Login;
using Identity.Commands.Register;
using Identity.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebUI.Controllers;

public class AccountController : Controller
{
    private readonly IMediator _mediator;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public AccountController(IMediator mediator, SignInManager<IdentityUser> signInManager,
        IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _signInManager = signInManager;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginRequest request, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(request);

        var command = LoginCommand.FromRequest(request);
        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Login failed");
            return View(request);
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (!ModelState.IsValid) return View(request);

        var command = RegisterCommand.FromRequest(request);
        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            foreach (var error in result.Errors ?? new List<string>())
                ModelState.AddModelError(string.Empty, error);
            return View(request);
        }

        TempData["SuccessMessage"] = "Registration successful! Please verify your email.";
        return RedirectToAction("Login");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ResetPassword(string token = "", string email = "")
    {
        var model = new ResetPasswordRequest { Token = token, Email = email };
        return View(model);
    }

    [HttpGet]
    public IActionResult Profile()
    {
        return View();
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Addresses()
    {
        var addresses = await _unitOfWork.Repository<Address>().GetQueryable()
            .Where(a => a.UserId == _currentUser.UserId)
            .OrderByDescending(a => a.IsDefault)
            .ToListAsync();
        return View(addresses);
    }

    [HttpGet]
    [Authorize]
    public IActionResult AddAddress()
    {
        return View("AddressForm", new AddressFormModel { Country = "India" });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAddress(AddressFormModel model)
    {
        if (!ModelState.IsValid) return View("AddressForm", model);

        var hasAny = await _unitOfWork.Repository<Address>().GetQueryable()
            .AnyAsync(a => a.UserId == _currentUser.UserId);

        var address = new Address
        {
            UserId = _currentUser.UserId!,
            FullName = model.FullName!.Trim(),
            PhoneNumber = model.PhoneNumber!.Trim(),
            AddressLine1 = model.AddressLine1!.Trim(),
            AddressLine2 = model.AddressLine2?.Trim(),
            City = model.City!.Trim(),
            State = model.State!.Trim(),
            ZipCode = model.ZipCode!.Trim(),
            Country = model.Country!.Trim(),
            IsDefault = model.IsDefault || !hasAny
        };
        _unitOfWork.Repository<Address>().AddAsync(address);
        await _unitOfWork.CompleteAsync();
        return RedirectToAction(nameof(Addresses));
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> EditAddress(int id)
    {
        var address = await _unitOfWork.Repository<Address>().GetQueryable()
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == _currentUser.UserId);
        if (address == null) return NotFound();

        return View("AddressForm", new AddressFormModel
        {
            Id = address.Id,
            FullName = address.FullName,
            PhoneNumber = address.PhoneNumber,
            AddressLine1 = address.AddressLine1,
            AddressLine2 = address.AddressLine2,
            City = address.City,
            State = address.State,
            ZipCode = address.ZipCode,
            Country = address.Country,
            IsDefault = address.IsDefault
        });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAddress(AddressFormModel model)
    {
        if (!ModelState.IsValid) return View("AddressForm", model);

        var address = await _unitOfWork.Repository<Address>().GetQueryable()
            .FirstOrDefaultAsync(a => a.Id == model.Id && a.UserId == _currentUser.UserId);
        if (address == null) return NotFound();

        address.FullName = model.FullName!.Trim();
        address.PhoneNumber = model.PhoneNumber!.Trim();
        address.AddressLine1 = model.AddressLine1!.Trim();
        address.AddressLine2 = model.AddressLine2?.Trim();
        address.City = model.City!.Trim();
        address.State = model.State!.Trim();
        address.ZipCode = model.ZipCode!.Trim();
        address.Country = model.Country!.Trim();
        address.IsDefault = model.IsDefault;
        _unitOfWork.Repository<Address>().Update(address);
        await _unitOfWork.CompleteAsync();
        return RedirectToAction(nameof(Addresses));
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveAddress(int id)
    {
        var address = await _unitOfWork.Repository<Address>().GetQueryable()
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == _currentUser.UserId);
        if (address != null)
        {
            _unitOfWork.Repository<Address>().Delete(address);
            await _unitOfWork.CompleteAsync();
        }
        return RedirectToAction(nameof(Addresses));
    }
}

public class AddressFormModel
{
    public int Id { get; set; }

    [Required]
    public string? FullName { get; set; }

    [Required]
    public string? PhoneNumber { get; set; }

    [Required]
    public string? AddressLine1 { get; set; }

    public string? AddressLine2 { get; set; }

    [Required]
    public string? City { get; set; }

    [Required]
    public string? State { get; set; }

    [Required]
    public string? ZipCode { get; set; }

    [Required]
    public string? Country { get; set; }

    public bool IsDefault { get; set; }
}
