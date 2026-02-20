using MisRecetas.Web.Data;
using MisRecetas.Web.Models;

namespace MisRecetas.Web.Services;

public class UsuarioService
{
    private readonly UsuarioRepository _repo;
    private readonly CatalogoRepository _catalogo;

    public UsuarioService(UsuarioRepository repo, CatalogoRepository catalogo)
    {
        _repo = repo;
        _catalogo = catalogo;
    }

    public List<Usuario> ObtenerTodos() => _repo.ObtenerTodos();
    public Usuario? ObtenerPorId(int id) => _repo.ObtenerPorId(id);
    public List<Rol> ObtenerRoles() => _catalogo.ObtenerRoles();

    public (bool ok, string error) Insertar(Usuario u, string passwordPlano)
    {
        if (_repo.ExisteNombreUsuario(u.Nombre_Usuario))
            return (false, "El nombre de usuario ya existe.");

        u.Password = BCrypt.Net.BCrypt.HashPassword(passwordPlano, workFactor: 12);
        _repo.Insertar(u);
        return (true, string.Empty);
    }

    public (bool ok, string error) Actualizar(Usuario u)
    {
        if (_repo.ExisteNombreUsuario(u.Nombre_Usuario, u.Id_Usuario))
            return (false, "El nombre de usuario ya existe.");

        _repo.Actualizar(u);
        return (true, string.Empty);
    }

    public void ResetPassword(int idUsuario, string nuevaPassword)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(nuevaPassword, workFactor: 12);
        _repo.ActualizarPassword(idUsuario, hash);
    }
}
