using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TP_Product.Data;
using TP_Product.Models;
using TP_Product.Repositories;
using TP_Product.ViewModels;

namespace TP_Product.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class DashboardController : Controller
{
    private readonly IOrderRepository _orders;
    private readonly IProductRepository _products;
    private readonly IReviewRepository _reviews;
    private readonly AppDbContext _db;

    public DashboardController(
        IOrderRepository orders,
        IProductRepository products,
        IReviewRepository reviews,
        AppDbContext db)
    {
        _orders = orders;
        _products = products;
        _reviews = reviews;
        _db = db;
    }

    public IActionResult Index()
    {
        var allOrders = _orders.Query()
            .Include(o => o.Items)
            .Include(o => o.User)
            .AsNoTracking()
            .ToList();

        var realized = allOrders
            .Where(o => o.Status != OrderStatuses.Cancelled && o.Status != OrderStatuses.Pending)
            .ToList();

        var start = DateTime.Now.Date.AddDays(-6);
        var sales = new List<SalesPoint>();

        for (var i = 0; i < 7; i++)
        {
            var day = start.AddDays(i);
            var dayOrders = realized.Where(o => o.OrderDate.Date == day).ToList();
            sales.Add(new SalesPoint
            {
                Label = day.ToString("dd/MM", CultureInfo.InvariantCulture),
                Amount = dayOrders.Sum(o => o.TotalAmount),
                Orders = dayOrders.Count
            });
        }

        var model = new DashboardViewModel
        {
            TotalRevenue = realized.Sum(o => o.TotalAmount),
            OrdersCount = allOrders.Count,
            ProductsCount = _products.Count(),
            CustomersCount = allOrders
                .Where(o => !string.IsNullOrWhiteSpace(o.Email))
                .Select(o => o.Email)
                .Distinct()
                .Count(),
            PendingCount = allOrders.Count(o => o.Status == OrderStatuses.Pending),
            ReviewsCount = _reviews.Count(),
            LowStockCount = _db.Products.Count(p => p.QteStock <= 5),
            RecentOrders = allOrders.OrderByDescending(o => o.OrderDate).Take(7).ToList(),
            TopProducts = _products.GetTopSelling(5),
            LowStock = _products.GetLowStock(5, 6),
            SalesLast7Days = sales,
            StatusCounts = allOrders
                .GroupBy(o => o.Status)
                .ToDictionary(g => g.Key, g => g.Count())
        };

        return View(model);
    }
}
