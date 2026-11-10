using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TP_Product.Models;

public class Product
{
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Le nom du produit est obligatoire.")]
    [StringLength(80, MinimumLength = 3)]
    [Display(Name = "Nom du produit")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le prix est obligatoire.")]
    [Range(0.01, 999999, ErrorMessage = "Le prix doit être supérieur à 0.")]
    [Display(Name = "Prix en DT")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Range(0.01, 999999)]
    [Display(Name = "Ancien prix (DT)")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal? OldPrice { get; set; }

    [Required]
    [Range(0, 99999)]
    [Display(Name = "Quantité en stock")]
    public int QteStock { get; set; }

    [Required]
    [Display(Name = "Catégorie")]
    public int CategoryId { get; set; }

    public Category? Category { get; set; }

    [Required]
    [StringLength(255)]
    [Display(Name = "Image")]
    public string Image { get; set; } = "products/default.svg";

    [StringLength(1200)]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Mis en avant")]
    public bool IsFeatured { get; set; }

    [Display(Name = "Nouveau")]
    public bool IsNew { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public double RatingAverage { get; set; }

    public int RatingCount { get; set; }

    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    [NotMapped]
    public int DiscountPercent =>
        OldPrice.HasValue && OldPrice.Value > Price && OldPrice.Value > 0
            ? (int)Math.Round((OldPrice.Value - Price) / OldPrice.Value * 100)
            : 0;

    [NotMapped]
    public bool InStock => QteStock > 0;

    [NotMapped]
    public bool LowStock => QteStock > 0 && QteStock <= 5;
}
