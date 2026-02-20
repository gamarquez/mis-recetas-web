using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MisRecetas.Web.Services;
using MisRecetas.Web.ViewModels;

namespace MisRecetas.Web.Controllers;

public class AccountController : Controller
{
    private readonly AuthService _authService;

    public AccountController(AuthService authService)
        => _authService = authService;

    // ── GET /Account/Login ─────────────────────────────────────────────────
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        ViewBag.ReturnUrl = returnUrl;
        return View(new LoginViewModel());
    }

    // ── POST /Account/Login ────────────────────────────────────────────────
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);

        var usuario = _authService.ValidarCredenciales(model.NombreUsuario, model.Password);

        if (usuario is null)
        {
            ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
            return View(model);
        }

        await _authService.SignInAsync(usuario);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Dashboard");
    }

    // ── POST /Account/Logout ───────────────────────────────────────────────
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _authService.SignOutAsync();
        return RedirectToAction("Login");
    }

    // ── GET /Account/AccesoDenegado ────────────────────────────────────────
    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccesoDenegado()
        => View();

    // ── GET /Account/CambiarPassword ───────────────────────────────────────
    [HttpGet]
    [Authorize]
    public IActionResult CambiarPassword()
        => View(new CambiarPasswordViewModel());

    // ── POST /Account/CambiarPassword ─────────────────────────────────────
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public IActionResult CambiarPassword(CambiarPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        int idUsuario = GetCurrentUserId();
        bool ok = _authService.CambiarPassword(idUsuario, model.PasswordActual, model.NuevaPassword);

        if (!ok)
        {
            ModelState.AddModelError(nameof(model.PasswordActual), "La contraseña actual es incorrecta.");
            return View(model);
        }

        TempData["Success"] = "Contraseña cambiada correctamente.";
        return RedirectToAction("Index", "Dashboard");
    }

    // ── Helper ─────────────────────────────────────────────────────────────
    private int GetCurrentUserId()
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
