using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Prodify.Infrastructure.Identity;

// Email verification links last 3 days; password reset links (the default provider) only 1 hour.
public class EmailConfirmationTokenProviderOptions : DataProtectionTokenProviderOptions
{
    public const string ProviderName = "EmailConfirmation";

    public EmailConfirmationTokenProviderOptions()
    {
        Name = ProviderName;
        TokenLifespan = TimeSpan.FromDays(3);
    }
}

public class EmailConfirmationTokenProvider<TUser> : DataProtectorTokenProvider<TUser> where TUser : class
{
    public EmailConfirmationTokenProvider(
        IDataProtectionProvider dataProtectionProvider,
        IOptions<EmailConfirmationTokenProviderOptions> options,
        ILogger<DataProtectorTokenProvider<TUser>> logger)
        : base(dataProtectionProvider, options, logger)
    {
    }
}
