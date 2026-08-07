namespace Domain.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? Email { get; }
    string? DisplayName { get; }
    string? Role { get; }
    bool IsAuthenticated { get; }
}
