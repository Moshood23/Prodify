using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Sellers.Entities;

namespace Prodify.Application.Common.Emails;

// Emails to sellers about their application and account.
public static class SellerEmails
{
    public static EmailContent Approved(Seller seller, IAppUrls urls) =>
        EmailLayout.Build(
            "Your Prodify store is approved",
            $"{seller.BusinessName} is approved!",
            new[]
            {
                "Good news: your seller application has been approved and your store is open.",
                "Add your products with photos, prices and stock in Seller Centre. They appear in the shop as soon as they have stock.",
            },
            ("Open Seller Centre", urls.Page("/seller")));

    public static EmailContent Rejected(Seller seller, string reason, IAppUrls urls) =>
        EmailLayout.Build(
            "Your Prodify seller application",
            "Your application wasn't approved",
            new[]
            {
                $"Thank you for applying to sell on Prodify as {seller.BusinessName}. Unfortunately we couldn't approve your application this time.",
                $"Reason: {reason}",
                "You can update your details and apply again.",
            },
            ("Update and reapply", urls.Page("/sell")));

    public static EmailContent Suspended(Seller seller, string reason) =>
        EmailLayout.Build(
            "Your Prodify store has been suspended",
            $"{seller.BusinessName} is suspended",
            new[]
            {
                "Your store has been suspended, so your products are hidden from the shop and you can't receive new orders.",
                $"Reason: {reason}",
                "If you think this is a mistake, reply to this email or contact Prodify support.",
            });

    public static EmailContent Reinstated(Seller seller, IAppUrls urls) =>
        EmailLayout.Build(
            "Your Prodify store is open again",
            $"{seller.BusinessName} is open again",
            new[] { "Your store has been reinstated. Your products are back in the shop and you can receive orders again." },
            ("Open Seller Centre", urls.Page("/seller")));
}
