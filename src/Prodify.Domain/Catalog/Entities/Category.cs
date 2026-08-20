using Prodify.Domain.Common;

namespace Prodify.Domain.Catalog.Entities;

public class Category : AuditableEntity
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public Guid? ParentCategoryId { get; private set; }

    private Category()
    {
    }

    private Category(Guid id, string name, string? description, Guid? parentCategoryId) : base(id)
    {
        Name = name;
        Description = description;
        ParentCategoryId = parentCategoryId;
    }

    public static Category Create(string name, string? description = null, Guid? parentCategoryId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name cannot be empty.", nameof(name));

        return new Category(Guid.NewGuid(), name.Trim(), description, parentCategoryId);
    }

    public void Update(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name cannot be empty.", nameof(name));

        Name = name.Trim();
        Description = description;
    }

    public void MoveToParent(Guid? parentCategoryId)
    {
        if (parentCategoryId == Id)
            throw new InvalidOperationException("A category cannot be its own parent.");

        ParentCategoryId = parentCategoryId;
    }
}