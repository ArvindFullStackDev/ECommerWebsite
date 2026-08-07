using Identity.DTOs;
using MediatR;

namespace Identity.Commands.Login;

public class LoginCommand : IRequest<AuthResponse>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }

    public static LoginCommand FromRequest(LoginRequest request) => new()
    {
        Email = request.Email,
        Password = request.Password,
        RememberMe = request.RememberMe
    };
}
