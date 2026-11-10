using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TP_Product.ViewModels;

namespace TP_Product.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly RoleManager<IdentityRole> roleManager;
    private readonly UserManager<IdentityUser> userManager;

    public AdminController(
        RoleManager<IdentityRole> roleManager,
        UserManager<IdentityUser> userManager)
    {
        this.roleManager = roleManager;
        this.userManager = userManager;
    }

    public async Task<IActionResult> ListRoles()
    {
        var roles = roleManager.Roles.OrderBy(r => r.Name).ToList();
        var counts = new Dictionary<string, int>();

        foreach (var role in roles)
        {
            if (!string.IsNullOrEmpty(role.Name))
                counts[role.Name] = (await userManager.GetUsersInRoleAsync(role.Name)).Count;
        }

        ViewBag.UserCounts = counts;
        return View(roles);
    }

    [HttpGet]
    public IActionResult CreateRole()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var role = new IdentityRole { Name = model.RoleName.Trim() };
        var result = await roleManager.CreateAsync(role);

        if (result.Succeeded)
        {
            TempData["Success"] = $"Le rôle « {role.Name} » a été créé.";
            return RedirectToAction(nameof(ListRoles));
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EditRole(string id)
    {
        var role = await roleManager.FindByIdAsync(id);
        if (role is null)
        {
            ViewBag.ErrorMessage = $"Le rôle « {id} » est introuvable.";
            return View("NotFound");
        }

        var model = new EditRoleViewModel
        {
            Id = role.Id,
            RoleName = role.Name ?? string.Empty
        };

        foreach (var user in userManager.Users.ToList())
        {
            if (await userManager.IsInRoleAsync(user, role.Name!))
                model.Users.Add(user.UserName ?? user.Email ?? user.Id);
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRole(EditRoleViewModel model)
    {
        var role = await roleManager.FindByIdAsync(model.Id);
        if (role is null)
        {
            ViewBag.ErrorMessage = $"Le rôle « {model.Id} » est introuvable.";
            return View("NotFound");
        }

        if (role.Name == "Admin" && !string.Equals(model.RoleName, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(model.RoleName), "Le rôle administrateur ne peut pas être renommé.");
            return View(model);
        }

        if (!ModelState.IsValid)
            return View(model);

        role.Name = model.RoleName.Trim();
        var result = await roleManager.UpdateAsync(role);

        if (result.Succeeded)
        {
            TempData["Success"] = "Le rôle a été mis à jour.";
            return RedirectToAction(nameof(ListRoles));
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRole(string id)
    {
        var role = await roleManager.FindByIdAsync(id);
        if (role is null)
        {
            ViewBag.ErrorMessage = $"Le rôle « {id} » est introuvable.";
            return View("NotFound");
        }

        if (role.Name == "Admin")
        {
            TempData["Error"] = "Le rôle Administrateur ne peut pas être supprimé.";
            return RedirectToAction(nameof(ListRoles));
        }

        var result = await roleManager.DeleteAsync(role);
        if (result.Succeeded)
        {
            TempData["Success"] = "Le rôle a été supprimé.";
            return RedirectToAction(nameof(ListRoles));
        }

        TempData["Error"] = string.Join(" ", result.Errors.Select(e => e.Description));
        return RedirectToAction(nameof(ListRoles));
    }

    [HttpGet]
    public async Task<IActionResult> EditUsersInRole(string roleId)
    {
        var role = await roleManager.FindByIdAsync(roleId);
        if (role is null)
        {
            ViewBag.ErrorMessage = $"Le rôle « {roleId} » est introuvable.";
            return View("NotFound");
        }

        ViewBag.RoleId = roleId;
        ViewBag.RoleName = role.Name;

        var model = new List<UserRoleViewModel>();
        foreach (var user in userManager.Users.ToList())
        {
            model.Add(new UserRoleViewModel
            {
                UserId = user.Id,
                UserName = user.UserName ?? user.Email ?? user.Id,
                IsSelected = await userManager.IsInRoleAsync(user, role.Name!)
            });
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUsersInRole(List<UserRoleViewModel> model, string roleId)
    {
        var role = await roleManager.FindByIdAsync(roleId);
        if (role is null)
        {
            ViewBag.ErrorMessage = $"Le rôle « {roleId} » est introuvable.";
            return View("NotFound");
        }

        for (var i = 0; i < model.Count; i++)
        {
            var user = await userManager.FindByIdAsync(model[i].UserId);
            if (user is null) continue;

            var isInRole = await userManager.IsInRoleAsync(user, role.Name!);

            if (model[i].IsSelected && !isInRole)
                await userManager.AddToRoleAsync(user, role.Name!);
            else if (!model[i].IsSelected && isInRole)
                await userManager.RemoveFromRoleAsync(user, role.Name!);
        }

        TempData["Success"] = "Les utilisateurs du rôle ont été mis à jour.";
        return RedirectToAction(nameof(EditRole), new { id = roleId });
    }

    public async Task<IActionResult> Users()
    {
        var users = userManager.Users.OrderBy(u => u.Email).ToList();
        var model = new List<AdminUserItem>();

        foreach (var user in users)
        {
            model.Add(new AdminUserItem
            {
                Id = user.Id,
                Email = user.Email ?? user.UserName ?? user.Id,
                Roles = (await userManager.GetRolesAsync(user)).ToList()
            });
        }

        return View(model);
    }
}
