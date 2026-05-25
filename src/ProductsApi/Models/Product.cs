namespace ProductsApi.Models;

public class Product
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public required string Name { get; set; }
    
    public string? Description { get; set; }
    
    public decimal Price { get; set; }
    
    public int StockQuantity { get; set; }
    
    public Guid CategoryId { get; set; }
    
    public string? Sku { get; set; }
    
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
