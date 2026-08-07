using Identity.DTOs;
using Identity.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Identity.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IJwtService _jwtService;

    public RegisterCommandHandler(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IJwtService jwtService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Email is already registered.",
                Errors = new List<string> { "A user with this email already exists." }
            };
        }

        var user = new IdentityUser
        {
            UserName = request.Email,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Registration failed.",
                Errors = result.Errors.Select(e => e.Description).ToList()
            };
        }

        if (!await _roleManager.RoleExistsAsync("Customer"))
            await _roleManager.CreateAsync(new IdentityRole("Customer"));

        await _userManager.AddToRoleAsync(user, "Customer");

        var token = await _jwtService.GenerateTokenAsync(user);

        return new AuthResponse
        {
            Success = true,
            Message = "Registration successful.",
            UserId = user.Id,
            Email = user.Email,
            DisplayName = request.FirstName + " " + request.LastName,
            Token = token,
            Roles = new List<string> { "Customer" }
        };
    }
}
