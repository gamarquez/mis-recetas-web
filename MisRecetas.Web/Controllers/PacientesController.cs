using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MisRecetas.Web.Models;
using MisRecetas.Web.Services;
using MisRecetas.Web.ViewModels;

namespace MisRecetas.Web.Controllers;

[Authorize]
public class PacientesController : Controller
{
    private readonly PacienteService _pacienteService;

    public PacientesController(PacienteService pacienteService)
        => _pacienteService = pacienteService;

    public IActionResult Index() => RedirectToAction(nameof(Buscar));

    [HttpGet]
    public IActionResult Buscar(string? nombre, string? apellido)
    {
        var lista = new List<Paciente>();
        if (!string.IsNullOrWhiteSpace(nombre) || !string.IsNullOrWhiteSpace(apellido))
            lista = _pacienteService.BuscarPorNombreApellido(nombre ?? "", apellido ?? "");

        ViewBag.Nombre = nombre;
        ViewBag.Apellido = apellido;
        return View(lista);
    }

    [HttpGet]
    public IActionResult Crear()
        => View(new PacienteViewModel { ComboTipoDocumento = ObtenerComboTipos() });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Crear(PacienteViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.ComboTipoDocumento = ObtenerComboTipos();
            return View(vm);
        }

        _pacienteService.Insertar(new Paciente
        {
            Id_TipoDocumento  = vm.Id_TipoDocumento,
            Nro_Documento     = vm.Nro_Documento.Trim(),
            Nombre_Paciente   = vm.Nombre_Paciente.Trim(),
            Apellido_Paciente = vm.Apellido_Paciente.Trim(),
            Email             = vm.Email?.Trim() ?? string.Empty
        });

        TempData["Success"] = $"Paciente {vm.Apellido_Paciente}, {vm.Nombre_Paciente} creado correctamente.";
        return RedirectToAction(nameof(Buscar));
    }

    [HttpGet]
    public IActionResult Editar(int id)
    {
        var p = _pacienteService.ObtenerPorId(id);
        if (p is null) return NotFound();

        return View(new PacienteViewModel
        {
            Id_Paciente       = p.Id_Paciente,
            Id_TipoDocumento  = p.Id_TipoDocumento,
            Nro_Documento     = p.Nro_Documento,
            Nombre_Paciente   = p.Nombre_Paciente,
            Apellido_Paciente = p.Apellido_Paciente,
            Email             = p.Email,
            ComboTipoDocumento = ObtenerComboTipos()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Editar(PacienteViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.ComboTipoDocumento = ObtenerComboTipos();
            return View(vm);
        }

        _pacienteService.Actualizar(new Paciente
        {
            Id_Paciente       = vm.Id_Paciente,
            Id_TipoDocumento  = vm.Id_TipoDocumento,
            Nro_Documento     = vm.Nro_Documento.Trim(),
            Nombre_Paciente   = vm.Nombre_Paciente.Trim(),
            Apellido_Paciente = vm.Apellido_Paciente.Trim(),
            Email             = vm.Email?.Trim() ?? string.Empty
        });

        TempData["Success"] = "Paciente actualizado correctamente.";
        return RedirectToAction(nameof(Buscar));
    }

    private List<SelectListItem> ObtenerComboTipos()
        => _pacienteService.ObtenerTiposDocumento()
            .Select(t => new SelectListItem(t.Descripcion, t.Id_TipoDocumento.ToString()))
            .ToList();
}
