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
    public IActionResult Addresses()
    {
        return View();
    }
}
