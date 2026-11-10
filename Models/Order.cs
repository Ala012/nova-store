using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace TP_Product.Models;

public class Order
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(250)]
    public string Address { get; set; } = string.Empty;

    [StringLength(30)]
    public string? Phone { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.Now;

    public string UserId { get; set; } = string.Empty;

    public IdentityUser? User { get; set; }

    [StringLength(30)]
    public string Status { get; set; } = OrderStatuses.Pending;

    [StringLength(40)]
    public string PaymentMethod { get; set; } = "Carte bancaire";

    public List<OrderItem> Items { get; set; } = new();
}

public static class OrderStatuses
{
    public const string Pending = "En attente";
    public const string Confirmed = "Confirmée";
    public const string Shipped = "Expédiée";
    public const string Delivered = "Livrée";
    public const string Cancelled = "Annulée";

    public static readonly string[] All =
    {
        Pending, Confirmed, Shipped, Delivered, Cancelled
    };

    public static string CssClass(string status) => status switch
    {
        Confirmed => "status-confirmed",
        Shipped => "status-shipped",
        Delivered => "status-delivered",
        Cancelled => "status-cancelled",
        _ => "status-pending"
    };
}
