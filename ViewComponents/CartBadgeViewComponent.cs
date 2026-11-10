using Microsoft.AspNetCore.Mvc;
using TP_Product.Services;

namespace TP_Product.ViewComponents;

public class CartBadgeViewComponent : ViewComponent
{
    private readonly ICartService _cart;

    public CartBadgeViewComponent(ICartService cart)
    {
        _cart = cart;
    }

    public IViewComponentResult Invoke()
    {
        return View(_cart.Count);
    }
}
