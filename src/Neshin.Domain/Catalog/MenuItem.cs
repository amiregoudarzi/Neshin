using Neshin.Domain.Common;

namespace Neshin.Domain.Catalog;

public sealed class MenuItem : Entity
{
    private MenuItem(
        Guid id,
        Guid menuId,
        string name,
        decimal price,
        string? categoryName,
        int displayOrder,
        DateTime createdAtUtc) : base(id)
    {
        MenuId = menuId;
        Name = name;
        Price = price;
        CategoryName = categoryName;
        DisplayOrder = displayOrder;
        IsAvailable = true;
        CreatedAtUtc = createdAtUtc;
    }

    private MenuItem() : base(Guid.Empty) { }

    public Guid MenuId { get; private init; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? CategoryName { get; private set; }
    public string? ImageUrl { get; private set; }
    public decimal Price { get; private set; }
    public bool IsAvailable { get; private set; }
    public int DisplayOrder { get; private set; }
    public DateTime CreatedAtUtc { get; private init; }

    public static MenuItem Create(
        Guid menuId,
        string name,
        decimal price,
        DateTime createdAtUtc,
        string? categoryName = null,
        int displayOrder = 0)
    {
        if (menuId == Guid.Empty) throw new DomainException("Menu is required.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Menu item name is required.");
        if (price < 0) throw new DomainException("Menu item price cannot be negative.");

        return new MenuItem(
            Guid.NewGuid(),
            menuId,
            name.Trim(),
            price,
            Normalize(categoryName, 100),
            displayOrder,
            createdAtUtc);
    }

    public void Update(
        string name,
        string? description,
        string? categoryName,
        string? imageUrl,
        decimal price,
        bool isAvailable,
        int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Menu item name is required.");
        if (price < 0) throw new DomainException("Menu item price cannot be negative.");

        Name = name.Trim();
        Description = Normalize(description, 1000);
        CategoryName = Normalize(categoryName, 100);
        ImageUrl = Normalize(imageUrl, 2000);
        Price = price;
        IsAvailable = isAvailable;
        DisplayOrder = displayOrder;
    }

    private static string? Normalize(string? value, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var normalized = value.Trim();
        if (normalized.Length > maximumLength)
            throw new DomainException($"The value cannot exceed {maximumLength} characters.");
        return normalized;
    }
}
