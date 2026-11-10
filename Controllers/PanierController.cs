using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TP_Product.Helpers;
using TP_Product.Models;
using TP_Product.Repositories;
using TP_Product.Services;
using TP_Product.ViewModels;

namespace TP_Product.Controllers;

public class PanierController : Controller
{
    private readonly IProductRepository productRepository;
    private readonly IOrderRepository orderRepository;
    private readonly ICartService cart;
    private readonly UserManager<IdentityUser> userManager;

    public PanierController(
        IProductRepository productRepository,
        IOrderRepository orderRepository,
        ICartService cart,
        UserManager<IdentityUser> userManager)
    {
        this.productRepository = productRepository;
        this.orderRepository = orderRepository;
        this.cart = cart;
        this.userManager = userManager;
    }

    public IActionResult Index()
    {
        ViewBag.Subtotal = cart.Subtotal;
        ViewBag.Count = cart.Count;
        return View(cart.Items.ToList());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddProduct(int id, string? returnUrl, int quantity = 1)
    {
        var product = productRepository.GetById(id);
        if (product is null)
        {
            TempData["Error"] = "Ce produit n'existe plus.";
            return RedirectToAction(nameof(Index));
        }

        if (quantity < 1) quantity = 1;

        if (product.QteStock <= 0)
        {
            TempData["Error"] = "Ce produit est momentanément en rupture de stock.";
        }
        else if (cart.Add(product, quantity))
        {
            TempData["Success"] = $"« {product.Name} » a été ajouté à votre panier.";
        }
        else
        {
            TempData["Error"] = "La quantité maximale disponible est déjà dans votre panier.";
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult PlusProduct(int id)
    {
        var item = cart.GetItem(id);
        if (item is null)
            return Json(new { ok = false, message = "Produit absent du panier." });

        var changed = cart.Increment(id);
        item = cart.GetItem(id);

        return Json(new
        {
            ok = changed,
            message = changed ? "Quantité mise à jour." : "Stock maximum atteint.",
            quantity = item?.Quantity ?? 0,
            lineTotal = item?.LineTotal ?? 0,
            subtotal = cart.Subtotal,
            count = cart.Count,
            removed = item is null
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult MinusProduct(int id)
    {
        var item = cart.GetItem(id);
        if (item is null)
            return Json(new { ok = false, message = "Produit absent du panier." });

        cart.Decrement(id);
        item = cart.GetItem(id);

        return Json(new
        {
            ok = true,
            message = item is null ? "Article retiré du panier." : "Quantité mise à jour.",
            quantity = item?.Quantity ?? 0,
            lineTotal = item?.LineTotal ?? 0,
            subtotal = cart.Subtotal,
            count = cart.Count,
            removed = item is null
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RemoveProduct(int id)
    {
        var removed = cart.Remove(id);

        return Json(new
        {
            ok = removed,
            message = removed ? "Article retiré du panier." : "Produit absent du panier.",
            subtotal = cart.Subtotal,
            count = cart.Count,
            removed = true
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Clear()
    {
        cart.Clear();
        TempData["Success"] = "Votre panier a été vidé.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Checkout()
    {
        if (!cart.Items.Any())
        {
            TempData["Error"] = "Votre panier est vide : ajoutez des produits avant de commander.";
            return RedirectToAction(nameof(Index));
        }

        var user = await userManager.GetUserAsync(User);
        if (user is null)
            return Challenge();

        var lastOrder = orderRepository.GetByUser(user.Id).FirstOrDefault();
        var subtotal = cart.Subtotal;
        var shipping = Shipping.Fee(subtotal);

        var model = new OrderViewModel
        {
            CustomerName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Address = lastOrder?.Address ?? string.Empty,
            Phone = lastOrder?.Phone ?? string.Empty,
            PaymentMethod = "Carte bancaire",
            Subtotal = subtotal,
            ShippingFee = shipping,
            TotalAmount = subtotal + shipping,
            CartItems = cart.Items.Select(item => new CartItemViewModel
            {
                ProductName = item.Name,
                Quantity = item.Quantity,
                Price = item.Price
            }).ToList()
        };

        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(OrderViewModel model)
    {
        if (!cart.Items.Any())
        {
            TempData["Error"] = "Votre panier est vide.";
            return RedirectToAction(nameof(Index));
        }

        var user = await userManager.GetUserAsync(User);
        if (user is null)
            return Challenge();

        foreach (var item in cart.Items)
        {
            var product = productRepository.GetById(item.ProductId);
            if (product is null)
                ModelState.AddModelError(string.Empty, $"Le produit « {item.Name} » n'est plus disponible.");
            else if (product.QteStock < item.Quantity)
                ModelState.AddModelError(string.Empty, $"Stock insuffisant pour « {item.Name} » ({product.QteStock} restant(s)).");
        }

        var subtotal = cart.Subtotal;
        var shipping = Shipping.Fee(subtotal);

        if (!ModelState.IsValid)
        {
            model.CustomerName = user.UserName ?? string.Empty;
            model.Email = user.Email ?? string.Empty;
            model.Subtotal = subtotal;
            model.ShippingFee = shipping;
            model.TotalAmount = subtotal + shipping;
            model.CartItems = cart.Items.Select(item => new CartItemViewModel
            {
                ProductName = item.Name,
                Quantity = item.Quantity,
                Price = item.Price
            }).ToList();
            return View(model);
        }

        var order = new Order
        {
            CustomerName = model.CustomerName,
            Email = model.Email,
            Address = model.Address.Trim(),
            Phone = model.Phone.Trim(),
            PaymentMethod = model.PaymentMethod,
            TotalAmount = subtotal + shipping,
            OrderDate = DateTime.Now,
            UserId = user.Id,
            Status = OrderStatuses.Pending,
            Items = cart.Items.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                ProductName = item.Name,
                Image = item.Image,
                Quantity = item.Quantity,
                Price = item.Price
            }).ToList()
        };

        var created = orderRepository.PlaceOrder(order);
        if (created is null)
        {
            TempData["Error"] = "Une erreur est survenue (stock insuffisant). Vérifiez votre panier.";
            return RedirectToAction(nameof(Index));
        }

        cart.Clear();
        TempData["Success"] = "Votre commande a été passée avec succès !";
        return RedirectToAction(nameof(Confirmation), new { orderId = created.Id });
    }

    [Authorize]
    public IActionResult Confirmation(int orderId)
    {
        var order = orderRepository.GetById(orderId);
        if (order is null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
            return Forbid();

        return View(order);
    }
}
