using System.ComponentModel.DataAnnotations;

namespace ProductsApi.Contracts;

/// <summary>Запрос на создание продукта</summary>
public record CreateProductRequest(
    [Required, MinLength(1)] string Name,
    string? Description,
    [Range(0, double.MaxValue)] double Price,
    [Range(0, int.MaxValue)] int StockQuantity,
    Guid CategoryId,
    string? Sku);
