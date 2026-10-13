using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Common.Emails;

public static class AccountEmails
{
    // Sent on sign-up, with the link that verifies the email address.
    public static EmailContent Welcome(string firstName, string verifyUrl) =>
        EmailLayout.Build(
            "Welcome to Prodify: please verify your email",
            $"Welcome, {firstName}!",
            new[]
            {
                "Your Prodify account is ready. Shop phones, fashion, home appliances and more from sellers across Nigeria.",
                "Please confirm this is your email address. You need to before you can place an order.",
            },
            ("Verify my email", verifyUrl),
            "The link works for 3 days. If you didn't create a Prodify account, you can ignore this email.");

    // "Send the link again".
    public static EmailContent VerifyEmail(string? firstName, string verifyUrl) =>
        EmailLayout.Build(
            "Verify your email for Prodify",
            "Verify your email",
            new[]
            {
                $"Hi {firstName ?? "there"},",
                "Click the button below to confirm this is your email address. You need to before you can place an order.",
            },
            ("Verify my email", verifyUrl),
            "The link works for 3 days. If you didn't ask for this, you can ignore this email.");

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
