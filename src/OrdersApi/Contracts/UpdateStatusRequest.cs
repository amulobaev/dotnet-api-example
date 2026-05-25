namespace OrdersApi.Contracts;

/// <summary>Запрос на обновление статуса заказа</summary>
/// <param name="Status">Целевой статус</param>
public record UpdateStatusRequest(TargetOrderStatus Status);
