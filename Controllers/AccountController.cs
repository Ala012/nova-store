using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TP_Product.Repositories;
using TP_Product.ViewModels;

namespace TP_Product.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> userManager;
    private readonly SignInManager<IdentityUser> signInManager;
    private readonly IOrderRepository orderRepository;
    private readonly IWishlistRepository wishlistRepository;

    public AccountController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IOrderRepository orderRepository,
        IWishlistRepository wishlistRepository)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.orderRepository = orderRepository;
        this.wishlistRepository = wishlistRepository;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;

        if (!model.AcceptTerms)
            ModelState.AddModelError(nameof(model.AcceptTerms), "Vous devez accepter les conditions d'utilisation.");

        if (!ModelState.IsValid)
            return View(model);

        var user = new IdentityUser
        {
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "User");
            await signInManager.SignInAsync(user, isPersistent: false);

            TempData["Success"] = "Bienvenue chez Nova ! Votre compte a été créé.";
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, TranslateIdentityError(error.Description));

        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;

        if (!ModelState.IsValid)
            return View(model);

        var result = await signInManager.PasswordSignInAsync(
            model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            TempData["Success"] = "Ravi de vous revoir !";
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "E-mail ou mot de passe incorrect.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        TempData["Success"] = "Vous êtes déconnecté. À bientôt !";
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        return View(await BuildProfileAsync());
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword([Bind(Prefix = "ChangePassword")] ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Merci de vérifier les champs du mot de passe.";
            return RedirectToAction(nameof(Profile));
        }

        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (result.Succeeded)
        {
            await signInManager.RefreshSignInAsync(user);
            TempData["Success"] = "Votre mot de passe a été mis à jour.";
        }
        else
        {
            TempData["Error"] = string.Join(" ", result.Errors.Select(e => TranslateIdentityError(e.Description)));
        }

        return RedirectToAction(nameof(Profile));
    }

    private async Task<ProfileViewModel> BuildProfileAsync()
    {
        var user = await userManager.GetUserAsync(User)
            ?? throw new InvalidOperationException("Utilisateur introuvable.");

        var roles = await userManager.GetRolesAsync(user);
        var orders = orderRepository.GetByUser(user.Id);

        return new ProfileViewModel
        {
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            RoleLabel = roles.Contains("Admin") ? "Administrateur" : roles.Contains("Manager") ? "Manager" : "Client",
            OrdersCount = orders.Count,
            WishlistCount = wishlistRepository.Count(user.Id),
            MemberSince = orders.Count > 0 ? orders.Min(o => o.OrderDate) : DateTime.Now
        };
    }

    private static string TranslateIdentityError(string message)
    {
        if (message.Contains("already taken", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
            return "Un compte existe déjà avec cette adresse e-mail.";

        if (message.Contains("password", StringComparison.OrdinalIgnoreCase))
            return "Le mot de passe ne respecte pas les critères de sécurité.";

        return message;
    }
}
