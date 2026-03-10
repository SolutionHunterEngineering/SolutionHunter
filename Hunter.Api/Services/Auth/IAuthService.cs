namespace Hunter.Api.Services;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginDto dto);
    Task LogoutAsync();
    Task<UserProfileDto?> GetUserProfileAsync(string userId);
}

public sealed record AuthResponse
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public string? UserId { get; init; }
    public string? UserName { get; init; }
    public static AuthResponse Fail(string error) => new() { Success = false, Error = error };
    public static AuthResponse Ok(string userId, string userName) => new() { Success = true, UserId = userId, UserName = userName };
}

// adjust namespaces/types to your existing Shared DTOs if needed:
public sealed record LoginDto(string UserName, string Password, bool RememberMe = false);

public sealed record UserProfileDto
{
    public string Id { get; init; } = "";
    public string UserName { get; init; } = "";
    public string? Email { get; init; }
}
