using Microsoft.EntityFrameworkCore;
using TP_Product.Data;
using TP_Product.Models;

namespace TP_Product.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext context;

    public OrderRepository(AppDbContext context)
    {
        this.context = context;
    }

    public IQueryable<Order> Query()
    {
        return context.Orders.AsQueryable();
    }

    public Order? GetById(int id)
    {
        return context.Orders
            .Include(o => o.Items)
            .Include(o => o.User)
            .FirstOrDefault(o => o.Id == id);
    }

    public IList<Order> GetAll()
    {
        return context.Orders
            .Include(o => o.Items)
            .Include(o => o.User)
            .OrderByDescending(o => o.OrderDate)
            .ToList();
    }

    public IList<Order> GetByUser(string userId)
    {
        return context.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToList();
    }

    public IList<Order> GetRecent(int count)
    {
        return context.Orders
            .Include(o => o.Items)
            .Include(o => o.User)
            .OrderByDescending(o => o.OrderDate)
            .Take(count)
            .ToList();
    }

    public Order? PlaceOrder(Order order)
    {
        using var transaction = context.Database.BeginTransaction();

        foreach (var item in order.Items)
        {
            var product = context.Products.Find(item.ProductId);
            if (product is null || product.QteStock < item.Quantity)
            {
                transaction.Rollback();
                return null;
            }

            product.QteStock -= item.Quantity;
            item.ProductName = product.Name;
            item.Image = product.Image;
            item.Price = product.Price;
        }

        context.Orders.Add(order);
        context.SaveChanges();
        transaction.Commit();
        return order;
    }

    public bool UpdateStatus(int orderId, string newStatus, out string? error)
    {
        error = null;
        var order = context.Orders
            .Include(o => o.Items)
            .FirstOrDefault(o => o.Id == orderId);

        if (order is null)
        {
            error = "Commande introuvable.";
            return false;
        }

        if (!OrderStatuses.All.Contains(newStatus))
        {
            error = "Statut invalide.";
            return false;
        }

        if (order.Status == newStatus)
            return true;

        var wasCancelled = order.Status == OrderStatuses.Cancelled;
        var willCancel = newStatus == OrderStatuses.Cancelled;

        if (!wasCancelled && willCancel)
        {
            foreach (var item in order.Items)
            {
                var product = context.Products.Find(item.ProductId);
                if (product is not null)
                    product.QteStock += item.Quantity;
            }
        }
        else if (wasCancelled && !willCancel)
        {
            foreach (var item in order.Items)
            {
                var product = context.Products.Find(item.ProductId);
                if (product is null || product.QteStock < item.Quantity)
                {
                    error = $"Stock insuffisant pour « {item.ProductName} ».";
                    return false;
                }
            }

            foreach (var item in order.Items)
            {
                var product = context.Products.Find(item.ProductId);
                if (product is not null)
                    product.QteStock -= item.Quantity;
            }
        }

        order.Status = newStatus;
        context.SaveChanges();
        return true;
    }

    public void Save()
    {
        context.SaveChanges();
    }
}
