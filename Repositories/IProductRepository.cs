using TP_Product.Models;
using TP_Product.Models.Query;

namespace TP_Product.Repositories;

public interface IProductRepository
{
    IQueryable<Product> GetAllProducts();
    IList<Product> GetAll();
    Product? GetById(int id);
    Product? GetByIdWithReviews(int id);
    void Add(Product product);
    Product? Update(Product product);
    void Delete(int id);
    IList<Product> FindByName(string name);
    IList<Product> GetProductsByCategID(int? categId);
    IList<Product> GetFeatured(int count);
    IList<Product> GetNewArrivals(int count);
    PagedResult<Product> GetPaged(ProductFilter filter);
    IList<Product> GetRelated(int productId, int categoryId, int count);
    IList<Product> GetLowStock(int threshold, int count);
    IList<Product> GetTopRated(int count);
    IList<TopProduct> GetTopSelling(int count);
    void RecalculateRating(int productId);
    int Count();
}
