using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MisRecetas.Web.Data;
using MisRecetas.Web.Services;
using MisRecetas.Web.ViewModels;

namespace MisRecetas.Web.Controllers;

[Authorize]
public class ReportesController : Controller
{
    private readonly ReporteService _reporteService;
    private readonly MedicoService _medicoService;
    private readonly CatalogoRepository _catalogo;

    public ReportesController(
        ReporteService reporteService,
        MedicoService medicoService,
        CatalogoRepository catalogo)
    {
        _reporteService = reporteService;
        _medicoService  = medicoService;
        _catalogo       = catalogo;
    }

    public IActionResult Index() => View();

    // ── 1. Ticket de receta individual ────────────────────────────────────
    [HttpGet]
    public IActionResult Ticket(int? nro = null)
    {
        var vm = new ReporteViewModel { NroReceta = nro?.ToString() };

        if (nro is null) return View(vm);

        vm.BusquedaRealizada = true;
        var detalle = _reporteService.TicketReceta(nro.Value);
        if (detalle is not null)
            vm.Datos.Add(detalle);

        return View(vm);
    }

    // ── 2. Recetas por estado ─────────────────────────────────────────────
    [HttpGet]
    public IActionResult PorEstado(int? estado = null)
    {
        var vm = new ReporteViewModel
        {
            IdEstado     = estado,
            ComboEstados = ComboEstados()
        };

        if (estado.HasValue)
        {
            vm.Datos = _reporteService.RecetasPorEstado(estado.Value);
            vm.TituloReporte    = "Recetas por estado";
            vm.SubtituloReporte = vm.Datos.FirstOrDefault()?.DescripcionEstado ?? estado.ToString()!;
        }

        return View(vm);
    }

    // ── 3. Recetas archivadas ─────────────────────────────────────────────
    [HttpGet]
    public IActionResult Archivadas(DateTime? fechaDesde = null, DateTime? fechaHasta = null)
    {
        var todas = _reporteService.RecetasArchivadas();
        var vm = new ReporteViewModel
        {
            TituloReporte  = "Recetas archivadas",
            FechaDesde     = fechaDesde,
            FechaHasta     = fechaHasta,
            BusquedaRealizada = true,
            Datos = todas
                .Where(r => (fechaDesde == null || r.Fecha_Archivo >= fechaDesde)
                         && (fechaHasta == null || r.Fecha_Archivo <= fechaHasta.Value.AddDays(1)))
                .ToList()
        };
        return View(vm);
    }

    // ── 4. Recetas desechadas ─────────────────────────────────────────────
    [HttpGet]
    public IActionResult Desechadas(DateTime? fechaDesde = null, DateTime? fechaHasta = null)
    {
        var todas = _reporteService.RecetasDesechadas();
        var vm = new ReporteViewModel
        {
            TituloReporte  = "Recetas desechadas",
            FechaDesde     = fechaDesde,
            FechaHasta     = fechaHasta,
            BusquedaRealizada = true,
            Datos = todas
                .Where(r => (fechaDesde == null || r.Fecha_Desecho >= fechaDesde)
                         && (fechaHasta == null || r.Fecha_Desecho <= fechaHasta.Value.AddDays(1)))
                .ToList()
        };
        return View(vm);
    }

    // ── 5. Entregadas por médico ──────────────────────────────────────────
    [HttpGet]
    public IActionResult EntregadasPorMedico(int? idMedico = null)
    {
        var vm = new ReporteViewModel
        {
            IdMedico     = idMedico,
            ComboMedicos = ComboMedicos()
        };

        if (idMedico.HasValue)
        {
            vm.Datos = _reporteService.RecetasPorMedico(idMedico.Value, 2);
            var medico = _medicoService.ObtenerPorId(idMedico.Value);
            vm.TituloReporte    = "Recetas entregadas al médico";
            vm.SubtituloReporte = medico?.NombreCompleto ?? idMedico.ToString()!;
        }

        return View(vm);
    }

    // ── 6. Recibidas por médico ───────────────────────────────────────────
    [HttpGet]
    public IActionResult RecibidasPorMedico(int? idMedico = null)
    {
        var vm = new ReporteViewModel
        {
            IdMedico     = idMedico,
            ComboMedicos = ComboMedicos()
        };

        if (idMedico.HasValue)
        {
            vm.Datos = _reporteService.RecetasPorMedico(idMedico.Value, 3);
            var medico = _medicoService.ObtenerPorId(idMedico.Value);
            vm.TituloReporte    = "Recetas recibidas del médico";
            vm.SubtituloReporte = medico?.NombreCompleto ?? idMedico.ToString()!;
        }

        return View(vm);
    }

    // ── 7. Por médico y rango de fechas ───────────────────────────────────
    [HttpGet]
    public IActionResult PorMedicoYFecha(int? idMedico = null, DateTime? desde = null, DateTime? hasta = null)
    {
        var vm = new ReporteViewModel
        {
            IdMedico     = idMedico,
            FechaDesde   = desde,
            FechaHasta   = hasta,
            ComboMedicos = ComboMedicos()
        };

        if (idMedico.HasValue)
        {
            // Obtener recetas del médico en todos los estados y filtrar por fecha
            var todasEstados = new[] { 1, 2, 3, 4, 5, 6 }
                .SelectMany(e => _reporteService.RecetasPorMedico(idMedico.Value, e))
                .ToList();

            vm.Datos = todasEstados
                .Where(r => (desde == null || r.Fecha_Registro >= desde)
                         && (hasta == null || r.Fecha_Registro <= hasta.Value.AddDays(1)))
                .OrderBy(r => r.Fecha_Registro)
                .ToList();

            var medico = _medicoService.ObtenerPorId(idMedico.Value);
            vm.TituloReporte    = "Recetas por médico y período";
            vm.SubtituloReporte = $"{medico?.NombreCompleto} — {desde?.ToString("dd/MM/yyyy") ?? "inicio"} al {hasta?.ToString("dd/MM/yyyy") ?? "hoy"}";
        }

        return View(vm);
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    private List<SelectListItem> ComboMedicos()
        => _medicoService.ObtenerCombo()
            .OrderBy(m => m.Apellido_Medico)
            .Select(m => new SelectListItem($"{m.Apellido_Medico}, {m.Nombre_Medico}", m.Id_Medico.ToString()))
            .ToList();

    private List<SelectListItem> ComboEstados()
        => _catalogo.ObtenerEstados()
            .Select(e => new SelectListItem(e.Descripcion, e.Id_Estado.ToString()))
            .ToList();
}
