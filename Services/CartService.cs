using System.Text.Json;
using TP_Product.Models;
using TP_Product.Models.Help;

namespace TP_Product.Services;

public interface ICartService
{
    IReadOnlyList<CartItem> Items { get; }
    int Count { get; }
    decimal Subtotal { get; }
    CartItem? GetItem(int productId);
    bool Add(Product product, int quantity = 1);
    bool Increment(int productId);
    bool Decrement(int productId);
    bool Remove(int productId);
    void Clear();
}

public class CartService : ICartService
{
    private const string SessionKey = "nova.cart.v1";
    private readonly IHttpContextAccessor _http;
    private List<CartItem>? _cache;

    public CartService(IHttpContextAccessor http)
    {
        _http = http;
    }

    private ISession Session => _http.HttpContext?.Session
        ?? throw new InvalidOperationException("La session HTTP n'est pas disponible.");

    private List<CartItem> Load()
    {
        if (_cache is not null) return _cache;

        var raw = Session.GetString(SessionKey);
        if (string.IsNullOrWhiteSpace(raw))
        {
            _cache = new List<CartItem>();
            return _cache;
        }

        try
        {
            _cache = JsonSerializer.Deserialize<List<CartItem>>(raw) ?? new List<CartItem>();
        }
        catch
        {
            _cache = new List<CartItem>();
        }

        return _cache;
    }

    private void Persist()
    {
        Session.SetString(SessionKey, JsonSerializer.Serialize(Load()));
    }

    public IReadOnlyList<CartItem> Items => Load();

    public int Count => Load().Sum(i => i.Quantity);

    public decimal Subtotal => Load().Sum(i => i.LineTotal);

    public CartItem? GetItem(int productId) => Load().FirstOrDefault(i => i.ProductId == productId);

    public bool Add(Product product, int quantity = 1)
    {
        if (product.QteStock <= 0 || quantity <= 0) return false;

        var items = Load();
        var existing = items.FirstOrDefault(i => i.ProductId == product.ProductId);

        if (existing is not null)
        {
            if (existing.Quantity >= product.QteStock) return false;
            existing.Quantity = Math.Min(existing.Quantity + quantity, product.QteStock);
            existing.Stock = product.QteStock;
        }
        else
        {
            items.Add(new CartItem
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Image = product.Image,
                CategoryName = product.Category?.CategoryName ?? string.Empty,
                Price = product.Price,
                OldPrice = product.OldPrice,
                Stock = product.QteStock,
                Quantity = Math.Min(quantity, product.QteStock)
            });
        }

        Persist();
        return true;
    }

    public bool Increment(int productId)
    {
        var item = GetItem(productId);
        if (item is null) return false;
        if (item.Quantity >= item.Stock) return false;

        item.Quantity++;
        Persist();
        return true;
    }

    public bool Decrement(int productId)
    {
        var item = GetItem(productId);
        if (item is null) return false;

        if (item.Quantity <= 1)
        {
            return Remove(productId);
        }

        item.Quantity--;
        Persist();
        return true;
    }

    public bool Remove(int productId)
    {
        var items = Load();
        var item = items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null) return false;

        items.Remove(item);
        Persist();
        return true;
    }

    public void Clear()
    {
        Load().Clear();
        Session.Remove(SessionKey);
        _cache = new List<CartItem>();
    }
}
