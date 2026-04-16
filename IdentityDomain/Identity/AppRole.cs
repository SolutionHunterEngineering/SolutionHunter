using Microsoft.AspNetCore.Identity;

namespace IdentityDomain;

public class AppRole : IdentityRole<int>
{
    public int ServerId { get; set; } 
}
