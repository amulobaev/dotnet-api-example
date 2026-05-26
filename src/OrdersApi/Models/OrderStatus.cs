namespace OrdersApi.Models;

/// <summary>Статус заказа</summary>
public enum OrderStatus
{
    /// <summary>Ожидает подтверждения</summary>
    Pending,
    /// <summary>Подтверждён</summary>
    Confirmed,
    /// <summary>Отправлен</summary>
    Shipped,
    /// <summary>Отменён</summary>
    Cancelled
}
