using Microsoft.EntityFrameworkCore;
using TP_Product.Data;
using TP_Product.Models;

namespace TP_Product.Repositories;

public class WishlistRepository : IWishlistRepository
{
    private readonly AppDbContext context;

    public WishlistRepository(AppDbContext context)
    {
        this.context = context;
    }

    public IList<WishlistItem> GetForUser(string userId)
    {
        return context.WishlistItems
            .Include(w => w.Product)
            .ThenInclude(p => p!.Category)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreatedAt)
            .ToList();
    }

    public bool Contains(string userId, int productId)
    {
        return context.WishlistItems.Any(w => w.UserId == userId && w.ProductId == productId);
    }

    public bool Toggle(string userId, int productId)
    {
        var existing = context.WishlistItems
            .FirstOrDefault(w => w.UserId == userId && w.ProductId == productId);

        if (existing is not null)
        {
            context.WishlistItems.Remove(existing);
            context.SaveChanges();
            return false;
        }

        if (!context.Products.Any(p => p.ProductId == productId))
            return false;

        context.WishlistItems.Add(new WishlistItem
        {
            UserId = userId,
            ProductId = productId
        });
        context.SaveChanges();
        return true;
    }

    public void Remove(string userId, int productId)
    {
        var existing = context.WishlistItems
            .FirstOrDefault(w => w.UserId == userId && w.ProductId == productId);
        if (existing is null) return;

        context.WishlistItems.Remove(existing);
        context.SaveChanges();
    }

    public int Count(string userId)
    {
        return context.WishlistItems.Count(w => w.UserId == userId);
    }
}
