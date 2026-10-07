using System.Text.RegularExpressions;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Infrastructure.Storage;

// Keeps uploaded photos on Cloudinary, so they survive server restarts and are served
// from a CDN. Used when FileStorage:CloudinaryUrl is set; see DependencyInjection.
public class CloudinaryFileStorage : IFileStorage
{
    // Very large phone photos are scaled down on arrival; nothing on the site is shown bigger.
    private const int MaxSide = 1600;

    private readonly Cloudinary _cloudinary;
    private readonly string _rootFolder;
    private readonly ILogger<CloudinaryFileStorage> _logger;

    public CloudinaryFileStorage(IOptions<FileStorageSettings> settings, ILogger<CloudinaryFileStorage> logger)
    {
        _cloudinary = new Cloudinary(settings.Value.CloudinaryUrl) { Api = { Secure = true } };
        _rootFolder = settings.Value.CloudinaryFolder.Trim('/');
        _logger = logger;
    }

    public async Task<string> SaveAsync(Stream content, string folder, string extension, CancellationToken cancellationToken = default)
    {
        var name = Guid.NewGuid().ToString("N");
        var upload = new ImageUploadParams
        {
            File = new FileDescription(name + extension, content),
            PublicId = $"{_rootFolder}/{folder}/{name}",
            Overwrite = false,
            Transformation = new Transformation().Width(MaxSide).Height(MaxSide).Crop("limit")
        };

        var result = await _cloudinary.UploadAsync(upload, cancellationToken);
        if (result.Error is not null || result.SecureUrl is null)
            throw new InvalidOperationException($"Cloudinary upload failed: {result.Error?.Message ?? "no URL returned"}");

        // f_auto,q_auto: Cloudinary sends each phone the smallest format and quality that still looks right.
        return result.SecureUrl.ToString().Replace("/image/upload/", "/image/upload/f_auto,q_auto/");
    }

    public async Task DeleteAsync(string url, CancellationToken cancellationToken = default)
    {
        var publicId = GetPublicId(url, _cloudinary.Api.Account.Cloud);

        // Only photos this shop uploaded; links to other websites are left alone.
        if (publicId is null || !publicId.StartsWith(_rootFolder + "/", StringComparison.Ordinal))
            return;

        // Invalidate: the CDN stops serving its cached copy too.
        var result = await _cloudinary.DestroyAsync(new DeletionParams(publicId) { ResourceType = ResourceType.Image, Invalidate = true });
        if (result.Error is not null)
            // Not worth failing the request over; the photo is just left behind.
            _logger.LogWarning("Could not delete Cloudinary photo {PublicId}: {Error}", publicId, result.Error.Message);
    }

    // https://res.cloudinary.com/{cloud}/image/upload/f_auto,q_auto/v1712345678/prodify/products/abc.jpg
    // gives "prodify/products/abc". Returns null for any other URL.
    public static string? GetPublicId(string url, string cloudName)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Host != "res.cloudinary.com")
            return null;

        var prefix = $"/{cloudName}/image/upload/";
        if (!uri.AbsolutePath.StartsWith(prefix, StringComparison.Ordinal))
            return null;

        // Everything after the version segment ("v" + digits) is the public id plus extension.
        var match = Regex.Match(uri.AbsolutePath[prefix.Length..], @"(?:^|/)v\d+/(?<id>.+)$");
        if (!match.Success)
            return null;

        var id = Uri.UnescapeDataString(match.Groups["id"].Value);
        var dot = id.LastIndexOf('.');
        return dot > id.LastIndexOf('/') ? id[..dot] : id;
    }
}
