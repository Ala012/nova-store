using TP_Product.Models;

namespace TP_Product.Repositories;

public interface ICategoryRepository
{
    IList<Category> GetAll();
    Category? GetById(int id);
    void Add(Category category);
    Category? Update(Category category);
    void Delete(int id);
    bool Exists(string name, int? exceptId = null);
    int CountProducts(int categoryId);
}
