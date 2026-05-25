namespace OrdersApi.Contracts;

/// <summary>Допустимые целевые статусы при ручном переходе (Pending недостижим вручную)</summary>
public enum TargetOrderStatus
{
    /// <summary>Подтверждён</summary>
    Confirmed,
    /// <summary>Отправлен</summary>
    Shipped,
    /// <summary>Отменён</summary>
    Cancelled
}
