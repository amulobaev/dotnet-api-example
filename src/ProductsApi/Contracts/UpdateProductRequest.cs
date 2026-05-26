using System.ComponentModel.DataAnnotations;

namespace ProductsApi.Contracts;

/// <summary>Запрос на обновление продукта (все поля опциональны)</summary>
public record UpdateProductRequest(
    [MinLength(1)] string? Name,
    string? Description,
    [Range(0, double.MaxValue)] double? Price,
    Guid? CategoryId);
