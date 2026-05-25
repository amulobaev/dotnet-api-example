namespace OrdersApi.Models;

/// <summary>Позиция заказа (доменная модель)</summary>
public class OrderLine
{
    /// <summary>Идентификатор товара</summary>
    public Guid ProductId { get; init; }

    /// <summary>Количество единиц (минимум 1)</summary>
    public int Quantity { get; init; }

    /// <summary>Цена за единицу на момент создания заказа</summary>
    public double UnitPrice { get; init; }
}
