// Hunter.Shared/DTOs/AuthResponse.cs
namespace Hunter.Shared.DTOs.Auth;

public class AuthResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int UserId { get; set; }
    public string? Username { get; set; }
    public string? UserType { get; set; }
}
