using Prodify.Domain.Common;

namespace Prodify.Domain.Inventory.Entities;

public class Warehouse : AuditableEntity
{
    public string Name { get; private set; } = null!;
    public string Code { get; private set; } = null!;
    public string? Address { get; private set; }
    public bool? IsActive { get; private set; }

    private Warehouse()
    {
    }

    private Warehouse(Guid id, string name, string code, string? address) : base(id)
    {
        Name = name;
        Code = code;
        Address = address;
        IsActive = true;
    }

    public static Warehouse Create(string name, string code, string? address = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Warehouse name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Warehouse code cannot be empty.", nameof(code));

        return new Warehouse(Guid.NewGuid(), name.Trim(), code.Trim().ToUpperInvariant(), address);
    }

    public void Update(string name, string? address)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Warehouse name cannot be empty.", nameof(name));

        Name = name.Trim();
        Address = address;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}