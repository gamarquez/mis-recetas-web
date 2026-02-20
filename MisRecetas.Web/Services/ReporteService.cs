using MisRecetas.Web.Data;
using MisRecetas.Web.Models;

namespace MisRecetas.Web.Services;

/// <summary>
/// Servicio de reportes — las consultas se delegarán a RecetaRepository en Fase 6.
/// Por ahora expone los métodos base que usan las queries genéricas del repo.
/// </summary>
public class ReporteService
{
    private readonly RecetaRepository _recetaRepo;

    public ReporteService(RecetaRepository recetaRepo)
        => _recetaRepo = recetaRepo;

    public List<RecetaDetalle> RecetasPorEstado(int idEstado)
        => _recetaRepo.ObtenerPorEstado(idEstado);

    public List<RecetaDetalle> RecetasArchivadas()
        => _recetaRepo.ObtenerPorEstado(5);

    public List<RecetaDetalle> RecetasDesechadas()
        => _recetaRepo.ObtenerPorEstado(6);

    /// <summary>
    /// idEstado = 0 retorna lista vacía (el caller itera sobre todos los estados separadamente).
    /// </summary>
    public List<RecetaDetalle> RecetasPorMedico(int idMedico, int idEstado)
    {
        if (idEstado == 0) return [];
        return _recetaRepo.ObtenerPorMedico(idMedico, idEstado);
    }

    public RecetaDetalle? TicketReceta(int nroReceta)
        => _recetaRepo.Buscar(nroReceta.ToString(), null, null, null).FirstOrDefault();
}
