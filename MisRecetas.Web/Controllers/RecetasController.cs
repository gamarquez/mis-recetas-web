using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MisRecetas.Web.Data;
using MisRecetas.Web.Models;
using MisRecetas.Web.Services;
using MisRecetas.Web.ViewModels;

namespace MisRecetas.Web.Controllers;

[Authorize]
public class RecetasController : Controller
{
    private readonly RecetaService _recetaService;
    private readonly MedicoService _medicoService;
    private readonly PacienteService _pacienteService;
    private readonly CatalogoRepository _catalogo;

    public RecetasController(
        RecetaService recetaService,
        MedicoService medicoService,
        PacienteService pacienteService,
        CatalogoRepository catalogo)
    {
        _recetaService = recetaService;
        _medicoService = medicoService;
        _pacienteService = pacienteService;
        _catalogo = catalogo;
    }

    public IActionResult Index() => RedirectToAction(nameof(Buscar));

    // ══════════════════════════════════════════════════════════════════════
    // BUSCAR
    // ══════════════════════════════════════════════════════════════════════

    [HttpGet]
    public IActionResult Buscar()
        => View(new BuscarRecetaViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Buscar(BuscarRecetaViewModel vm)
    {
        vm.Resultados = _recetaService.Buscar(vm.NroReceta, vm.Nombre, vm.Apellido, vm.Documento);
        vm.BusquedaRealizada = true;

        if (vm.Resultados.Count == 0)
            TempData["Warning"] = "No se encontraron recetas con los criterios ingresados.";

        return View(vm);
    }

    // ══════════════════════════════════════════════════════════════════════
    // REGISTRAR — Flujo de dos pasos
    // ══════════════════════════════════════════════════════════════════════

    [HttpGet]
    public IActionResult Registrar()
    {
        var vm = BuildRegistrarVm();
        vm.NroReceta = _recetaService.SiguienteNroReceta();
        return View(vm);
    }

    /// <summary>Paso 1 — Buscar paciente por tipo+número de documento.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult BuscarPaciente(RegistrarRecetaViewModel vm)
    {
        // Limpiar errores de campos del paso 2 que no aplican aquí
        foreach (var key in new[] { nameof(vm.NombrePaciente), nameof(vm.ApellidoPaciente),
                                    nameof(vm.EmailPaciente), nameof(vm.IdMedico), nameof(vm.NroReceta) })
            ModelState.Remove(key);

        if (vm.IdTipoDocumento == 0 || string.IsNullOrWhiteSpace(vm.NroDocumento))
        {
            ModelState.AddModelError("", "Seleccione un tipo de documento e ingrese el número.");
            PopularCombos(vm);
            return View("Registrar", vm);
        }

        var paciente = _pacienteService.BuscarPorDocumento(vm.IdTipoDocumento, vm.NroDocumento.Trim());

        if (paciente is not null)
        {
            vm.IdPacienteEncontrado = paciente.Id_Paciente;
            vm.NombrePaciente = paciente.Nombre_Paciente;
            vm.ApellidoPaciente = paciente.Apellido_Paciente;
            vm.EmailPaciente = paciente.Email;
            vm.PacienteNuevo = false;
            vm.MensajeBusqueda = $"Paciente encontrado: {paciente.NombreCompleto}";
        }
        else
        {
            vm.IdPacienteEncontrado = null;
            vm.PacienteNuevo = true;
            vm.MensajeBusqueda = "Paciente no encontrado. Complete los datos para registrarlo.";
        }

        vm.Paso = 2;
        vm.NroReceta = _recetaService.SiguienteNroReceta();
        PopularCombos(vm);
        return View("Registrar", vm);
    }

    /// <summary>Paso 2 — Confirmar y registrar receta (crea paciente si es nuevo).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ConfirmarRegistro(RegistrarRecetaViewModel vm)
    {
        if (string.IsNullOrWhiteSpace(vm.NombrePaciente))
            ModelState.AddModelError(nameof(vm.NombrePaciente), "El nombre es obligatorio.");
        if (string.IsNullOrWhiteSpace(vm.ApellidoPaciente))
            ModelState.AddModelError(nameof(vm.ApellidoPaciente), "El apellido es obligatorio.");
        if (vm.IdMedico == 0)
            ModelState.AddModelError(nameof(vm.IdMedico), "Seleccione un médico.");
        if (vm.NroReceta <= 0)
            ModelState.AddModelError(nameof(vm.NroReceta), "El número de receta es inválido.");

        if (!ModelState.IsValid)
        {
            vm.Paso = 2;
            PopularCombos(vm);
            return View("Registrar", vm);
        }

        int idPaciente;

        if (vm.IdPacienteEncontrado.HasValue)
        {
            idPaciente = vm.IdPacienteEncontrado.Value;
        }
        else
        {
            var nuevoPaciente = new Paciente
            {
                Id_TipoDocumento = vm.IdTipoDocumento,
                Nro_Documento    = vm.NroDocumento.Trim(),
                Nombre_Paciente  = vm.NombrePaciente.Trim(),
                Apellido_Paciente = vm.ApellidoPaciente.Trim(),
                Email = vm.EmailPaciente?.Trim() ?? string.Empty
            };
            idPaciente = _pacienteService.Insertar(nuevoPaciente);
        }

        _recetaService.Registrar(vm.NroReceta, vm.IdMedico, idPaciente, GetCurrentUserId());

        TempData["Success"] = $"Receta Nro. {vm.NroReceta} registrada correctamente.";
        TempData["NroRecetaRegistrada"] = vm.NroReceta;
        return RedirectToAction(nameof(Registrar));
    }

    // ══════════════════════════════════════════════════════════════════════
    // ENTREGAR A MÉDICO  (Estado 1 → 2)
    // ══════════════════════════════════════════════════════════════════════

    [HttpGet]
    public IActionResult EntregarAMedico(int? idMedico = null)
    {
        var vm = new EntregarAMedicoViewModel
        {
            IdMedicoFiltro = idMedico,
            ComboMedicos = ComboMedicos(conOpcionTodos: true)
        };

        vm.Recetas = (idMedico.HasValue && idMedico > 0)
            ? _recetaService.ObtenerPorMedicoYEstado(idMedico.Value, 1)
            : _recetaService.ObtenerPendientes();

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EntregarAMedico(EntregarAMedicoViewModel vm)
    {
        if (vm.NrosSeleccionados is null || vm.NrosSeleccionados.Count == 0)
        {
            TempData["Warning"] = "Seleccione al menos una receta.";
            return RedirectToAction(nameof(EntregarAMedico));
        }

        _recetaService.EntregarAMedico(vm.NrosSeleccionados, GetCurrentUserId());
        TempData["Success"] = $"{vm.NrosSeleccionados.Count} receta(s) entregada(s) al médico.";
        return RedirectToAction(nameof(EntregarAMedico));
    }

    // ══════════════════════════════════════════════════════════════════════
    // RECIBIR DE MÉDICO  (Estado 2 → 3)
    // ══════════════════════════════════════════════════════════════════════

    [HttpGet]
    public IActionResult RecibirDeMedico(int? idMedico = null)
    {
        var vm = new RecibirDeMedicoViewModel
        {
            IdMedicoFiltro = idMedico,
            ComboMedicos = ComboMedicos(conOpcionTodos: true)
        };

        vm.Recetas = (idMedico.HasValue && idMedico > 0)
            ? _recetaService.ObtenerPorMedicoYEstado(idMedico.Value, 2)
            : _recetaService.ObtenerConMedico();

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RecibirDeMedico(RecibirDeMedicoViewModel vm)
    {
        if (vm.NrosSeleccionados is null || vm.NrosSeleccionados.Count == 0)
        {
            TempData["Warning"] = "Seleccione al menos una receta.";
            return RedirectToAction(nameof(RecibirDeMedico));
        }

        _recetaService.RecibirDeMedico(vm.NrosSeleccionados, GetCurrentUserId());
        TempData["Success"] = $"{vm.NrosSeleccionados.Count} receta(s) recibida(s) del médico.";
        return RedirectToAction(nameof(RecibirDeMedico));
    }

    // ══════════════════════════════════════════════════════════════════════
    // ENTREGAR A PACIENTE  (Estado 3 → 4)
    // ══════════════════════════════════════════════════════════════════════

    [HttpGet]
    public IActionResult EntregarAPaciente()
    {
        var vm = new EntregarAPacienteViewModel
        {
            Recetas = _recetaService.ObtenerListasParaEntregar()
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EntregarAPaciente(EntregarAPacienteViewModel vm)
    {
        if (vm.NroRecetaSeleccionada <= 0)
        {
            TempData["Warning"] = "Seleccione una receta.";
            vm.Recetas = _recetaService.ObtenerListasParaEntregar();
            return View(vm);
        }

        if (!ModelState.IsValid)
        {
            vm.Recetas = _recetaService.ObtenerListasParaEntregar();
            return View(vm);
        }

        _recetaService.EntregarAPaciente(
            vm.NroRecetaSeleccionada,
            GetCurrentUserId(),
            vm.NombreRetira.Trim(),
            vm.Parentezco.Trim());

        TempData["Success"] = $"Receta Nro. {vm.NroRecetaSeleccionada} entregada al paciente.";
        return RedirectToAction(nameof(EntregarAPaciente));
    }

    // ══════════════════════════════════════════════════════════════════════
    // ARCHIVAR  (Estado 3 con 7+ días → 5)
    // ══════════════════════════════════════════════════════════════════════

    [HttpGet]
    [Authorize(Policy = "AdminOAdministracion")]
    public IActionResult Archivar()
    {
        var vm = new ArchivarRecetasViewModel
        {
            Recetas = _recetaService.ObtenerParaArchivar()
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "AdminOAdministracion")]
    public IActionResult Archivar(ArchivarRecetasViewModel vm)
    {
        if (vm.NrosSeleccionados is null || vm.NrosSeleccionados.Count == 0)
        {
            TempData["Warning"] = "Seleccione al menos una receta.";
            return RedirectToAction(nameof(Archivar));
        }

        _recetaService.Archivar(vm.NrosSeleccionados, GetCurrentUserId());
        TempData["Success"] = $"{vm.NrosSeleccionados.Count} receta(s) archivada(s).";
        return RedirectToAction(nameof(Archivar));
    }

    // ══════════════════════════════════════════════════════════════════════
    // ENTREGAR ARCHIVADAS  (Estado 5 → 4)
    // ══════════════════════════════════════════════════════════════════════

    [HttpGet]
    [Authorize(Policy = "AdminOAdministracion")]
    public IActionResult EntregarArchivadas()
    {
        var vm = new EntregarArchivadasViewModel
        {
            Recetas = _recetaService.ObtenerArchivadas()
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "AdminOAdministracion")]
    public IActionResult EntregarArchivadas(EntregarArchivadasViewModel vm)
    {
        if (vm.NroRecetaSeleccionada <= 0)
        {
            TempData["Warning"] = "Seleccione una receta.";
            vm.Recetas = _recetaService.ObtenerArchivadas();
            return View(vm);
        }

        if (!ModelState.IsValid)
        {
            vm.Recetas = _recetaService.ObtenerArchivadas();
            return View(vm);
        }

        _recetaService.EntregarArchivada(
            vm.NroRecetaSeleccionada,
            GetCurrentUserId(),
            vm.NombreRetira.Trim(),
            vm.Parentezco.Trim());

        TempData["Success"] = $"Receta archivada Nro. {vm.NroRecetaSeleccionada} entregada al paciente.";
        return RedirectToAction(nameof(EntregarArchivadas));
    }

    // ══════════════════════════════════════════════════════════════════════
    // DESECHAR  (Estado 5 → 6)
    // ══════════════════════════════════════════════════════════════════════

    [HttpGet]
    [Authorize(Policy = "AdminOAdministracion")]
    public IActionResult Desechar()
    {
        var vm = new DesecharRecetasViewModel
        {
            Recetas = _recetaService.ObtenerArchivadas()
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "AdminOAdministracion")]
    public IActionResult Desechar(DesecharRecetasViewModel vm)
    {
        if (vm.NrosSeleccionados is null || vm.NrosSeleccionados.Count == 0)
        {
            TempData["Warning"] = "Seleccione al menos una receta.";
            return RedirectToAction(nameof(Desechar));
        }

        _recetaService.Desechar(vm.NrosSeleccionados, GetCurrentUserId());
        TempData["Success"] = $"{vm.NrosSeleccionados.Count} receta(s) desechada(s).";
        return RedirectToAction(nameof(Desechar));
    }

    // ══════════════════════════════════════════════════════════════════════
    // MODIFICAR  (Solo Admin)
    // ══════════════════════════════════════════════════════════════════════

    [HttpGet]
    [Authorize(Policy = "SoloAdmin")]
    public IActionResult Modificar(string? nro = null)
    {
        var vm = new ModificarRecetaViewModel
        {
            NroRecetaBuscar = nro,
            ComboMedicos = ComboMedicos(),
            ComboEstados = ComboEstados()
        };

        if (!string.IsNullOrWhiteSpace(nro))
        {
            vm.RecetaActual = _recetaService.Buscar(nro, null, null, null).FirstOrDefault();
            if (vm.RecetaActual is not null)
            {
                vm.IdMedico = vm.RecetaActual.Id_Medico;
                vm.IdEstado = vm.RecetaActual.Id_Estado;
            }
            else
            {
                TempData["Warning"] = $"No se encontró la receta Nro. {nro}.";
            }
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "SoloAdmin")]
    public IActionResult Modificar(ModificarRecetaViewModel vm)
    {
        if (vm.RecetaActual is null)
        {
            vm.ComboMedicos = ComboMedicos();
            vm.ComboEstados = ComboEstados();
            return View(vm);
        }

        _recetaService.ModificarMedicoYEstado(vm.RecetaActual.Nro_Receta, vm.IdMedico, vm.IdEstado);
        TempData["Success"] = $"Receta Nro. {vm.RecetaActual.Nro_Receta} modificada correctamente.";
        return RedirectToAction(nameof(Modificar));
    }

    // ══════════════════════════════════════════════════════════════════════
    // Helpers
    // ══════════════════════════════════════════════════════════════════════

    private int GetCurrentUserId()
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private RegistrarRecetaViewModel BuildRegistrarVm()
    {
        var vm = new RegistrarRecetaViewModel();
        PopularCombos(vm);
        return vm;
    }

    private void PopularCombos(RegistrarRecetaViewModel vm)
    {
        vm.ComboMedicos = ComboMedicos();
        vm.ComboTipoDocumento = _pacienteService
            .ObtenerTiposDocumento()
            .Select(t => new SelectListItem(t.Descripcion, t.Id_TipoDocumento.ToString()))
            .ToList();
    }

    private List<SelectListItem> ComboMedicos(bool conOpcionTodos = false)
    {
        var items = _medicoService.ObtenerCombo()
            .OrderBy(m => m.Apellido_Medico)
            .Select(m => new SelectListItem($"{m.Apellido_Medico}, {m.Nombre_Medico}", m.Id_Medico.ToString()))
            .ToList();

        if (conOpcionTodos)
            items.Insert(0, new SelectListItem("— Todos los médicos —", ""));

        return items;
    }

    private List<SelectListItem> ComboEstados()
        => _catalogo.ObtenerEstados()
            .Select(e => new SelectListItem(e.Descripcion, e.Id_Estado.ToString()))
            .ToList();
}
