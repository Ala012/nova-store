using TP_Product.Models;

namespace TP_Product.Repositories;

public interface IOrderRepository
{
    IQueryable<Order> Query();
    Order? GetById(int id);
    IList<Order> GetAll();
    IList<Order> GetByUser(string userId);
    IList<Order> GetRecent(int count);
    Order? PlaceOrder(Order order);
    bool UpdateStatus(int orderId, string newStatus, out string? error);
    void Save();
}
