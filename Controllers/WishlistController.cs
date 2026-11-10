using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TP_Product.Repositories;

namespace TP_Product.Controllers;

[Authorize]
public class WishlistController : Controller
{
    private readonly IWishlistRepository wishlistRepository;

    public WishlistController(IWishlistRepository wishlistRepository)
    {
        this.wishlistRepository = wishlistRepository;
    }

    public IActionResult Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return View(wishlistRepository.GetForUser(userId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Toggle(int id, string? returnUrl)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var added = wishlistRepository.Toggle(userId, id);
        var count = wishlistRepository.Count(userId);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Json(new
            {
                ok = true,
                inWishlist = added,
                count,
                message = added ? "Ajouté à vos favoris." : "Retiré de vos favoris."
            });
        }

        TempData["Success"] = added ? "Produit ajouté à vos favoris." : "Produit retiré de vos favoris.";

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction(nameof(Index));
    }
}
