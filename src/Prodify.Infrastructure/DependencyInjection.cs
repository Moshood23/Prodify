using System.Text;
using FluentValidation;
using MediatR;
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
using Prodify.Infrastructure.Persistence.Seed;

namespace Prodify.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<DomainEventInterceptor>();
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddDbContext<ProdifyDbContext>((sp, options) =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            options.AddInterceptors(
                sp.GetRequiredService<DomainEventInterceptor>(),
                sp.GetRequiredService<AuditableEntityInterceptor>());
        });

        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
        {
            options.Password.RequiredLength = 8;
            options.User.RequireUniqueEmail = true;
        })
            .AddEntityFrameworkStores<ProdifyDbContext>()
            .AddDefaultTokenProviders();

        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.AddSingleton<JwtService>();

        services.Configure<SeedAdminSettings>(configuration.GetSection("SeedAdmin"));
        services.AddScoped<IdentitySeeder>();

        var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
                        .AddJwtBearer(options =>
                        {
                            // Keep claim names exactly as written in the token ("sub", "role", "customerId").
                            options.MapInboundClaims = false;

                            options.TokenValidationParameters = new TokenValidationParameters
                            {
                                NameClaimType = "sub",
                                RoleClaimType = "role",
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
        services.AddScoped<IIdentityService, IdentityService>();

        services.AddHostedService<OutboxProcessorHostedService>();
        services.AddHostedService<ReservationExpirationService>();
        services.AddHostedService<PaymentRetryService>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Prodify.Application.AssemblyMarker).Assembly));

        services.AddValidatorsFromAssembly(typeof(Prodify.Application.AssemblyMarker).Assembly);

        services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(Prodify.Application.Common.Behaviors.LoggingBehavior<,>));
        services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(Prodify.Application.Common.Behaviors.ValidationBehavior<,>));
        services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(Prodify.Application.Common.Behaviors.TransactionBehavior<,>));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ProdifyDbContext>());
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}