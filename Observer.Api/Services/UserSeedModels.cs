using System.Collections.Generic;

namespace Observer.Api.Services.Auth;

// Root object for the JSON file
public sealed class ServerSeedData
{
    public List<ServerSeedRecord> Servers { get; set; } = new();
}

public sealed class ServerSeedRecord
{
    public string Name { get; set; } = string.Empty;

    // Global roles available on this server (Admin, Engineer, Viewer, etc.)
    public List<string> Roles { get; set; } = new();

    // Users to seed for this server
    public List<UserSeedRecord> Users { get; set; } = new();
}

public sealed class UserSeedRecord
{
    public string UserName { get; set; } = string.Empty;
    public string SeedPassword { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();

    public string Organization { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string KnownAs { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string AltPhone { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
}
