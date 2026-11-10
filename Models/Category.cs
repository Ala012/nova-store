using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TP_Product.Models;

public class Category
{
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Le nom de la catégorie est obligatoire.")]
    [StringLength(60, MinimumLength = 2)]
    [Display(Name = "Nom de la catégorie")]
    public string CategoryName { get; set; } = string.Empty;

    [StringLength(240)]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [StringLength(60)]
    public string Icon { get; set; } = "bi-grid";

    [StringLength(20)]
    public string Accent { get; set; } = "#6d5efc";

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
