namespace Prodify.Application.Common.Security;

public static class Roles
{
    public const string Customer = "Customer";
    public const string Seller = "Seller";
    public const string Admin = "Admin";

    public const string SellerOrAdmin = Seller + "," + Admin;

    public static readonly IReadOnlyList<string> All = new[] { Customer, Seller, Admin };
}