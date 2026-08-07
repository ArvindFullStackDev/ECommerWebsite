using MediatR;

namespace Identity.Commands.ForgotPassword;

public class ForgotPasswordCommand : IRequest<Identity.DTOs.AuthResponse>
{
    public string Email { get; set; } = string.Empty;
}
