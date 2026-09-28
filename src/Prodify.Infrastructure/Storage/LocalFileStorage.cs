using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Infrastructure.Storage;

// Keeps uploaded files in a folder next to the API; Program.cs serves that
// folder at /uploads. Good for development; use cloud storage in production.
public class LocalFileStorage : IFileStorage
{
    private readonly FileStorageSettings _settings;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<LocalFileStorage> _logger;
    private readonly string _rootPath;

    public LocalFileStorage(
        IOptions<FileStorageSettings> settings,
        IHostEnvironment environment,
        IHttpContextAccessor httpContextAccessor,
        ILogger<LocalFileStorage> logger)
    {
        _settings = settings.Value;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _rootPath = GetRootPath(_settings, environment);
    }

    public static string GetRootPath(FileStorageSettings settings, IHostEnvironment environment) =>
        Path.GetFullPath(Path.Combine(environment.ContentRootPath, settings.LocalPath));

    public async Task<string> SaveAsync(Stream content, string folder, string extension, CancellationToken cancellationToken = default)
    {
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var directory = Path.Combine(_rootPath, folder);
        Directory.CreateDirectory(directory);

        await using (var file = File.Create(Path.Combine(directory, fileName)))
        {
            await content.CopyToAsync(file, cancellationToken);
        }

        return $"{GetBaseUrl()}{_settings.RequestPath.TrimEnd('/')}/{folder}/{fileName}";
    }

    public Task DeleteAsync(string url, CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return Task.CompletedTask;

        var prefix = _settings.RequestPath.TrimEnd('/') + "/";
        if (!uri.AbsolutePath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return Task.CompletedTask;

        var relativePath = Uri.UnescapeDataString(uri.AbsolutePath[prefix.Length..]);
        var fullPath = Path.GetFullPath(Path.Combine(_rootPath, relativePath));

        // Never delete anything outside the uploads folder.
        if (!fullPath.StartsWith(_rootPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            return Task.CompletedTask;

        try
        {
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
        catch (IOException ex)
        {
            // Not worth failing the request over; the file is just left behind.
            _logger.LogWarning(ex, "Could not delete uploaded file {Path}", fullPath);
        }

        return Task.CompletedTask;
    }

    private string GetBaseUrl()
    {
        if (!string.IsNullOrWhiteSpace(_settings.PublicBaseUrl))
            return _settings.PublicBaseUrl.TrimEnd('/');

        var request = _httpContextAccessor.HttpContext?.Request
            ?? throw new InvalidOperationException("Set FileStorage:PublicBaseUrl to save files outside a web request.");

        return $"{request.Scheme}://{request.Host}{request.PathBase}";
    }
}