namespace ProductsApi.Models;

/// <summary>Категория товаров.</summary>
public class Category
{
    /// <summary>Уникальный идентификатор категории.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Название категории.</summary>
    public required string Name { get; set; }
}
