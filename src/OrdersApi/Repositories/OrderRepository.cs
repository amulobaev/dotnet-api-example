using System.Collections.Concurrent;
using OrdersApi.Models;

namespace OrdersApi.Repositories;

/// <inheritdoc/>
public class OrderRepository : IOrderRepository
{
    private readonly ConcurrentDictionary<Guid, Order> _store = new();

    /// <summary>Инициализирует репозиторий с тестовыми данными</summary>
    public OrderRepository() => Seed();

    /// <inheritdoc/>
    public IEnumerable<Order> GetAll(string? status, Guid? customerId)
    {
        var query = _store.Values.AsEnumerable();

        if (status is not null && Enum.TryParse<OrderStatus>(status, out var parsedStatus))
            query = query.Where(o => o.Status == parsedStatus);

        if (customerId.HasValue)
            query = query.Where(o => o.CustomerId == customerId.Value);

        return query.OrderByDescending(o => o.CreatedAt);
    }

    /// <inheritdoc/>
    public Order? GetById(Guid id) => _store.GetValueOrDefault(id);

    /// <inheritdoc/>
    public Order Add(Order order)
    {
        _store[order.Id] = order;
        return order;
    }

    /// <inheritdoc/>
    public void UpdateStatus(Guid id, OrderStatus newStatus)
    {
        if (_store.TryGetValue(id, out var order))
            order.Status = newStatus;
    }

    // ── seed ──────────────────────────────────────────────────────────────────

    private void Seed()
    {
        var customer1 = new Guid("11111111-0000-0000-0000-000000000001");
        var customer2 = new Guid("11111111-0000-0000-0000-000000000002");

        var product1 = new Guid("22222222-0000-0000-0000-000000000001");
        var product2 = new Guid("22222222-0000-0000-0000-000000000002");
        var product3 = new Guid("22222222-0000-0000-0000-000000000003");

        var orders = new[]
        {
            new Order
            {
                Id = new Guid("aaaaaaaa-0000-0000-0000-000000000001"),
                CustomerId = customer1,
                Status = OrderStatus.Pending,
                CreatedAt = DateTimeOffset.UtcNow.AddHours(-3),
                Lines =
                [
                    new OrderLine { ProductId = product1, Quantity = 2, UnitPrice = 499.99 },
                    new OrderLine { ProductId = product2, Quantity = 1, UnitPrice = 1299 }
                ]
            },
            new Order
            {
                Id = new Guid("aaaaaaaa-0000-0000-0000-000000000002"),
                CustomerId = customer1,
                Status = OrderStatus.Confirmed,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
                Lines =
                [
                    new OrderLine { ProductId = product3, Quantity = 3, UnitPrice = 249.50 }
                ]
            },
            new Order
            {
                Id = new Guid("aaaaaaaa-0000-0000-0000-000000000003"),
                CustomerId = customer2,
                Status = OrderStatus.Shipped,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-5),
                Lines =
                [
                    new OrderLine { ProductId = product1, Quantity = 1, UnitPrice = 499.99 },
                    new OrderLine { ProductId = product3, Quantity = 2, UnitPrice = 249.50 }
                ]
            },
            new Order
            {
                Id = new Guid("aaaaaaaa-0000-0000-0000-000000000004"),
                CustomerId = customer2,
                Status = OrderStatus.Cancelled,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-10),
                Lines =
                [
                    new OrderLine { ProductId = product2, Quantity = 1, UnitPrice = 1299 }
                ]
            }
        };

        foreach (var order in orders)
            _store[order.Id] = order;
    }
}
