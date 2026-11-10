using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TP_Product.Models;

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public Order? Order { get; set; }

    public int ProductId { get; set; }

    [Required]
    [StringLength(120)]
    public string ProductName { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Image { get; set; }

    [Range(1, 9999)]
    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [NotMapped]
    public decimal LineTotal => Price * Quantity;
}
