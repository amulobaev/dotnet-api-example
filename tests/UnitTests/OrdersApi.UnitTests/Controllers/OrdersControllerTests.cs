using Microsoft.AspNetCore.Mvc;
using Moq;
using OrdersApi.Contracts;
using OrdersApi.Controllers;
using OrdersApi.Models;
using OrdersApi.Repositories;
using Xunit;

namespace OrdersApi.UnitTests.Controllers;

public class OrdersControllerTests
{
    private readonly Mock<IOrderRepository> _repo = new();
    private readonly OrdersController _sut;

    public OrdersControllerTests()
    {
        _sut = new OrdersController(_repo.Object);
    }

    [Fact]
    public void GetOrders_ReturnsOk()
    {
        _repo.Setup(x => x.GetAll(null, null)).Returns([MakeOrder(OrderStatus.Pending)]);

        var result = _sut.GetOrders(null, null);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public void GetOrders_PassesFiltersToRepository()
    {
        var customerId = Guid.NewGuid();
        _repo.Setup(x => x.GetAll("Pending", customerId)).Returns([]);

        _sut.GetOrders("Pending", customerId);

        _repo.Verify(x => x.GetAll("Pending", customerId), Times.Once);
    }

    [Fact]
    public void GetOrder_ReturnsOk_WhenFound()
    {
        var order = MakeOrder(OrderStatus.Confirmed);
        _repo.Setup(x => x.GetById(order.Id)).Returns(order);

        var result = _sut.GetOrder(order.Id);

        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<OrderDetailDto>(ok.Value);
        Assert.Equal(order.Id, dto.Id);
    }

    [Fact]
    public void GetOrder_ReturnsNotFound_WhenMissing()
    {
        _repo.Setup(x => x.GetById(It.IsAny<Guid>())).Returns((Order?)null);

        var result = _sut.GetOrder(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void CreateOrder_Returns201_AndPersistsOrder()
    {
        var request = new CreateOrderRequest(Guid.NewGuid(),
        [
            new OrderLineDto(Guid.NewGuid(), 2, 99.99)
        ]);
        _repo.Setup(x => x.Add(It.IsAny<Order>())).Returns<Order>(o => o);

        var result = _sut.CreateOrder(request);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var dto = Assert.IsType<OrderDetailDto>(created.Value);
        Assert.Equal(request.CustomerId, dto.CustomerId);
        _repo.Verify(x => x.Add(It.IsAny<Order>()), Times.Once);
    }

    [Theory]
    [InlineData(OrderStatus.Pending,   TargetOrderStatus.Confirmed)]
    [InlineData(OrderStatus.Confirmed, TargetOrderStatus.Shipped)]
    [InlineData(OrderStatus.Pending,   TargetOrderStatus.Cancelled)]
    [InlineData(OrderStatus.Confirmed, TargetOrderStatus.Cancelled)]
    public void UpdateStatus_ReturnsOk_ForValidTransitions(
        OrderStatus from, TargetOrderStatus to)
    {
        var order = MakeOrder(from);
        _repo.Setup(x => x.GetById(order.Id)).Returns(order);

        var result = _sut.UpdateStatus(order.Id, new UpdateStatusRequest(to));

        Assert.IsType<OkObjectResult>(result);
        _repo.Verify(x => x.UpdateStatus(order.Id, It.IsAny<OrderStatus>()), Times.Once);
    }

    [Theory]
    [InlineData(OrderStatus.Shipped,   TargetOrderStatus.Confirmed)]
    [InlineData(OrderStatus.Cancelled, TargetOrderStatus.Confirmed)]
    [InlineData(OrderStatus.Shipped,   TargetOrderStatus.Cancelled)]
    public void UpdateStatus_ReturnsConflict_ForInvalidTransitions(
        OrderStatus from, TargetOrderStatus to)
    {
        var order = MakeOrder(from);
        _repo.Setup(x => x.GetById(order.Id)).Returns(order);

        var result = _sut.UpdateStatus(order.Id, new UpdateStatusRequest(to));

        Assert.IsType<ConflictObjectResult>(result);
        _repo.Verify(x => x.UpdateStatus(It.IsAny<Guid>(), It.IsAny<OrderStatus>()), Times.Never);
    }

    [Fact]
    public void UpdateStatus_ReturnsNotFound_WhenOrderMissing()
    {
        _repo.Setup(x => x.GetById(It.IsAny<Guid>())).Returns((Order?)null);

        var result = _sut.UpdateStatus(Guid.NewGuid(), new UpdateStatusRequest(TargetOrderStatus.Confirmed));

        Assert.IsType<NotFoundResult>(result);
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Confirmed)]
    public void CancelOrder_ReturnsNoContent_WhenCancellable(OrderStatus status)
    {
        var order = MakeOrder(status);
        _repo.Setup(x => x.GetById(order.Id)).Returns(order);

        var result = _sut.CancelOrder(order.Id);

        Assert.IsType<NoContentResult>(result);
        _repo.Verify(x => x.UpdateStatus(order.Id, OrderStatus.Cancelled), Times.Once);
    }

    [Theory]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Cancelled)]
    public void CancelOrder_ReturnsConflict_WhenNotCancellable(OrderStatus status)
    {
        var order = MakeOrder(status);
        _repo.Setup(x => x.GetById(order.Id)).Returns(order);

        var result = _sut.CancelOrder(order.Id);

        Assert.IsType<ConflictObjectResult>(result);
        _repo.Verify(x => x.UpdateStatus(It.IsAny<Guid>(), It.IsAny<OrderStatus>()), Times.Never);
    }

    [Fact]
    public void CancelOrder_ReturnsNotFound_WhenOrderMissing()
    {
        _repo.Setup(x => x.GetById(It.IsAny<Guid>())).Returns((Order?)null);

        var result = _sut.CancelOrder(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    private static Order MakeOrder(OrderStatus status) => new()
    {
        Id = Guid.NewGuid(),
        CustomerId = Guid.NewGuid(),
        Status = status,
        Lines = [new OrderLine { ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 100 }]
    };
}
