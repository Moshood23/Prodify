using Prodify.Domain.Common;

namespace Prodify.Domain.Catalog.Entities;

public class Brand : AuditableEntity
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? LogoUrl { get; private set; }

    private Brand()
    {
    }

    private Brand(Guid id, string name, string? description, string? logoUrl) : base(id)
    {
        Name = name;
        Description = description;
        LogoUrl = logoUrl;
    }

    public static Brand Create(string name, string? description = null, string? logoUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Brand name cannot be empty.", nameof(name));

        return new Brand(Guid.NewGuid(), name.Trim(), description, logoUrl);
    }

    public void Update(string name, string? description, string? logoUrl)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Brand name cannot be empty.", nameof(name));

        Name = name.Trim();
        Description = description;
        LogoUrl = logoUrl;
    }
}