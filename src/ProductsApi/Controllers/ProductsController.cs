using Microsoft.AspNetCore.Mvc;
using ProductsApi.Contracts;
using ProductsApi.Models;
using ProductsApi.Repositories;

namespace ProductsApi.Controllers;

/// <summary>Управление продуктами</summary>
[ApiController]
[Route("products")]
[Produces("application/json")]
[Tags("Products")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _repository;

    /// <inheritdoc/>
    public ProductsController(IProductRepository repository)
    {
        _repository = repository;
    }

    /// <summary>Список продуктов</summary>
    /// <param name="categoryId">Фильтр по категории</param>
    /// <param name="inStock">true — только в наличии, false — только отсутствующие</param>
    /// <param name="search">Поиск по названию, описанию или SKU</param>
    [HttpGet]
    [ProducesResponseType<IEnumerable<ProductSummaryDto>>(StatusCodes.Status200OK)]
    public IActionResult GetProducts(
        [FromQuery] Guid? categoryId,
        [FromQuery] bool? inStock,
        [FromQuery] string? search)
    {
        var products = _repository.GetAll(categoryId, inStock, search);
        return Ok(products.Select(ToSummary));
    }

    /// <summary>Получить продукт по идентификатору</summary>
    /// <param name="id">Идентификатор продукта</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ProductDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetProduct(Guid id)
    {
        var product = _repository.GetById(id);
        return product is null ? NotFound() : Ok(ToDetail(product));
    }

    /// <summary>Добавить продукт</summary>
    /// <param name="request">Данные нового продукта</param>
    [HttpPost]
    [ProducesResponseType<ProductDetailDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public IActionResult CreateProduct([FromBody] CreateProductRequest request)
    {
        var product = new Product
        {
            Name          = request.Name,
            Description   = request.Description,
            Price         = request.Price,
            StockQuantity = request.StockQuantity,
            CategoryId    = request.CategoryId,
            Sku           = request.Sku
        };

        _repository.Add(product);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, ToDetail(product));
    }

    /// <summary>Обновить продукт</summary>
    /// <param name="id">Идентификатор продукта</param>
    /// <param name="request">Поля для обновления (null-поля игнорируются)</param>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<ProductDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult UpdateProduct(Guid id, [FromBody] UpdateProductRequest request)
    {
        var product = _repository.Update(id, p =>
        {
            if (request.Name        is not null) p.Name        = request.Name;
            if (request.Description is not null) p.Description = request.Description;
            if (request.Price       is not null) p.Price       = request.Price.Value;
            if (request.CategoryId  is not null) p.CategoryId  = request.CategoryId.Value;
        });

        return product is null ? NotFound() : Ok(ToDetail(product));
    }

    /// <summary>Обновить остаток продукта</summary>
    /// <param name="id">Идентификатор продукта</param>
    /// <param name="request">Новое абсолютное значение остатка</param>
    [HttpPut("{id:guid}/stock")]
    [ProducesResponseType<ProductDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult UpdateStock(Guid id, [FromBody] UpdateStockRequest request)
    {
        var product = _repository.Update(id, p => p.StockQuantity = request.Quantity);
        return product is null ? NotFound() : Ok(ToDetail(product));
    }

    // ── helpers ────────────────────────────────────────────────────────────────

    private static ProductSummaryDto ToSummary(Product p) =>
        new(p.Id, p.Name, p.Price, p.StockQuantity, p.CategoryId);

    private static ProductDetailDto ToDetail(Product p) =>
        new(p.Id, p.Name, p.Price, p.StockQuantity, p.CategoryId,
            p.Description, p.Sku, p.CreatedAt, p.UpdatedAt);
}
