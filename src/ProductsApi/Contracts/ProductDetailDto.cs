namespace ProductsApi.Contracts;

/// <summary>Подробная информация о продукте</summary>
public record ProductDetailDto(
    Guid Id,
    string Name,
    decimal Price,
    int StockQuantity,
    Guid CategoryId,
    string? Description,
    string? Sku,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
