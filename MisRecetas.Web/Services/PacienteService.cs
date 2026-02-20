using MisRecetas.Web.Data;
using MisRecetas.Web.Models;

namespace MisRecetas.Web.Services;

public class PacienteService
{
    private readonly PacienteRepository _repo;
    private readonly CatalogoRepository _catalogo;

    public PacienteService(PacienteRepository repo, CatalogoRepository catalogo)
    {
        _repo = repo;
        _catalogo = catalogo;
    }

    public List<TipoDocumento> ObtenerTiposDocumento() => _catalogo.ObtenerComboTipoDocumento();

    public Paciente? BuscarPorDocumento(int idTipoDocumento, string nroDocumento)
        => _repo.BuscarPorDocumento(idTipoDocumento, nroDocumento);

    public Paciente? ObtenerPorId(int id) => _repo.ObtenerPorId(id);

    public List<Paciente> BuscarPorNombreApellido(string nombre, string apellido)
        => _repo.BuscarPorNombreApellido(nombre, apellido);

    public int Insertar(Paciente p) => _repo.Insertar(p);
    public void Actualizar(Paciente p) => _repo.Actualizar(p);
}
