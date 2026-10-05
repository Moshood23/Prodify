using MediatR;

namespace Prodify.Application.Admin.Settings.Queries.GetPlatformSettings;

public class GetPlatformSettingsQuery : IRequest<PlatformSettingsDto>
{
}

public class PlatformSettingsDto
{
    public decimal CommissionRate { get; set; }
}
