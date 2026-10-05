using MediatR;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Payouts.Common;

namespace Prodify.Application.Admin.Settings.Queries.GetPlatformSettings;

public class GetPlatformSettingsQueryHandler : IRequestHandler<GetPlatformSettingsQuery, PlatformSettingsDto>
{
    private readonly IApplicationDbContext _context;

    public GetPlatformSettingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PlatformSettingsDto> Handle(GetPlatformSettingsQuery request, CancellationToken cancellationToken) =>
        new() { CommissionRate = await _context.GetCommissionRateAsync(cancellationToken) };
}
