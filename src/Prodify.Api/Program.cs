using Microsoft.EntityFrameworkCore;
using Prodify.Infrastructure.Persistence;
using Prodify.Infrastructure;
using Prodify.Infrastructure.Persistence.Seed;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Prodify.Infrastructure.Storage;
using Microsoft.AspNetCore.HttpOverrides;
using Prodify.Api.Security;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "Enter 'Bearer' followed by your JWT token. Example: Bearer eyJhbGciOi...",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks()
    .AddCheck<Prodify.Api.HealthChecks.DatabaseHealthCheck>("database");

// The websites allowed to call the API, from settings (Cors:AllowedOrigins).
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddProdifyRateLimiting(builder.Configuration);

// JSON requests are small; the photo upload endpoint allows more with its own [RequestSizeLimit].
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 1024 * 1024);

// Behind a hosting provider's proxy, the visitor's real IP and https arrive in X-Forwarded-* headers.
// Only switch this on when the API can be reached through that proxy alone.
var behindProxy = builder.Configuration.GetValue("ReverseProxy:Enabled", false);
if (behindProxy)
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });
}

ProductionSettings.Check(builder.Configuration, builder.Environment);
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    // Development always; on a live server when Database:MigrateOnStartup is true, so each
    // deploy brings the database up to date before the API starts taking requests.
    if (app.Environment.IsDevelopment() || app.Configuration.GetValue("Database:MigrateOnStartup", false))
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ProdifyDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    var identitySeeder = scope.ServiceProvider.GetRequiredService<IdentitySeeder>();
    await identitySeeder.SeedAsync();

    var deliveryFeeSeeder = scope.ServiceProvider.GetRequiredService<DeliveryFeeSeeder>();
    await deliveryFeeSeeder.SeedAsync();

    var demoCatalogSeeder = scope.ServiceProvider.GetRequiredService<DemoCatalogSeeder>();
    await demoCatalogSeeder.SeedAsync();
    var demoReviewSeeder = scope.ServiceProvider.GetRequiredService<DemoReviewSeeder>();
    await demoReviewSeeder.SeedAsync();
}

if (behindProxy)
    app.UseForwardedHeaders();

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<Prodify.Api.Middleware.CorrelationIdMiddleware>();
app.UseMiddleware<Prodify.Api.Middleware.ExceptionHandlingMiddleware>();

app.UseCors("AllowFrontend");

// Serves uploaded product photos, e.g. /uploads/products/abc.jpg.
var fileStorage = app.Services.GetRequiredService<IOptions<FileStorageSettings>>().Value;
var uploadsPath = LocalFileStorage.GetRootPath(fileStorage, app.Environment);
Directory.CreateDirectory(uploadsPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = fileStorage.RequestPath,
    // File names are random and never reused, so browsers can keep them.
    OnPrepareResponse = ctx => ctx.Context.Response.Headers.CacheControl = "public,max-age=31536000,immutable"
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
    app.UseHsts();

app.UseHttpsRedirection();

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();