namespace TP_Product.Models.Help;

public class CartItem
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Image { get; set; } = "products/default.svg";
    public string CategoryName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }
    public int Stock { get; set; }
    public int Quantity { get; set; }

    public decimal LineTotal => Price * Quantity;
}
