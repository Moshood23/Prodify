namespace Prodify.Api.Security;

// Stops the live API from starting with the development secrets in appsettings.json.
public static class ProductionSettings
{
    private const string DevelopmentJwtKey = "vcUB3tFzZjWXuAaN+oV1w7hWMfDXhBBMMf8UY0aXJek=";

    public static void Check(IConfiguration configuration, IHostEnvironment environment)
    {
        if (environment.IsDevelopment())
            return;

        var problems = new List<string>();

        var jwtKey = configuration["Jwt:SecretKey"] ?? "";
        if (jwtKey == DevelopmentJwtKey || jwtKey.Length < 32)
            problems.Add("Jwt__SecretKey must be set to a new random key of at least 32 characters.");

        var connection = configuration.GetConnectionString("DefaultConnection") ?? "";
        if (connection.Contains("Prodify_Dev_2026!", StringComparison.Ordinal))
            problems.Add("ConnectionStrings__DefaultConnection must point at the live database, not the development one.");

        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        if (origins.Length == 0 || origins.Any(o => o.Contains("localhost", StringComparison.OrdinalIgnoreCase)))
            problems.Add("Cors__AllowedOrigins__0 must be the live website address, e.g. https://prodify.ng.");

        // Most hosts wipe the server's own disk on every deploy, which would lose every product photo.
        if (string.IsNullOrWhiteSpace(configuration["FileStorage:CloudinaryUrl"]))
            problems.Add("FileStorage__CloudinaryUrl must be set (cloudinary://API_KEY:API_SECRET@CLOUD_NAME), so photos are kept on Cloudinary.");

        if (problems.Count > 0)
            throw new InvalidOperationException(
                "The API is not configured for production:" + Environment.NewLine + string.Join(Environment.NewLine, problems));
    }
}
