using System.ComponentModel.DataAnnotations;
using TP_Product.Models;
using TP_Product.Models.Query;

namespace TP_Product.ViewModels;

public class CategoryNavItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = "bi-grid";
    public string Accent { get; set; } = "#6d5efc";
    public int ProductCount { get; set; }
}

public class HomeViewModel
{
    public IList<Product> Featured { get; set; } = new List<Product>();
    public IList<Product> NewArrivals { get; set; } = new List<Product>();
    public IList<Product> TopRated { get; set; } = new List<Product>();
    public IList<CategoryNavItem> Categories { get; set; } = new List<CategoryNavItem>();
    public int TotalProducts { get; set; }
    public int TotalOrders { get; set; }
    public int HappyCustomers { get; set; }
}

public class ProductListViewModel
{
    public PagedResult<Product> Result { get; set; } = new();
    public ProductFilter Filter { get; set; } = new();
    public string? CategoryName { get; set; }
    public IList<CategoryNavItem> Categories { get; set; } = new List<CategoryNavItem>();
    public decimal MinCatalogPrice { get; set; }
    public decimal MaxCatalogPrice { get; set; }
}

public class ReviewInputModel
{
    [Range(1, 5, ErrorMessage = "Choisissez une note entre 1 et 5 étoiles.")]
    public int Rating { get; set; } = 5;

    [StringLength(1000, ErrorMessage = "Votre avis ne peut pas dépasser 1000 caractères.")]
    [Display(Name = "Votre avis")]
    public string? Comment { get; set; }
}

public class ProductDetailsViewModel
{
    public Product Product { get; set; } = null!;
    public IList<Review> Reviews { get; set; } = new List<Review>();
    public IList<Product> Related { get; set; } = new List<Product>();
    public bool IsInWishlist { get; set; }
    public bool CanReview { get; set; }
    public ReviewInputModel NewReview { get; set; } = new();
}

public class LoginViewModel
{
    [Required(ErrorMessage = "L'e-mail est obligatoire.")]
    [EmailAddress(ErrorMessage = "Adresse e-mail invalide.")]
    [Display(Name = "Adresse e-mail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mot de passe")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Se souvenir de moi")]
    public bool RememberMe { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "L'e-mail est obligatoire.")]
    [EmailAddress(ErrorMessage = "Adresse e-mail invalide.")]
    [Display(Name = "Adresse e-mail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "6 caractères minimum.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mot de passe")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirmer le mot de passe")]
    [Compare(nameof(Password), ErrorMessage = "Les mots de passe ne correspondent pas.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Display(Name = "J'accepte les conditions d'utilisation")]
    public bool AcceptTerms { get; set; }
}

public class CreateRoleViewModel
{
    [Required(ErrorMessage = "Le nom du rôle est obligatoire.")]
    [Display(Name = "Rôle")]
    public string RoleName { get; set; } = string.Empty;
}

public class EditRoleViewModel
{
    public EditRoleViewModel()
    {
        Users = new List<string>();
    }

    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le nom du rôle est obligatoire.")]
    [Display(Name = "Rôle")]
    public string RoleName { get; set; } = string.Empty;

    public List<string> Users { get; set; }
}

public class UserRoleViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
}

public class CartItemViewModel
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public class OrderViewModel
{
    [Required(ErrorMessage = "L'adresse de livraison est obligatoire.")]
    [StringLength(250, MinimumLength = 8, ErrorMessage = "Adresse trop courte.")]
    [Display(Name = "Adresse de livraison")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le numéro de téléphone est obligatoire.")]
    [Phone(ErrorMessage = "Numéro de téléphone invalide.")]
    [StringLength(30)]
    [Display(Name = "Téléphone")]
    public string Phone { get; set; } = string.Empty;

    [Display(Name = "Méthode de paiement")]
    public string PaymentMethod { get; set; } = "Carte bancaire";

    public decimal Subtotal { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal TotalAmount { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<CartItemViewModel> CartItems { get; set; } = new();
}

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Le mot de passe actuel est obligatoire.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mot de passe actuel")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le nouveau mot de passe est obligatoire.")]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Nouveau mot de passe")]
    public string NewPassword { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirmer le nouveau mot de passe")]
    [Compare(nameof(NewPassword), ErrorMessage = "Les mots de passe ne correspondent pas.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class ProfileViewModel
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string RoleLabel { get; set; } = "Client";
    public int OrdersCount { get; set; }
    public int WishlistCount { get; set; }
    public DateTime MemberSince { get; set; }
    public ChangePasswordViewModel ChangePassword { get; set; } = new();
}

public class SalesPoint
{
    public string Label { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Orders { get; set; }
}

public class DashboardViewModel
{
    public decimal TotalRevenue { get; set; }
    public int OrdersCount { get; set; }
    public int ProductsCount { get; set; }
    public int CustomersCount { get; set; }
    public int PendingCount { get; set; }
    public int ReviewsCount { get; set; }
    public int LowStockCount { get; set; }
    public IList<Order> RecentOrders { get; set; } = new List<Order>();
    public IList<TopProduct> TopProducts { get; set; } = new List<TopProduct>();
    public IList<Product> LowStock { get; set; } = new List<Product>();
    public IList<SalesPoint> SalesLast7Days { get; set; } = new List<SalesPoint>();
    public Dictionary<string, int> StatusCounts { get; set; } = new();
}

public class ManageOrdersViewModel
{
    public IList<Order> Orders { get; set; } = new List<Order>();
    public string? Status { get; set; }
    public int Total { get; set; }
    public int Pending { get; set; }
    public int Delivered { get; set; }
    public int Cancelled { get; set; }
}
