// Observer.Api/Services/Auth/AuthService.cs
using Microsoft.AspNetCore.Identity;
using Observer.Shared.DTOs.Auth;
using Observer.Shared.Identity; // AppUser

namespace Observer.Api.Services;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _users;
    private readonly SignInManager<AppUser> _signIn;

    public AuthService(UserManager<AppUser> users, SignInManager<AppUser> signIn)
    {
        _users = users;
        _signIn = signIn;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest dto)
    {
        if (dto is null) return AuthResponse.Fail("Missing credentials.");

        var user = await _users.FindByNameAsync(dto.User)
                   ?? await _users.FindByEmailAsync(dto.User);

        if (user is null) return AuthResponse.Fail("User not found.");

        var pw = await _signIn.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: false);
        if (!pw.Succeeded) return AuthResponse.Fail("Invalid password.");

        var userName = user.UserName ?? user.Email ?? dto.User;
        return AuthResponse.Ok(user.Id, userName);
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
