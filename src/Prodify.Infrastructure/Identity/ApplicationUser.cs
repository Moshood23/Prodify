using Microsoft.AspNetCore.Identity;

namespace Prodify.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public Guid? CustomerId { get; set; }
    public Guid? SellerId { get; set; }
}