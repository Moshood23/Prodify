using Microsoft.Extensions.Configuration;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Infrastructure;

// Website links for emails. Set App:FrontendUrl to the real address when the site goes live.
public class AppUrls : IAppUrls
{
    private readonly string _frontendUrl;

    public AppUrls(IConfiguration configuration)
    {
        _frontendUrl = (configuration["App:FrontendUrl"] ?? "http://localhost:5173").TrimEnd('/');
    }

    public string Page(string path) => $"{_frontendUrl}/{path.TrimStart('/')}";
}
