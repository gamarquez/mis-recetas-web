using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MisRecetas.Web.Models;
using MisRecetas.Web.Services;
using MisRecetas.Web.ViewModels;

namespace MisRecetas.Web.Controllers;

[Authorize(Policy = "SoloAdmin")]
public class UsuariosController : Controller
{
    private readonly UsuarioService _usuarioService;

    public UsuariosController(UsuarioService usuarioService)
        => _usuarioService = usuarioService;

    public IActionResult Index()
        => View(_usuarioService.ObtenerTodos());

    [HttpGet]
    public IActionResult Crear()
        => View(new UsuarioViewModel { ComboRoles = ObtenerComboRoles() });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Crear(UsuarioViewModel vm)
    {
        if (string.IsNullOrWhiteSpace(vm.Password))
            ModelState.AddModelError(nameof(vm.Password), "La contraseña es obligatoria al crear un usuario.");

        if (!ModelState.IsValid)
        {
            vm.ComboRoles = ObtenerComboRoles();
            return View(vm);
        }

        var (ok, error) = _usuarioService.Insertar(new Usuario
        {
            Nombre_Usuario = vm.Nombre_Usuario.Trim(),
            Nombre         = vm.Nombre.Trim(),
            Apellido       = vm.Apellido.Trim(),
            Id_Rol         = vm.Id_Rol
        }, vm.Password!);

        if (!ok)
        {
            ModelState.AddModelError("", error);
            vm.ComboRoles = ObtenerComboRoles();
            return View(vm);
        }

        TempData["Success"] = $"Usuario '{vm.Nombre_Usuario}' creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Editar(int id)
    {
        var u = _usuarioService.ObtenerPorId(id);
        if (u is null) return NotFound();

        return View(new UsuarioViewModel
        {
            Id_Usuario     = u.Id_Usuario,
            Nombre_Usuario = u.Nombre_Usuario,
            Nombre         = u.Nombre,
            Apellido       = u.Apellido,
            Id_Rol         = u.Id_Rol,
            ComboRoles     = ObtenerComboRoles()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Editar(UsuarioViewModel vm)
    {
        ModelState.Remove(nameof(vm.Password));
        ModelState.Remove(nameof(vm.ConfirmarPassword));

        if (!ModelState.IsValid)
        {
            vm.ComboRoles = ObtenerComboRoles();
            return View(vm);
        }

        var (ok, error) = _usuarioService.Actualizar(new Usuario
        {
            Id_Usuario     = vm.Id_Usuario,
            Nombre_Usuario = vm.Nombre_Usuario.Trim(),
            Nombre         = vm.Nombre.Trim(),
            Apellido       = vm.Apellido.Trim(),
            Id_Rol         = vm.Id_Rol
        });

        if (!ok)
        {
            ModelState.AddModelError("", error);
            vm.ComboRoles = ObtenerComboRoles();
            return View(vm);
        }

        TempData["Success"] = $"Usuario '{vm.Nombre_Usuario}' actualizado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult ResetPassword(int id)
    {
        var u = _usuarioService.ObtenerPorId(id);
        if (u is null) return NotFound();
        ViewBag.NombreUsuario = u.Nombre_Usuario;
        ViewBag.IdUsuario = id;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ResetPassword(int id, string nuevaPassword, string confirmarPassword)
    {
        if (string.IsNullOrWhiteSpace(nuevaPassword) || nuevaPassword.Length < 6)
        {
            TempData["Error"] = "La contraseña debe tener al menos 6 caracteres.";
            return RedirectToAction(nameof(ResetPassword), new { id });
        }

        if (nuevaPassword != confirmarPassword)
        {
            TempData["Error"] = "Las contraseñas no coinciden.";
            return RedirectToAction(nameof(ResetPassword), new { id });
        }

        _usuarioService.ResetPassword(id, nuevaPassword);
        TempData["Success"] = "Contraseña reseteada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    private List<SelectListItem> ObtenerComboRoles()
        => _usuarioService.ObtenerRoles()
            .Select(r => new SelectListItem(r.Descripcion, r.Id_Rol.ToString()))
            .ToList();
}
