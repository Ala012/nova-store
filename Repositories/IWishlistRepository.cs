using TP_Product.Models;

namespace TP_Product.Repositories;

public interface IWishlistRepository
{
    IList<WishlistItem> GetForUser(string userId);
    bool Contains(string userId, int productId);
    bool Toggle(string userId, int productId);
    void Remove(string userId, int productId);
    int Count(string userId);
}
