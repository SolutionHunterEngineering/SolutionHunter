using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using Observer.Shared.Enums;

namespace Observer.Shared.Identity;

public class AppUser : IdentityUser<int>
{
    [MaxLength(255)] public string FirstName { get; set; } = "";
    [MaxLength(255)] public string LastName { get; set; } = "";
    [MaxLength(255)] public string KnownAs { get; set; } = "";
    [MaxLength(255)] public string Organization { get; set; } = "";

    [Phone, MaxLength(255)] public override string? PhoneNumber { get; set; }
    [Phone, MaxLength(255)] public string AltPhone { get; set; } = "";

    [MaxLength(255)] public string City { get; set; } = "";
    [MaxLength(255)] public string State { get; set; } = "";
    [MaxLength(255)] public string Country { get; set; } = "";
    [MaxLength(255)] public string ZipCode { get; set; } = "";

    [MaxLength(255)] public string Question { get; set; } = "";
    [MaxLength(255)] public string Answer { get; set; } = "";

    public Observer.Shared.Enums.UserType UserType { get; set; } = Observer.Shared.Enums.UserType.None;

    public ICollection<AppUserRole> UserRoles { get; set; } = new List<AppUserRole>();
}
