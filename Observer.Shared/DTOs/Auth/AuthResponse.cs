namespace Observer.Shared.DTOs.Auth;

public sealed record AuthResponse
{
    public bool Success { get; init; }
    public string? Error { get; init; }

    public string? Jwt { get; init; }

    public int UserId { get; init; }
    public string? UserName { get; init; }

    public static AuthResponse Fail(string error) =>
        new() { Success = false, Error = error };

    public static AuthResponse Ok(int userId, string userName, string? jwt = null) =>
        new() { Success = true, UserId = userId, UserName = userName, Jwt = jwt };
}
