using System.ComponentModel.DataAnnotations;

namespace TP_Product.ViewModels;

public class CreateViewModel
{
    [Required(ErrorMessage = "Le nom du produit est obligatoire.")]
    [StringLength(80, MinimumLength = 3)]
    [Display(Name = "Nom du produit")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le prix est obligatoire.")]
    [Range(0.01, 999999)]
    [Display(Name = "Prix en DT")]
    public decimal Price { get; set; }

    [Range(0.01, 999999)]
    [Display(Name = "Ancien prix (DT)")]
    public decimal? OldPrice { get; set; }

    [Required]
    [Range(0, 99999)]
    [Display(Name = "Quantité en stock")]
    public int QteStock { get; set; }

    [Required(ErrorMessage = "Choisissez une catégorie.")]
    [Display(Name = "Catégorie")]
    public int CategoryId { get; set; }

    [StringLength(1200)]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Mis en avant")]
    public bool IsFeatured { get; set; }

    [Display(Name = "Nouveau")]
    public bool IsNew { get; set; }

    [Display(Name = "Image du produit")]
    public IFormFile? ImagePath { get; set; }
}

public class EditViewModel : CreateViewModel
{
    public int ProductId { get; set; }

    public string? ExistingImagePath { get; set; }
}
