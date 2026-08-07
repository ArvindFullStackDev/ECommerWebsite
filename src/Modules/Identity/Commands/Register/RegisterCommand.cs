using Identity.DTOs;
using MediatR;

namespace Identity.Commands.Register;

public class RegisterCommand : IRequest<AuthResponse>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }

    public static RegisterCommand FromRequest(RegisterRequest request) => new()
    {
        Email = request.Email,
        Password = request.Password,
        ConfirmPassword = request.ConfirmPassword,
        FirstName = request.FirstName,
        LastName = request.LastName,
        PhoneNumber = request.PhoneNumber
    };
}
