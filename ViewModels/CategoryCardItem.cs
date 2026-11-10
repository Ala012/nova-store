using TP_Product.Models;

namespace TP_Product.ViewModels;

public class CategoryCardItem
{
    public Category Category { get; set; } = null!;
    public int ProductCount { get; set; }
}
