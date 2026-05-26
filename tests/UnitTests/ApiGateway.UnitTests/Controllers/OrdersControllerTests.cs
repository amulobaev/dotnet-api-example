using System.Net;
using ApiGateway.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OrdersApi.Client;
using Refit;
using Xunit;

namespace ApiGateway.UnitTests.Controllers;

/// <summary>
/// Демонстрирует ключевое преимущество Refit-подхода:
/// IOrdersApi — обычный C# интерфейс, мокается напрямую без MockHttpMessageHandler.
/// </summary>
public class OrdersControllerTests
{
    private readonly Mock<IOrdersApi> _api = new();
    private readonly OrdersController _sut;

    public OrdersControllerTests()
    {
        _sut = new OrdersController(_api.Object);
    }

    [Fact]
    public async Task GetOrders_ReturnsOk_WithOrders()
    {
        var expected = new List<OrderSummaryDto>
        {
            new() { Id = Guid.NewGuid(), Status = OrderStatus.Pending, Total = 100 }
        };
        _api.Setup(x => x.GetOrders(It.IsAny<string>(), It.IsAny<Guid?>()))
            .ReturnsAsync(expected);

        var result = await _sut.GetOrders(null, null);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expected, ok.Value);
    }

    [Fact]
    public async Task CreateOrder_Returns201_WithCreatedOrder()
    {
        var detail = new OrderDetailDto { Id = Guid.NewGuid(), Status = OrderStatus.Pending };
        _api.Setup(x => x.CreateOrder(It.IsAny<CreateOrderRequest>()))
            .ReturnsAsync(detail);

        var result = await _sut.CreateOrder(new CreateOrderRequest());

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(detail, created.Value);
    }

    [Fact]
    public async Task GetOrder_ReturnsOk_WhenOrderFound()
    {
        var id = Guid.NewGuid();
        var detail = new OrderDetailDto { Id = id, Status = OrderStatus.Confirmed };
        _api.Setup(x => x.GetOrder(id)).ReturnsAsync(detail);

        var result = await _sut.GetOrder(id);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(detail, ok.Value);
    }

    [Fact]
    public async Task GetOrder_ReturnsNotFound_WhenApiReturns404()
    {
        _api.Setup(x => x.GetOrder(It.IsAny<Guid>()))
            .ThrowsAsync(await ApiExceptionFor(HttpStatusCode.NotFound));

        var result = await _sut.GetOrder(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task CancelOrder_ReturnsNoContent_OnSuccess()
    {
        _api.Setup(x => x.CancelOrder(It.IsAny<Guid>())).Returns(Task.CompletedTask);

        var result = await _sut.CancelOrder(Guid.NewGuid());

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task CancelOrder_ReturnsNotFound_WhenApiReturns404()
    {
        _api.Setup(x => x.CancelOrder(It.IsAny<Guid>()))
            .ThrowsAsync(await ApiExceptionFor(HttpStatusCode.NotFound));

        var result = await _sut.CancelOrder(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task CancelOrder_ReturnsConflict_WhenApiReturns409()
    {
        _api.Setup(x => x.CancelOrder(It.IsAny<Guid>()))
            .ThrowsAsync(await ApiExceptionFor(HttpStatusCode.Conflict));

        var result = await _sut.CancelOrder(Guid.NewGuid());

        Assert.IsType<ConflictResult>(result);
    }

    [Fact]
    public async Task UpdateStatus_ReturnsOk_OnSuccess()
    {
        var id = Guid.NewGuid();
        var detail = new OrderDetailDto { Id = id, Status = OrderStatus.Confirmed };
        _api.Setup(x => x.UpdateStatus(id, It.IsAny<UpdateStatusRequest>()))
            .ReturnsAsync(detail);

        var result = await _sut.UpdateStatus(id, new UpdateStatusRequest { Status = TargetOrderStatus.Confirmed });

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(detail, ok.Value);
    }

    [Fact]
    public async Task UpdateStatus_ReturnsNotFound_WhenApiReturns404()
    {
        _api.Setup(x => x.UpdateStatus(It.IsAny<Guid>(), It.IsAny<UpdateStatusRequest>()))
            .ThrowsAsync(await ApiExceptionFor(HttpStatusCode.NotFound));

        var result = await _sut.UpdateStatus(Guid.NewGuid(), new UpdateStatusRequest { Status = TargetOrderStatus.Confirmed });

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateStatus_ReturnsConflict_WhenApiReturns409()
    {
        _api.Setup(x => x.UpdateStatus(It.IsAny<Guid>(), It.IsAny<UpdateStatusRequest>()))
            .ThrowsAsync(await ApiExceptionFor(HttpStatusCode.Conflict));

        var result = await _sut.UpdateStatus(Guid.NewGuid(), new UpdateStatusRequest { Status = TargetOrderStatus.Shipped });

        Assert.IsType<ConflictResult>(result);
    }

    private static async Task<ApiException> ApiExceptionFor(HttpStatusCode statusCode) =>
        await ApiException.Create(
            new HttpRequestMessage(HttpMethod.Get, "/"),
            HttpMethod.Get,
            new HttpResponseMessage(statusCode),
            new RefitSettings());
}
