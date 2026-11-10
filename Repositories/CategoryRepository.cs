using Microsoft.EntityFrameworkCore;
using TP_Product.Data;
using TP_Product.Models;

namespace TP_Product.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext context;

    public CategoryRepository(AppDbContext context)
    {
        this.context = context;
    }

    public IList<Category> GetAll()
    {
        return context.Categories
            .OrderBy(c => c.CategoryName)
            .ToList();
    }

    public Category? GetById(int id)
    {
        return context.Categories.Find(id);
    }

    public void Add(Category category)
    {
        context.Categories.Add(category);
        context.SaveChanges();
    }

    public Category? Update(Category category)
    {
        var existing = context.Categories.Find(category.CategoryId);
        if (existing is null) return null;

        existing.CategoryName = category.CategoryName;
        existing.Description = category.Description;
        existing.Icon = category.Icon;
        existing.Accent = category.Accent;
        context.SaveChanges();
        return existing;
    }

    public void Delete(int id)
    {
        var category = context.Categories.Find(id);
        if (category is null) return;

        context.Categories.Remove(category);
        context.SaveChanges();
    }

    public bool Exists(string name, int? exceptId = null)
    {
        var query = context.Categories.Where(c => c.CategoryName == name);
        if (exceptId.HasValue)
            query = query.Where(c => c.CategoryId != exceptId.Value);

        return query.Any();
    }

    public int CountProducts(int categoryId)
    {
        return context.Products.Count(p => p.CategoryId == categoryId);
    }
}
