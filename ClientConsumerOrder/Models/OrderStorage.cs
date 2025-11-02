using System.Collections.Concurrent;

namespace ClientConsumerOrder.Models;

public static class OrderStorage
{
    private static readonly ConcurrentDictionary<Guid, OrderDto> _orders = new();

    public static List<OrderDto> Orders => _orders.Values.ToList();

    public static void AddOrder(OrderDto order)
    {
        _orders[order.OrderId] = order;

        // Mantener máximo 1000 órdenes
        if (_orders.Count > 1000)
        {
            var oldestOrder = _orders.Values.OrderBy(o => o.OrderDate).First();
            _orders.TryRemove(oldestOrder.OrderId, out _);
        }
    }

    public static OrderDto? GetOrder(Guid orderId)
    {
        _orders.TryGetValue(orderId, out var order);
        return order;
    }

    public static bool UpdateOrder(OrderDto updatedOrder)
    {
        if (_orders.ContainsKey(updatedOrder.OrderId))
        {
            _orders[updatedOrder.OrderId] = updatedOrder;
            return true;
        }
        return false;
    }

    public static bool DeleteOrder(Guid orderId)
    {
        return _orders.TryRemove(orderId, out _);
    }

    public static int GetCount() => _orders.Count;

    public static List<OrderDto> GetRecentOrders(int count = 10)
    {
        return _orders.Values
            .OrderByDescending(o => o.OrderDate)
            .Take(count)
            .ToList();
    }

    public static void ClearAll()
    {
        _orders.Clear();
    }
}