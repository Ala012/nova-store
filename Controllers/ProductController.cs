using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TP_Product.Models;
using TP_Product.Models.Query;
using TP_Product.Repositories;
using TP_Product.ViewModels;

namespace TP_Product.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class ProductController : Controller
{
    private static readonly string[] AllowedExtensions =
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif", ".svg"
    };

    private readonly IProductRepository ProductRepository;
    private readonly ICategoryRepository CategRepository;
    private readonly IReviewRepository reviewRepository;
    private readonly IWishlistRepository wishlistRepository;
    private readonly IWebHostEnvironment hostingEnvironment;

    public ProductController(
        IProductRepository productRepository,
        ICategoryRepository categRepository,
        IReviewRepository reviewRepository,
        IWishlistRepository wishlistRepository,
        IWebHostEnvironment hostingEnvironment)
    {
        ProductRepository = productRepository;
        CategRepository = categRepository;
        this.reviewRepository = reviewRepository;
        this.wishlistRepository = wishlistRepository;
        this.hostingEnvironment = hostingEnvironment;
    }

    [AllowAnonymous]
    public IActionResult Index(
        int? categoryId, string? q, string? sort, decimal? min, decimal? max,
        bool promo = false, bool stock = false, int page = 1)
    {
        var filter = new ProductFilter
        {
            CategoryId = categoryId,
            Search = q,
            Sort = sort,
            MinPrice = min,
            MaxPrice = max,
            OnlyPromo = promo,
            InStockOnly = stock,
            Page = page,
            PageSize = 9
        };

        var result = ProductRepository.GetPaged(filter);
        var all = ProductRepository.GetAll();

        var categories = CategRepository.GetAll()
            .Select(c => new CategoryNavItem
            {
                Id = c.CategoryId,
                Name = c.CategoryName,
                Icon = c.Icon,
                Accent = c.Accent,
                ProductCount = ProductRepository.GetProductsByCategID(c.CategoryId).Count
            })
            .ToList();

        var model = new ProductListViewModel
        {
            Result = result,
            Filter = filter,
            Categories = categories,
            CategoryName = categoryId.HasValue ? CategRepository.GetById(categoryId.Value)?.CategoryName : null,
            MinCatalogPrice = all.Count > 0 ? Math.Floor(all.Min(p => p.Price)) : 0,
            MaxCatalogPrice = all.Count > 0 ? Math.Ceiling(all.Max(p => p.Price)) : 1000
        };

        return View(model);
    }

    [AllowAnonymous]
    public IActionResult Search(string val)
    {
        if (string.IsNullOrWhiteSpace(val))
            return RedirectToAction(nameof(Index));

        return RedirectToAction(nameof(Index), new { q = val });
    }

    [AllowAnonymous]
    public IActionResult Details(int id)
    {
        var product = ProductRepository.GetByIdWithReviews(id);
        if (product is null) return NotFound();

        ViewData["Title"] = product.Name;

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isInWishlist = !string.IsNullOrEmpty(userId) && wishlistRepository.Contains(userId, id);
        var canReview = !string.IsNullOrEmpty(userId) && !reviewRepository.HasUserReviewed(id, userId);

        var model = new ProductDetailsViewModel
        {
            Product = product,
            Reviews = product.Reviews.OrderByDescending(r => r.CreatedAt).ToList(),
            Related = ProductRepository.GetRelated(product.ProductId, product.CategoryId, 4),
            IsInWishlist = isInWishlist,
            CanReview = canReview
        };

        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddReview(int id, ReviewInputModel model)
    {
        var product = ProductRepository.GetById(id);
        if (product is null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Challenge();

        if (reviewRepository.HasUserReviewed(id, userId))
        {
            TempData["Error"] = "Vous avez déjà publié un avis sur ce produit.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Merci de choisir une note entre 1 et 5 étoiles.";
            return RedirectToAction(nameof(Details), new { id });
        }

        reviewRepository.Add(new Review
        {
            ProductId = id,
            UserId = userId,
            UserName = User.Identity?.Name ?? "Client Nova",
            Rating = model.Rating,
            Comment = model.Comment?.Trim(),
            CreatedAt = DateTime.Now
        });

        reviewRepository.RecalculateProductRating(id);
        TempData["Success"] = "Merci ! Votre avis a bien été publié.";
        return RedirectToAction(nameof(Details), new { id });
    }

    public IActionResult Manage(string? q, int? categoryId)
    {
        var products = ProductRepository.GetAllProducts().ToList();

        if (!string.IsNullOrWhiteSpace(q))
            products = products.Where(p => p.Name.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();

        if (categoryId.HasValue)
            products = products.Where(p => p.CategoryId == categoryId.Value).ToList();

        ViewBag.Search = q;
        ViewBag.CategoryId = new SelectList(CategRepository.GetAll(), "CategoryId", "CategoryName", categoryId);
        return View(products.OrderByDescending(p => p.ProductId).ToList());
    }

    public IActionResult Create()
    {
        ViewBag.CategoryId = new SelectList(CategRepository.GetAll(), "CategoryId", "CategoryName");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CreateViewModel model)
    {
        ViewBag.CategoryId = new SelectList(CategRepository.GetAll(), "CategoryId", "CategoryName", model.CategoryId);
        ValidatePrice(model);

        if (!ModelState.IsValid)
            return View(model);

        string? image = null;
        try
        {
            image = ProcessUploadedFile(model.ImagePath);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(model.ImagePath), ex.Message);
            return View(model);
        }

        var product = new Product
        {
            Name = model.Name.Trim(),
            Price = model.Price,
            OldPrice = model.OldPrice,
            QteStock = model.QteStock,
            CategoryId = model.CategoryId,
            Description = model.Description?.Trim(),
            IsFeatured = model.IsFeatured,
            IsNew = model.IsNew,
            Image = image ?? "products/default.svg",
            CreatedAt = DateTime.UtcNow
        };

        ProductRepository.Add(product);
        TempData["Success"] = $"Le produit « {product.Name} » a été ajouté.";
        return RedirectToAction(nameof(Manage));
    }

    public IActionResult Edit(int id)
    {
        var product = ProductRepository.GetById(id);
        if (product is null) return NotFound();

        ViewBag.CategoryId = new SelectList(CategRepository.GetAll(), "CategoryId", "CategoryName", product.CategoryId);
        var model = new EditViewModel
        {
            ProductId = product.ProductId,
            Name = product.Name,
            Price = product.Price,
            OldPrice = product.OldPrice,
            QteStock = product.QteStock,
            CategoryId = product.CategoryId,
            Description = product.Description,
            IsFeatured = product.IsFeatured,
            IsNew = product.IsNew,
            ExistingImagePath = product.Image
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(EditViewModel model)
    {
        ViewBag.CategoryId = new SelectList(CategRepository.GetAll(), "CategoryId", "CategoryName", model.CategoryId);
        ValidatePrice(model);

        if (!ModelState.IsValid)
            return View(model);

        var product = ProductRepository.GetById(model.ProductId);
        if (product is null) return NotFound();

        try
        {
            var newImage = ProcessUploadedFile(model.ImagePath);
            if (newImage is not null)
            {
                if (!string.IsNullOrEmpty(model.ExistingImagePath) &&
                    !model.ExistingImagePath.EndsWith("default.svg", StringComparison.OrdinalIgnoreCase))
                {
                    var oldPath = Path.Combine(hostingEnvironment.WebRootPath, "images", model.ExistingImagePath.Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                product.Image = newImage;
            }
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(model.ImagePath), ex.Message);
            return View(model);
        }

        product.Name = model.Name.Trim();
        product.Price = model.Price;
        product.OldPrice = model.OldPrice;
        product.QteStock = model.QteStock;
        product.CategoryId = model.CategoryId;
        product.Description = model.Description?.Trim();
        product.IsFeatured = model.IsFeatured;
        product.IsNew = model.IsNew;

        ProductRepository.Update(product);
        TempData["Success"] = $"Le produit « {product.Name} » a été mis à jour.";
        return RedirectToAction(nameof(Manage));
    }

    public IActionResult Delete(int id)
    {
        var product = ProductRepository.GetById(id);
        if (product is null) return NotFound();
        return View(product);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        var product = ProductRepository.GetById(id);
        if (product is null) return NotFound();

        ProductRepository.Delete(id);
        TempData["Success"] = "Le produit a été supprimé.";
        return RedirectToAction(nameof(Manage));
    }

    private void ValidatePrice(CreateViewModel model)
    {
        if (model.OldPrice.HasValue && model.OldPrice <= model.Price)
            ModelState.AddModelError(nameof(model.OldPrice), "L'ancien prix doit être supérieur au prix actuel.");
    }

    private string? ProcessUploadedFile(IFormFile? file)
    {
        if (file is null || file.Length == 0) return null;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new InvalidOperationException("Format d'image non supporté. Utilisez JPG, PNG, WEBP, GIF ou SVG.");

        if (file.Length > 6 * 1024 * 1024)
            throw new InvalidOperationException("L'image ne doit pas dépasser 6 Mo.");

        var uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images", "products");
        Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = Guid.NewGuid().ToString("N") + extension;
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        file.CopyTo(stream);

        return "products/" + uniqueFileName;
    }
}
