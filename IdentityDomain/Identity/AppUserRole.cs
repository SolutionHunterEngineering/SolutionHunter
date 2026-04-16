using Microsoft.AspNetCore.Identity;

namespace IdentityDomain;

public class AppUserRole : IdentityUserRole<int>
{
   public int ServerId { get; set; }
}
