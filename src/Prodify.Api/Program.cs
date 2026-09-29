using Microsoft.EntityFrameworkCore;
using Prodify.Infrastructure.Persistence;
using Prodify.Infrastructure;
using Prodify.Infrastructure.Persistence.Seed;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Prodify.Infrastructure.Storage;

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    if (app.Environment.IsDevelopment())
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
}
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

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();