using MediatR;

namespace Prodify.Application.Admin.Settings.Commands.UpdatePlatformSettings;

// Changing the commission only affects orders delivered from now on.
public class UpdatePlatformSettingsCommand : IRequest
{
    public decimal CommissionRate { get; set; }
}
