using MisRecetas.Web.Data;
using MisRecetas.Web.Models;

namespace MisRecetas.Web.Services;

/// <summary>
/// Orquesta la lógica de negocio de recetas sobre RecetaRepository.
/// </summary>
public class RecetaService
{
    private readonly RecetaRepository _recetaRepo;

    public RecetaService(RecetaRepository recetaRepo)
        => _recetaRepo = recetaRepo;

    public List<RecetaDetalle> ObtenerPendientes()
        => _recetaRepo.ObtenerPorEstado(1);

    public List<RecetaDetalle> ObtenerConMedico()
        => _recetaRepo.ObtenerPorEstado(2);

    public List<RecetaDetalle> ObtenerListasParaEntregar()
        => _recetaRepo.ObtenerPorEstado(3);

    public List<RecetaDetalle> ObtenerEntregadas()
        => _recetaRepo.ObtenerPorEstado(4);

    public List<RecetaDetalle> ObtenerArchivadas()
        => _recetaRepo.ObtenerPorEstado(5);

    public List<RecetaDetalle> ObtenerDesechadas()
        => _recetaRepo.ObtenerPorEstado(6);

    public List<RecetaDetalle> ObtenerParaArchivar()
        => _recetaRepo.ObtenerParaArchivar();

    public List<RecetaDetalle> ObtenerPorMedicoYEstado(int idMedico, int idEstado)
        => _recetaRepo.ObtenerPorMedico(idMedico, idEstado);

    public List<RecetaDetalle> Buscar(string? nroReceta, string? nombre, string? apellido, string? documento)
        => _recetaRepo.Buscar(nroReceta, nombre, apellido, documento);

    public int SiguienteNroReceta()
        => _recetaRepo.ObtenerUltimoNroReceta() + 1;

    public void Registrar(int nroReceta, int idMedico, int idPaciente, int idUsuario)
        => _recetaRepo.Registrar(nroReceta, idMedico, idPaciente, idUsuario);

    public void EntregarAMedico(IEnumerable<int> nrosReceta, int idUsuario)
    {
        foreach (var nro in nrosReceta)
            _recetaRepo.EntregarAMedico(nro, idUsuario);
    }

    public void RecibirDeMedico(IEnumerable<int> nrosReceta, int idUsuario)
    {
        foreach (var nro in nrosReceta)
            _recetaRepo.RecibirDeMedico(nro, idUsuario);
    }

    public void EntregarAPaciente(int nroReceta, int idUsuario, string nombreRetira, string parentezco)
        => _recetaRepo.EntregarAPaciente(nroReceta, idUsuario, nombreRetira, parentezco);

    public void Archivar(IEnumerable<int> nrosReceta, int idUsuario)
    {
        foreach (var nro in nrosReceta)
            _recetaRepo.Archivar(nro, idUsuario);
    }

    public void Desechar(IEnumerable<int> nrosReceta, int idUsuario)
    {
        foreach (var nro in nrosReceta)
            _recetaRepo.Desechar(nro, idUsuario);
    }

    public void EntregarArchivada(int nroReceta, int idUsuario, string nombreRetira, string parentezco)
        => _recetaRepo.EntregarArchivada(nroReceta, idUsuario, nombreRetira, parentezco);

    public void ModificarMedicoYEstado(int nroReceta, int idMedico, int idEstado)
        => _recetaRepo.ModificarMedicoYEstado(nroReceta, idMedico, idEstado);
}
