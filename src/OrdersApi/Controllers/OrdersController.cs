using Microsoft.AspNetCore.Mvc;
using OrdersApi.Contracts;
using OrdersApi.Models;
using OrdersApi.Repositories;

namespace OrdersApi.Controllers;

/// <summary>Управление заказами</summary>
[ApiController]
[Route("api/orders")]
[Produces("application/json")]
[Tags("Orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderRepository _repository;

    /// <inheritdoc/>
    public OrdersController(IOrderRepository repository)
    {
        _repository = repository;
    }

    /// <summary>Список заказов</summary>
    /// <param name="status">Фильтр по статусу: Pending, Confirmed, Shipped, Cancelled</param>
    /// <param name="customerId">Фильтр по идентификатору покупателя</param>
    [HttpGet]
    [ProducesResponseType<IEnumerable<OrderSummaryDto>>(StatusCodes.Status200OK)]
    public IActionResult GetOrders([FromQuery] string? status, [FromQuery] Guid? customerId)
    {
        var orders = _repository.GetAll(status, customerId);
        return Ok(orders.Select(ToSummary));
    }

    /// <summary>Получить заказ по идентификатору</summary>
    /// <param name="id">Идентификатор заказа</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<OrderDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetOrder(Guid id)
    {
        var order = _repository.GetById(id);
        return order is null ? NotFound() : Ok(ToDetail(order));
    }

    /// <summary>Создать заказ</summary>
    /// <param name="request">Данные нового заказа</param>
    [HttpPost]
    [ProducesResponseType<OrderDetailDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public IActionResult CreateOrder([FromBody] CreateOrderRequest request)
    {
        var order = new Order
        {
            CustomerId = request.CustomerId,
            Lines = request.Lines
                .Select(l => new OrderLine { ProductId = l.ProductId, Quantity = l.Quantity, UnitPrice = l.UnitPrice })
                .ToList()
        };

        _repository.Add(order);
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, ToDetail(order));
    }

    /// <summary>Обновить статус заказа</summary>
    /// <param name="id">Идентификатор заказа</param>
    /// <param name="request">Целевой статус: Confirmed, Shipped или Cancelled</param>
    /// <remarks>
    /// Допустимые переходы: Pending→Confirmed, Confirmed→Shipped, Pending/Confirmed→Cancelled.
    /// </remarks>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType<OrderDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request)
    {
        var order = _repository.GetById(id);
        if (order is null) return NotFound();

        var newStatus = Enum.Parse<OrderStatus>(request.Status.ToString());

        if (!IsValidTransition(order.Status, newStatus))
            return Conflict(new { title = "Invalid status transition", status = 409 });

        _repository.UpdateStatus(id, newStatus);
        return Ok(ToDetail(order));
    }

    /// <summary>Отменить заказ</summary>
    /// <param name="id">Идентификатор заказа</param>
    /// <remarks>Нельзя отменить заказ в статусе Shipped или Cancelled.</remarks>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult CancelOrder(Guid id)
    {
        var order = _repository.GetById(id);
        if (order is null) return NotFound();

        if (order.Status is OrderStatus.Shipped or OrderStatus.Cancelled)
            return Conflict(new { title = "Order cannot be cancelled in its current state", status = 409 });

        _repository.UpdateStatus(id, OrderStatus.Cancelled);
        return NoContent();
    }

    // ── helpers ────────────────────────────────────────────────────────────────

    private static bool IsValidTransition(OrderStatus from, OrderStatus to) => (from, to) switch
    {
        (OrderStatus.Pending,   OrderStatus.Confirmed)  => true,
        (OrderStatus.Confirmed, OrderStatus.Shipped)    => true,
        (OrderStatus.Pending,   OrderStatus.Cancelled)  => true,
        (OrderStatus.Confirmed, OrderStatus.Cancelled)  => true,
        _ => false
    };

    private static OrderSummaryDto ToSummary(Order o) =>
        new(o.Id, o.CustomerId, o.Status, o.Total, o.CreatedAt);

    private static OrderDetailDto ToDetail(Order o) =>
        new(o.Id, o.CustomerId, o.Status, o.Total, o.CreatedAt,
            o.Lines.Select(l => new OrderLineDto(l.ProductId, l.Quantity, l.UnitPrice)).ToList());
}
