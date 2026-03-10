// Observer.Api/Services/Auth/IAuthService.cs
using Observer.Shared.DTOs.Auth;

namespace Observer.Api.Services;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest dto);
    Task LogoutAsync();
    Task<UserProfileDto?> GetUserProfileAsync(string userId);
}

public sealed record UserProfileDto
{
    public string Id { get; init; } = "";
    public string UserName { get; init; } = "";
    public string? Email { get; init; }
}
