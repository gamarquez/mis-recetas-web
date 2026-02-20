using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MisRecetas.Web.Services;
using MisRecetas.Web.ViewModels;

namespace MisRecetas.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly RecetaService _recetaService;

    public DashboardController(RecetaService recetaService)
        => _recetaService = recetaService;

    public IActionResult Index()
    {
        var vm = new DashboardViewModel
        {
            NombreUsuario = User.FindFirstValue(ClaimTypes.GivenName) ?? User.Identity!.Name!,
            Rol = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty
        };

        // Los contadores se cargarán cuando la DB esté conectada (Fase 2)
        // Por ahora se muestran en 0 para que el login funcione
        try
        {
            vm.TotalPendientes = _recetaService.ObtenerPendientes().Count;
            vm.TotalConMedico = _recetaService.ObtenerConMedico().Count;
            vm.TotalListasParaEntregar = _recetaService.ObtenerListasParaEntregar().Count;
            vm.TotalArchivadas = _recetaService.ObtenerArchivadas().Count;
            vm.TotalDesechadas = _recetaService.ObtenerDesechadas().Count;
            vm.TotalParaArchivar = _recetaService.ObtenerParaArchivar().Count;
        }
        catch
        {
            // Si la DB no está disponible, el dashboard igual carga con 0
            TempData["Error"] = "No se pudo conectar a la base de datos. Configure la cadena de conexión.";
        }

        return View(vm);
    }
}
