using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TP_Product.Data;
using TP_Product.Models;
using TP_Product.Repositories;
using TP_Product.ViewModels;

namespace TP_Product.Controllers;

public class HomeController : Controller
{
    private readonly IProductRepository _products;
    private readonly AppDbContext _db;

    public HomeController(IProductRepository products, AppDbContext db)
    {
        _products = products;
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _db.Categories
            .AsNoTracking()
            .OrderBy(c => c.CategoryName)
            .Select(c => new CategoryNavItem
            {
                Id = c.CategoryId,
                Name = c.CategoryName,
                Icon = c.Icon,
                Accent = c.Accent,
                ProductCount = c.Products.Count
            })
            .ToListAsync();

        var totalOrders = await _db.Orders.CountAsync();
        var happyCustomers = await _db.Orders
            .Where(o => o.Email != null && o.Email != "")
            .Select(o => o.Email)
            .Distinct()
            .CountAsync();

        var model = new HomeViewModel
        {
            Featured = _products.GetFeatured(8),
            NewArrivals = _products.GetNewArrivals(4),
            TopRated = _products.GetTopRated(4),
            Categories = categories,
            TotalProducts = _products.Count(),
            TotalOrders = totalOrders,
            HappyCustomers = happyCustomers
        };

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
