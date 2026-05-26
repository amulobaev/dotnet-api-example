using System.Collections.Concurrent;
using ProductsApi.Models;

namespace ProductsApi.Repositories;

/// <inheritdoc/>
public class ProductRepository : IProductRepository
{
    private readonly ConcurrentDictionary<Guid, Product> _products = new();
    private readonly ConcurrentDictionary<Guid, Category> _categories = new();

    /// <summary>Инициализирует репозиторий с тестовыми данными</summary>
    public ProductRepository() => Seed();

    /// <inheritdoc/>
    public IEnumerable<Product> GetAll(Guid? categoryId, bool? inStock, string? search)
    {
        var query = _products.Values.AsEnumerable();

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (inStock.HasValue)
            query = inStock.Value
                ? query.Where(p => p.StockQuantity > 0)
                : query.Where(p => p.StockQuantity == 0);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p =>
                p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (p.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (p.Sku?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));

        return query.OrderBy(p => p.Name);
    }

    /// <inheritdoc/>
    public Product? GetById(Guid id) => _products.GetValueOrDefault(id);

    /// <inheritdoc/>
    public Product Add(Product product)
    {
        _products[product.Id] = product;
        return product;
    }

    /// <inheritdoc/>
    public Product? Update(Guid id, Action<Product> applyChanges)
    {
        if (!_products.TryGetValue(id, out var product))
            return null;

        applyChanges(product);
        product.UpdatedAt = DateTimeOffset.UtcNow;
        return product;
    }

    /// <inheritdoc/>
    public IEnumerable<Category> GetAllCategories() =>
        _categories.Values.OrderBy(c => c.Name);

    // ── seed ──────────────────────────────────────────────────────────────────

    private void Seed()
    {
        var electronics = new Category
        {
            Id = new Guid("cccccccc-0000-0000-0000-000000000001"),
            Name = "Электроника"
        };
        var accessories = new Category
        {
            Id = new Guid("cccccccc-0000-0000-0000-000000000002"),
            Name = "Аксессуары"
        };

        _categories[electronics.Id] = electronics;
        _categories[accessories.Id] = accessories;

        var products = new[]
        {
            new Product
            {
                Id = new Guid("dddddddd-0000-0000-0000-000000000001"),
                Name = "Ноутбук UltraBook 15",
                Description = "Тонкий ноутбук с процессором Intel Core i7, 16 ГБ RAM, SSD 512 ГБ",
                Price = 89_999,
                StockQuantity = 15,
                CategoryId = electronics.Id,
                Sku = "LAPTOP-UB15",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-30),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-5)
            },
            new Product
            {
                Id = new Guid("dddddddd-0000-0000-0000-000000000002"),
                Name = "Монитор 27\" 4K",
                Description = "IPS-матрица, 3840×2160, 60 Гц, USB-C",
                Price = 34_999,
                StockQuantity = 8,
                CategoryId = electronics.Id,
                Sku = "MON-27-4K",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-60),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-10)
            },
            new Product
            {
                Id = new Guid("dddddddd-0000-0000-0000-000000000003"),
                Name = "Беспроводная мышь Pro",
                Description = "Эргономичная мышь, Bluetooth 5.0, до 70 часов без подзарядки",
                Price = 2_499,
                StockQuantity = 50,
                CategoryId = accessories.Id,
                Sku = "MOUSE-WL-PRO",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-90),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-2)
            },
            new Product
            {
                Id = new Guid("dddddddd-0000-0000-0000-000000000004"),
                Name = "USB-C хаб 7-в-1",
                Description = "HDMI 4K, 3× USB-A, USB-C PD 100W, SD/microSD",
                Price = 3_999,
                StockQuantity = 30,
                CategoryId = accessories.Id,
                Sku = "HUB-UC7",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-45),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-3)
            },
            new Product
            {
                Id = new Guid("dddddddd-0000-0000-0000-000000000005"),
                Name = "Механическая клавиатура TKL",
                Description = "Tenkeyless, переключатели Cherry MX Red, RGB-подсветка",
                Price = 8_999,
                StockQuantity = 0,
                CategoryId = accessories.Id,
                Sku = "KB-MECH-TKL",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-20),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1)
            }
        };

        foreach (var product in products)
            _products[product.Id] = product;
    }
}
