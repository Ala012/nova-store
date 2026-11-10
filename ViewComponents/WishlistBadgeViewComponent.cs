using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TP_Product.Repositories;

namespace TP_Product.ViewComponents;

public class WishlistBadgeViewComponent : ViewComponent
{
    private readonly IWishlistRepository _wishlist;

    public WishlistBadgeViewComponent(IWishlistRepository wishlist)
    {
        _wishlist = wishlist;
    }

    public IViewComponentResult Invoke()
    {
        var userId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        var count = string.IsNullOrEmpty(userId) ? 0 : _wishlist.Count(userId);
        return View(count);
    }
}
