namespace OrdersApi.Models;

/// <summary>Заказ (доменная модель)</summary>
public class Order
{
    /// <summary>Идентификатор заказа</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Идентификатор покупателя</summary>
    public Guid CustomerId { get; init; }

    /// <summary>Текущий статус заказа</summary>
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    /// <summary>Позиции заказа</summary>
    public List<OrderLine> Lines { get; init; } = [];

    /// <summary>Итоговая сумма — вычисляется из позиций</summary>
    public decimal Total => Lines.Sum(l => l.Quantity * l.UnitPrice);

    /// <summary>Дата и время создания</summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
