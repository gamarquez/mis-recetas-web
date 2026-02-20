using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MisRecetas.Web.Models;
using MisRecetas.Web.Services;
using MisRecetas.Web.ViewModels;

namespace MisRecetas.Web.Controllers;

[Authorize(Policy = "AdminOAdministracion")]
public class MedicosController : Controller
{
    private readonly MedicoService _medicoService;

    public MedicosController(MedicoService medicoService)
        => _medicoService = medicoService;

    public IActionResult Index()
        => View(_medicoService.ObtenerTodos());

    [HttpGet]
    public IActionResult Crear()
        => View(new MedicoViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Crear(MedicoViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        _medicoService.Insertar(new Medico
        {
            Nro_Matricula   = vm.Nro_Matricula.Trim(),
            Nombre_Medico   = vm.Nombre_Medico.Trim(),
            Apellido_Medico = vm.Apellido_Medico.Trim()
        });

        TempData["Success"] = $"Médico {vm.Apellido_Medico}, {vm.Nombre_Medico} creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Editar(int id)
    {
        var medico = _medicoService.ObtenerPorId(id);
        if (medico is null) return NotFound();

        return View(new MedicoViewModel
        {
            Id_Medico       = medico.Id_Medico,
            Nro_Matricula   = medico.Nro_Matricula,
            Nombre_Medico   = medico.Nombre_Medico,
            Apellido_Medico = medico.Apellido_Medico
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Editar(MedicoViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        _medicoService.Actualizar(new Medico
        {
            Id_Medico       = vm.Id_Medico,
            Nro_Matricula   = vm.Nro_Matricula.Trim(),
            Nombre_Medico   = vm.Nombre_Medico.Trim(),
            Apellido_Medico = vm.Apellido_Medico.Trim()
        });

        TempData["Success"] = "Médico actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
