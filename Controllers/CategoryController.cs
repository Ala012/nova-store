using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TP_Product.Models;
using TP_Product.Repositories;
using TP_Product.ViewModels;

namespace TP_Product.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class CategoryController : Controller
{
    private readonly ICategoryRepository CategRepository;

    public CategoryController(ICategoryRepository categRepository)
    {
        CategRepository = categRepository;
    }

    [AllowAnonymous]
    public IActionResult Index()
    {
        var categories = CategRepository.GetAll()
            .Select(c => new CategoryCardItem
            {
                Category = c,
                ProductCount = CategRepository.CountProducts(c.CategoryId)
            })
            .ToList();

        return View(categories);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Category model)
    {
        if (CategRepository.Exists(model.CategoryName.Trim()))
            ModelState.AddModelError(nameof(model.CategoryName), "Une catégorie porte déjà ce nom.");

        if (!ModelState.IsValid)
            return View(model);

        model.CategoryName = model.CategoryName.Trim();
        CategRepository.Add(model);
        TempData["Success"] = $"La catégorie « {model.CategoryName} » a été créée.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var category = CategRepository.GetById(id);
        if (category is null) return NotFound();
        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Category model)
    {
        if (CategRepository.Exists(model.CategoryName.Trim(), model.CategoryId))
            ModelState.AddModelError(nameof(model.CategoryName), "Une catégorie porte déjà ce nom.");

        if (!ModelState.IsValid)
            return View(model);

        model.CategoryName = model.CategoryName.Trim();
        CategRepository.Update(model);
        TempData["Success"] = "La catégorie a été mise à jour.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int id)
    {
        var category = CategRepository.GetById(id);
        if (category is null) return NotFound();

        ViewBag.ProductCount = CategRepository.CountProducts(id);
        return View(category);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        var count = CategRepository.CountProducts(id);
        if (count > 0)
        {
            TempData["Error"] = $"Impossible de supprimer cette catégorie : {count} produit(s) y sont rattachés.";
            return RedirectToAction(nameof(Index));
        }

        CategRepository.Delete(id);
        TempData["Success"] = "La catégorie a été supprimée.";
        return RedirectToAction(nameof(Index));
    }
}
