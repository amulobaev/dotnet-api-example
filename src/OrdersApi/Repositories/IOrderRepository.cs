using OrdersApi.Models;

namespace OrdersApi.Repositories;

/// <summary>Хранилище заказов</summary>
public interface IOrderRepository
{
    /// <summary>Возвращает все заказы с опциональной фильтрацией</summary>
    IEnumerable<Order> GetAll(string? status, Guid? customerId);

    /// <summary>Возвращает заказ по идентификатору или <c>null</c></summary>
    Order? GetById(Guid id);

    /// <summary>Добавляет заказ в хранилище</summary>
    Order Add(Order order);

    /// <summary>Обновляет статус существующего заказа</summary>
    void UpdateStatus(Guid id, OrderStatus newStatus);
}
