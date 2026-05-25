using OrdersApi.Models;

namespace OrdersApi.Contracts;

/// <summary>Краткая информация о заказе</summary>
/// <param name="Id">Идентификатор заказа</param>
/// <param name="CustomerId">Идентификатор покупателя</param>
/// <param name="Status">Текущий статус заказа</param>
/// <param name="Total">Итоговая сумма</param>
/// <param name="CreatedAt">Дата и время создания</param>
public record OrderSummaryDto(
    Guid Id,
    Guid CustomerId,
    OrderStatus Status,
    decimal Total,
    DateTimeOffset CreatedAt);
