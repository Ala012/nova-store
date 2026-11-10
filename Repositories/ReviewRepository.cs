using Microsoft.EntityFrameworkCore;
using TP_Product.Data;
using TP_Product.Models;

namespace TP_Product.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly AppDbContext context;

    public ReviewRepository(AppDbContext context)
    {
        this.context = context;
    }

    public IList<Review> GetForProduct(int productId)
    {
        return context.Reviews
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .ToList();
    }

    public bool HasUserReviewed(int productId, string userId)
    {
        return context.Reviews.Any(r => r.ProductId == productId && r.UserId == userId);
    }

    public void Add(Review review)
    {
        context.Reviews.Add(review);
        context.SaveChanges();
    }

    public void RecalculateProductRating(int productId)
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
        return context.Reviews.Count();
    }
}
