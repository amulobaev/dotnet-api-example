using System.ComponentModel.DataAnnotations;

namespace OrdersApi.Contracts;

/// <summary>Запрос на создание заказа</summary>
/// <param name="CustomerId">Идентификатор покупателя</param>
/// <param name="Lines">Позиции заказа (минимум одна)</param>
public record CreateOrderRequest(Guid CustomerId, [MinLength(1)] List<OrderLineDto> Lines);
