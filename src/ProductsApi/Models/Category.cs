namespace ProductsApi.Models;

public class Category
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public required string Name { get; set; }
}
