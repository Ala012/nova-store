namespace TP_Product.Helpers;

public static class Shipping
{
    public const decimal FreeThreshold = 299m;
    public const decimal FeeAmount = 7m;

    public static decimal Fee(decimal subtotal) => subtotal >= FreeThreshold ? 0m : FeeAmount;
}
