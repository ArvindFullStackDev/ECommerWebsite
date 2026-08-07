using Identity.DTOs;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Identity.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, AuthResponse>
{
    private readonly UserManager<IdentityUser> _userManager;

    public ResetPasswordCommandHandler(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<AuthResponse> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Invalid request."
            };
        }

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.Password);
        if (!result.Succeeded)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Password reset failed.",
                Errors = result.Errors.Select(e => e.Description).ToList()
            };
        }

        return new AuthResponse
        {
            Success = true,
            Message = "Password has been reset successfully."
        };
    }
}
