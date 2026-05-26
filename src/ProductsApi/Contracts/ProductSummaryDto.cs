namespace ProductsApi.Contracts;

/// <summary>Краткая информация о продукте</summary>
public record ProductSummaryDto(
    Guid Id,
    string Name,
    double Price,
    int StockQuantity,
    Guid CategoryId);
