using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TP_Product.Data;
using TP_Product.ViewModels;

namespace TP_Product.ViewComponents;

public class CategoriesViewComponent : ViewComponent
{
    private readonly AppDbContext _db;

    public CategoriesViewComponent(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var activeId = int.TryParse(HttpContext.Request.Query["categoryId"], out var id) ? id : (int?)null;

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

        ViewData["ActiveCategoryId"] = activeId;
        return View(categories);
    }
}
