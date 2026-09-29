using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Common.Emails;

public static class AccountEmails
{
    public static EmailContent Welcome(string firstName, IAppUrls urls) =>
        EmailLayout.Build(
            "Welcome to Prodify",
            $"Welcome, {firstName}!",
            new[]
            {
                "Your Prodify account is ready. Shop phones, fashion, home appliances and more from sellers across Nigeria.",
                "Pay by card or pay on delivery, and track every order from your account.",
            },
            ("Start shopping", urls.Page("/")));

    public static EmailContent PasswordReset(string? firstName, string resetUrl) =>
        EmailLayout.Build(
            "Reset your Prodify password",
            "Reset your password",
            new[]
            {
                $"Hi {firstName ?? "there"},",
                "We received a request to reset your Prodify password. Click the button below to choose a new one. The link works once and expires in 1 hour.",
            },
            ("Choose a new password", resetUrl),
            "If you didn't ask for this, you can ignore this email. Your password won't change.");

    public static EmailContent PasswordChanged(string? firstName, IAppUrls urls) =>
        EmailLayout.Build(
            "Your Prodify password was changed",
            "Your password was changed",
            new[]
            {
                $"Hi {firstName ?? "there"},",
                "The password for your Prodify account was just changed, and you were logged out on your other devices.",
                "If this wasn't you, reset your password straight away and contact Prodify support.",
            },
            ("Reset password", urls.Page("/forgot-password")));
}
