using TP_Product.Models;

namespace TP_Product.Repositories;

public interface IReviewRepository
{
    IList<Review> GetForProduct(int productId);
    bool HasUserReviewed(int productId, string userId);
    void Add(Review review);
    void RecalculateProductRating(int productId);
    int Count();
}
