using OrdersApi.Models;

namespace OrdersApi.Contracts;

/// <summary>Детальная информация о заказе, включая позиции</summary>
/// <param name="Id">Идентификатор заказа</param>
/// <param name="CustomerId">Идентификатор покупателя</param>
/// <param name="Status">Текущий статус заказа</param>
/// <param name="Total">Итоговая сумма</param>
/// <param name="CreatedAt">Дата и время создания</param>
/// <param name="Lines">Позиции заказа</param>
public record OrderDetailDto(
    Guid Id,
    Guid CustomerId,
    OrderStatus Status,
    decimal Total,
    DateTimeOffset CreatedAt,
    List<OrderLineDto> Lines);
