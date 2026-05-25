using Microsoft.AspNetCore.Mvc;
using ProductsApi.Contracts;
using ProductsApi.Repositories;

namespace ProductsApi.Controllers;

/// <summary>Категории продуктов</summary>
[ApiController]
[Route("api/categories")]
[Produces("application/json")]
[Tags("Categories")]
public class CategoriesController : ControllerBase
{
    private readonly IProductRepository _repository;

    /// <inheritdoc/>
    public CategoriesController(IProductRepository repository)
    {
        _repository = repository;
    }

    /// <summary>Список всех категорий</summary>
    [HttpGet]
    [ProducesResponseType<IEnumerable<CategoryDto>>(StatusCodes.Status200OK)]
    public IActionResult GetCategories()
    {
        var categories = _repository.GetAllCategories();
        return Ok(categories.Select(c => new CategoryDto(c.Id, c.Name)));
    }
}
