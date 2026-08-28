using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Prodify.Application.Common.Interfaces;
using Prodify.Infrastructure.BackgroundJobs;
using Prodify.Infrastructure.Identity;
using Prodify.Infrastructure.Messaging.InProcess;
using Prodify.Infrastructure.Messaging.Outbox;
using Prodify.Infrastructure.Notifications;
using Prodify.Infrastructure.Payments;
using Prodify.Infrastructure.Persistence;
using Prodify.Infrastructure.Persistence.Interceptors;
using System.Text;
using MediatR;

namespace Prodify.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<DomainEventInterceptor>();

        services.AddDbContext<ProdifyDbContext>((sp, options) =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            options.AddInterceptors(sp.GetRequiredService<DomainEventInterceptor>());
        });

        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
        {
            options.Password.RequiredLength = 8;
        })
            .AddEntityFrameworkStores<ProdifyDbContext>()
            .AddDefaultTokenProviders();

        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.AddSingleton<JwtService>();

        var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings?.Issuer,
                    ValidAudience = jwtSettings?.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings?.SecretKey ?? string.Empty))
                };
            });

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddScoped<IMessagePublisher, MediatRMessagePublisher>();
        services.AddScoped<OutboxProcessor>();

        services.AddScoped<IPaymentService, SimulatedPaymentGateway>();
        services.AddScoped<INotificationService, LogNotificationService>();

        services.AddHostedService<OutboxProcessorHostedService>();
        services.AddHostedService<ReservationExpirationService>();
        services.AddHostedService<PaymentRetryService>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Prodify.Application.AssemblyMarker).Assembly));
        services.AddSingleton<DomainEventInterceptor>();

        return services;
    }
}