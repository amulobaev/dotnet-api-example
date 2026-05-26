namespace ProductsApi.Models;

/// <summary>Товар в каталоге.</summary>
public class Product
{
    /// <summary>Уникальный идентификатор товара.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Название товара.</summary>
    public required string Name { get; set; }

    /// <summary>Описание товара.</summary>
    public string? Description { get; set; }

    /// <summary>Цена товара.</summary>
    public double Price { get; set; }

    /// <summary>Количество единиц на складе.</summary>
    public int StockQuantity { get; set; }

    /// <summary>Идентификатор категории, к которой относится товар.</summary>
    public Guid CategoryId { get; set; }

    /// <summary>Артикул (Stock Keeping Unit).</summary>
    public string? Sku { get; set; }

    /// <summary>Дата и время создания записи (UTC).</summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>Дата и время последнего обновления записи (UTC).</summary>
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
