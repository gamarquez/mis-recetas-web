using MisRecetas.Web.Data;
using MisRecetas.Web.Models;

namespace MisRecetas.Web.Services;

public class MedicoService
{
    private readonly MedicoRepository _repo;
    private readonly CatalogoRepository _catalogo;

    public MedicoService(MedicoRepository repo, CatalogoRepository catalogo)
    {
        _repo = repo;
        _catalogo = catalogo;
    }

    public List<Medico> ObtenerTodos() => _repo.ObtenerTodos();
    public List<Medico> ObtenerCombo() => _catalogo.ObtenerComboMedicos();
    public Medico? ObtenerPorId(int id) => _repo.ObtenerPorId(id);
    public void Insertar(Medico m) => _repo.Insertar(m);
    public void Actualizar(Medico m) => _repo.Actualizar(m);
}
