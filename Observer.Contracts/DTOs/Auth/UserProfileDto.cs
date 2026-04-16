// Hunter.Shared/DTOs/UserProfileDto.cs
namespace Hunter.Shared.DTOs.Auth;

public class UserProfileDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string KnownAs { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty;
    public List<string> Capabilities { get; set; } = new();
}
