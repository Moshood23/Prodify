using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Payouts.Entities;

namespace Prodify.Application.Admin.Settings.Commands.UpdatePlatformSettings;

public class UpdatePlatformSettingsCommandHandler : IRequestHandler<UpdatePlatformSettingsCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdatePlatformSettingsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdatePlatformSettingsCommand request, CancellationToken cancellationToken)
    {
        var settings = await _context.PlatformSettings.FirstOrDefaultAsync(cancellationToken);
        if (settings is null)
        {
            settings = PlatformSettings.CreateDefault();
            _context.Add(settings);
        }

        settings.SetCommissionRate(request.CommissionRate);
    }
}
