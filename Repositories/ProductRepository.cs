using Microsoft.EntityFrameworkCore;
using TP_Product.Data;
using TP_Product.Models;
using TP_Product.Models.Query;

namespace TP_Product.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext context;

    public ProductRepository(AppDbContext context)
    {
        this.context = context;
    }

    public IQueryable<Product> GetAllProducts()
    {
        return context.Products.Include(p => p.Category);
    }

    public IList<Product> GetAll()
    {
        return context.Products
            .Include(p => p.Category)
            .OrderBy(p => p.Name)
            .ToList();
    }

    public Product? GetById(int id)
    {
        return context.Products
            .Include(p => p.Category)
            .FirstOrDefault(p => p.ProductId == id);
    }

    public Product? GetByIdWithReviews(int id)
    {
        return context.Products
            .Include(p => p.Category)
            .Include(p => p.Reviews.OrderByDescending(r => r.CreatedAt))
            .FirstOrDefault(p => p.ProductId == id);
    }

    public void Add(Product product)
    {
        context.Products.Add(product);
        context.SaveChanges();
    }

    public Product? Update(Product product)
    {
        var existing = context.Products.Find(product.ProductId);
        if (existing is null) return null;

        existing.Name = product.Name;
        existing.Price = product.Price;
        existing.OldPrice = product.OldPrice;
        existing.QteStock = product.QteStock;
        existing.CategoryId = product.CategoryId;
        existing.Description = product.Description;
        existing.Image = product.Image;
        existing.IsFeatured = product.IsFeatured;
        existing.IsNew = product.IsNew;
        context.SaveChanges();
        return existing;
    }

    public void Delete(int id)
    {
        var product = context.Products.Find(id);
        if (product is null) return;

        context.Products.Remove(product);
        context.SaveChanges();
    }

    public IList<Product> FindByName(string name)
    {
        return context.Products
            .Where(p => p.Name.Contains(name) || (p.Category != null && p.Category.CategoryName.Contains(name)))
            .Include(p => p.Category)
            .OrderBy(p => p.Name)
            .ToList();
    }

    public IList<Product> GetProductsByCategID(int? categId)
    {
        return context.Products
            .Where(p => p.CategoryId == categId)
            .Include(p => p.Category)
            .OrderBy(p => p.ProductId)
            .ToList();
    }

    public IList<Product> GetFeatured(int count)
    {
        return context.Products
            .Include(p => p.Category)
            .Where(p => p.IsFeatured && p.QteStock > 0)
            .OrderByDescending(p => p.RatingAverage)
            .ThenByDescending(p => p.CreatedAt)
            .Take(count)
            .ToList();
    }

    public IList<Product> GetNewArrivals(int count)
    {
        return context.Products
            .Include(p => p.Category)
            .Where(p => p.IsNew && p.QteStock > 0)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToList();
    }

    public PagedResult<Product> GetPaged(ProductFilter filter)
    {
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 48);

        var query = context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            query = query.Where(p =>
                p.Name.Contains(term) ||
                (p.Category != null && p.Category.CategoryName.Contains(term)) ||
                (p.Description != null && p.Description.Contains(term)));
        }

        if (filter.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == filter.CategoryId.Value);

        if (filter.MinPrice.HasValue)
            query = query.Where(p => p.Price >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= filter.MaxPrice.Value);

        if (filter.OnlyPromo)
            query = query.Where(p => p.OldPrice != null && p.OldPrice > p.Price);

        if (filter.InStockOnly)
            query = query.Where(p => p.QteStock > 0);

        query = filter.Sort switch
        {
            "price-asc" => query.OrderBy(p => p.Price),
            "price-desc" => query.OrderByDescending(p => p.Price),
            "rating" => query.OrderByDescending(p => p.RatingAverage).ThenByDescending(p => p.RatingCount),
            "name" => query.OrderBy(p => p.Name),
            "new" => query.OrderByDescending(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.CreatedAt)
        };

        var total = query.Count();
        var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new PagedResult<Product>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public IList<Product> GetRelated(int productId, int categoryId, int count)
    {
        return context.Products
            .Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId && p.ProductId != productId && p.QteStock > 0)
            .OrderByDescending(p => p.RatingAverage)
            .ThenByDescending(p => p.CreatedAt)
            .Take(count)
            .ToList();
    }

    public IList<Product> GetLowStock(int threshold, int count)
    {
        return context.Products
            .Include(p => p.Category)
            .Where(p => p.QteStock <= threshold)
            .OrderBy(p => p.QteStock)
            .Take(count)
            .ToList();
    }

    public IList<Product> GetTopRated(int count)
    {
        return context.Products
            .Include(p => p.Category)
            .Where(p => p.RatingCount > 0)
            .OrderByDescending(p => p.RatingAverage)
            .ThenByDescending(p => p.RatingCount)
            .Take(count)
            .ToList();
    }

    public IList<TopProduct> GetTopSelling(int count)
    {
        var sold = context.OrderItems
            .Where(i => i.Order != null && i.Order.Status != OrderStatuses.Cancelled)
            .GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, Total = g.Sum(x => x.Quantity) })
            .OrderByDescending(x => x.Total)
            .Take(count)
            .ToList();

        var ids = sold.Select(x => x.ProductId).ToList();
        var products = context.Products
            .Include(p => p.Category)
            .Where(p => ids.Contains(p.ProductId))
            .ToDictionary(p => p.ProductId);

        return sold
            .Where(x => products.ContainsKey(x.ProductId))
            .Select(x => new TopProduct { Product = products[x.ProductId], Sold = x.Total })
            .ToList();
    }

    public void RecalculateRating(int productId)
    {
        var product = context.Products.Find(productId);
        if (product is null) return;

        var reviews = context.Reviews.Where(r => r.ProductId == productId);
        product.RatingCount = reviews.Count();
        product.RatingAverage = product.RatingCount == 0 ? 0 : Math.Round(reviews.Average(r => r.Rating), 2);
        context.SaveChanges();
    }

    public int Count()
    {
        return context.Products.Count();
    }
}
