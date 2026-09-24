namespace Prodify.Infrastructure.Persistence.Seed;

public class DemoDataSettings
{
    public bool Enabled { get; set; }
    public string SellerEmail { get; set; } = "seller@prodify.local";
    public string SellerPassword { get; set; } = string.Empty;
}
