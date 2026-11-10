using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TP_Product.Models;
using TP_Product.Repositories;
using TP_Product.ViewModels;

namespace TP_Product.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly IOrderRepository orderRepository;

    public OrdersController(IOrderRepository orderRepository)
    {
        this.orderRepository = orderRepository;
    }

    public IActionResult MyOrders()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return View(orderRepository.GetByUser(userId));
    }

    public IActionResult Details(int id)
    {
        var order = orderRepository.GetById(id);
        if (order is null) return NotFound();

        if (!CanAccess(order)) return Forbid();
        return View(order);
    }

    public IActionResult Invoice(int id)
    {
        var order = orderRepository.GetById(id);
        if (order is null) return NotFound();

        if (!CanAccess(order)) return Forbid();
        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Cancel(int id)
    {
        var order = orderRepository.GetById(id);
        if (order is null) return NotFound();

        if (!CanAccess(order)) return Forbid();

        if (order.Status is not (OrderStatuses.Pending or OrderStatuses.Confirmed))
        {
            TempData["Error"] = "Cette commande ne peut plus être annulée.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (orderRepository.UpdateStatus(id, OrderStatuses.Cancelled, out var error))
            TempData["Success"] = "Votre commande a été annulée.";
        else
            TempData["Error"] = error ?? "Impossible d'annuler la commande.";

        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Admin,Manager")]
    public IActionResult Manage(string? status)
    {
        var all = orderRepository.Query()
            .Include(o => o.Items)
            .Include(o => o.User)
            .AsNoTracking()
            .ToList();

        var filtered = string.IsNullOrEmpty(status) || status == "all"
            ? all
            : all.Where(o => o.Status == status).ToList();

        var model = new ManageOrdersViewModel
        {
            Orders = filtered.OrderByDescending(o => o.OrderDate).ToList(),
            Status = status,
            Total = all.Count,
            Pending = all.Count(o => o.Status == OrderStatuses.Pending),
            Delivered = all.Count(o => o.Status == OrderStatuses.Delivered),
            Cancelled = all.Count(o => o.Status == OrderStatuses.Cancelled)
        };

        return View(model);
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateStatus(int orderId, string status, string? returnUrl)
    {
        if (orderRepository.UpdateStatus(orderId, status, out var error))
            TempData["Success"] = $"Statut de la commande #{orderId} mis à jour : {status}.";
        else
            TempData["Error"] = error ?? "Impossible de modifier le statut.";

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction(nameof(Manage));
    }

    private bool CanAccess(Order order)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return order.UserId == userId || User.IsInRole("Admin") || User.IsInRole("Manager");
    }
}
