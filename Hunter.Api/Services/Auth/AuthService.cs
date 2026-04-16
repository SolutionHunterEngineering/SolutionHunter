using Microsoft.AspNetCore.Identity;
using IdentityDomain; 

namespace Hunter.Api.Services;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _users;
    private readonly SignInManager<AppUser> _signIn;

    public AuthService(UserManager<AppUser> users, SignInManager<AppUser> signIn)
    {
        _users = users;
        _signIn = signIn;
    }

    public async Task<AuthResponse> LoginAsync(LoginDto dto)
    {
        if (dto is null) return AuthResponse.Fail("Missing credentials.");
        // username or email ï¿½ try both
        var user = await _users.FindByNameAsync(dto.UserName)
                   ?? await _users.FindByEmailAsync(dto.UserName);
        if (user is null) return AuthResponse.Fail("User not found.");

        var pw = await _signIn.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: false);
        if (!pw.Succeeded) return AuthResponse.Fail("Invalid password.");

        // Cookie sign-in (optional while youï¿½re still wiring hubs):
        // await _signIn.SignInAsync(user, isPersistent: dto.RememberMe);

        return AuthResponse.Ok(user.Id.ToString(), user.UserName ?? user.Email ?? dto.UserName);
    }

    public Task LogoutAsync() => _signIn.SignOutAsync();

    public async Task<UserProfileDto?> GetUserProfileAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return null;
        var user = await _users.FindByIdAsync(userId);
        if (user is null) return null;
        return new UserProfileDto
        {
            Id = user.Id.ToString(),
            UserName = user.UserName ?? "",
            Email = user.Email
        };
    }
}
