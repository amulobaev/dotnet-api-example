using OrdersApi.Models;
using OrdersApi.Repositories;
using Xunit;

namespace OrdersApi.UnitTests.Repositories;

/// <summary>
/// OrderRepository тестируется напрямую — моки не нужны.
/// Seed создаёт 4 заказа:
///   customer1: Pending, Confirmed
///   customer2: Shipped, Cancelled
/// </summary>
public class OrderRepositoryTests
{
    private static readonly Guid Customer1 = new("11111111-0000-0000-0000-000000000001");
    private static readonly Guid Customer2 = new("11111111-0000-0000-0000-000000000002");
    private static readonly Guid SeededId  = new("aaaaaaaa-0000-0000-0000-000000000001"); // Pending

    private readonly OrderRepository _sut = new();

    [Fact]
    public void GetAll_NoFilter_ReturnsFourSeededOrders()
    {
        Assert.Equal(4, _sut.GetAll(null, null).Count());
    }

    [Fact]
    public void GetAll_FilterByStatus_ReturnsPendingOnly()
    {
        var result = _sut.GetAll("Pending", null).ToList();

        Assert.Single(result);
        Assert.Equal(OrderStatus.Pending, result[0].Status);
    }

    [Fact]
    public void GetAll_FilterByCustomerId_ReturnsTwoOrders()
    {
        var result = _sut.GetAll(null, Customer1).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, o => Assert.Equal(Customer1, o.CustomerId));
    }

    [Fact]
    public void GetAll_CombinedFilter_ReturnsMatchingOrders()
    {
        var result = _sut.GetAll("Shipped", Customer2).ToList();

        Assert.Single(result);
        Assert.Equal(OrderStatus.Shipped, result[0].Status);
        Assert.Equal(Customer2, result[0].CustomerId);
    }

    [Fact]
    public void GetAll_InvalidStatusString_ReturnsAllOrders()
    {
        // неизвестный статус игнорируется — TryParse вернёт false
        Assert.Equal(4, _sut.GetAll("NonExistentStatus", null).Count());
    }

    [Fact]
    public void GetAll_ReturnsSortedByCreatedAtDesc()
    {
        var result = _sut.GetAll(null, null).ToList();

        for (var i = 0; i < result.Count - 1; i++)
            Assert.True(result[i].CreatedAt >= result[i + 1].CreatedAt);
    }

    [Fact]
    public void GetById_ReturnsOrder_WhenExists()
    {
        var order = _sut.GetById(SeededId);

        Assert.NotNull(order);
        Assert.Equal(SeededId, order.Id);
    }

    [Fact]
    public void GetById_ReturnsNull_WhenNotFound()
    {
        Assert.Null(_sut.GetById(Guid.NewGuid()));
    }

    [Fact]
    public void Add_PersistsOrder_RetrievableAfterwards()
    {
        var order = new Order { CustomerId = Guid.NewGuid() };

        _sut.Add(order);

        var retrieved = _sut.GetById(order.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(order.CustomerId, retrieved.CustomerId);
    }

    [Fact]
    public void Add_IncreasesTotalCount()
    {
        var before = _sut.GetAll(null, null).Count();

        _sut.Add(new Order { CustomerId = Guid.NewGuid() });

        Assert.Equal(before + 1, _sut.GetAll(null, null).Count());
    }

    [Fact]
    public void UpdateStatus_ChangesOrderStatus()
    {
        _sut.UpdateStatus(SeededId, OrderStatus.Confirmed);

        Assert.Equal(OrderStatus.Confirmed, _sut.GetById(SeededId)!.Status);
    }

    [Fact]
    public void UpdateStatus_UnknownId_DoesNotThrow()
    {
        // не должен бросать исключение
        _sut.UpdateStatus(Guid.NewGuid(), OrderStatus.Confirmed);
    }
}
