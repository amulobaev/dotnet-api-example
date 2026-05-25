namespace OrdersApi.Contracts;

/// <summary>Позиция заказа</summary>
/// <param name="ProductId">Идентификатор товара</param>
/// <param name="Quantity">Количество (минимум 1)</param>
/// <param name="UnitPrice">Цена за единицу</param>
public record OrderLineDto(Guid ProductId, int Quantity, double UnitPrice);
