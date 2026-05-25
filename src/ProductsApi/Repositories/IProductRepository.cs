using ProductsApi.Models;

namespace ProductsApi.Repositories;

/// <summary>Хранилище продуктов и категорий</summary>
public interface IProductRepository
{
    /// <summary>Список продуктов с опциональными фильтрами</summary>
    IEnumerable<Product> GetAll(Guid? categoryId, bool? inStock, string? search);

    /// <summary>Найти продукт по идентификатору</summary>
    Product? GetById(Guid id);

    /// <summary>Добавить продукт</summary>
    Product Add(Product product);

    /// <summary>Обновить поля продукта</summary>
    Product? Update(Guid id, Action<Product> applyChanges);

    /// <summary>Список всех категорий</summary>
    IEnumerable<Category> GetAllCategories();
}
