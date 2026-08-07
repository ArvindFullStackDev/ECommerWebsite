using Identity.DTOs;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Identity.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, AuthResponse>
{
    private readonly UserManager<IdentityUser> _userManager;

    public ForgotPasswordCommandHandler(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<AuthResponse> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return new AuthResponse
            {
                Success = true,
                Message = "If the email is registered, you will receive a password reset link."
            };
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        // In production, send email with the reset link containing the token
        return new AuthResponse
        {
            Success = true,
            Message = "If the email is registered, you will receive a password reset link."
        };
    }
}
