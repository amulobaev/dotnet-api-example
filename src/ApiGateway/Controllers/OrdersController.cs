using Microsoft.AspNetCore.Mvc;
using ApiGateway.Contracts.Orders;
using Refit;
using System.Net;

namespace ApiGateway.Controllers;

/// <summary>Публичный контракт Gateway для заказов. Делегирует в OrdersApi через Refit.</summary>
[ApiController]
[Route("api/orders")]
[Produces("application/json")]
[Tags("Orders (via Refit)")]
public class OrdersController : ControllerBase
{
    private readonly IOrdersApi _ordersApi;

    /// <inheritdoc/>
    public OrdersController(IOrdersApi ordersApi)
    {
        _ordersApi = ordersApi;
    }

    /// <summary>Список заказов</summary>
    [HttpGet]
    [ProducesResponseType<ICollection<OrderSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrders(
        [FromQuery] string? status,
        [FromQuery] Guid? customerId)
    {
        // Refit omits null query params — null-forgiving is safe here
        var orders = await _ordersApi.GetOrders(status!, customerId);
        return Ok(orders);
    }

    /// <summary>Создать заказ</summary>
    [HttpPost]
    [ProducesResponseType<OrderDetailDto>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest body)
    {
        var order = await _ordersApi.CreateOrder(body);
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }

    /// <summary>Получить заказ</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<OrderDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        try
        {
            var order = await _ordersApi.GetOrder(id);
            return Ok(order);
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return NotFound();
        }
    }

    /// <summary>Отменить заказ</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelOrder(Guid id)
    {
        try
        {
            await _ordersApi.CancelOrder(id);
            return NoContent();
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return NotFound();
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            return Conflict();
        }
    }

    /// <summary>Обновить статус заказа</summary>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType<OrderDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest body)
    {
        try
        {
            var order = await _ordersApi.UpdateStatus(id, body);
            return Ok(order);
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return NotFound();
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            return Conflict();
        }
    }
}
