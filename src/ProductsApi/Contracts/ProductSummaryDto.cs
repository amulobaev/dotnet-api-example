namespace ProductsApi.Contracts;

/// <summary>Краткая информация о продукте</summary>
public record ProductSummaryDto(
    Guid Id,
    string Name,
    decimal Price,
    int StockQuantity,
    Guid CategoryId);
