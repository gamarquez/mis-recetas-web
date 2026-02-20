using Microsoft.Data.SqlClient;
using MisRecetas.Web.Models;

namespace MisRecetas.Web.Data;

public class UsuarioRepository
{
    private readonly DbConnectionFactory _factory;

    public UsuarioRepository(DbConnectionFactory factory)
        => _factory = factory;

    // ── Login ──────────────────────────────────────────────────────────────
    public Usuario? ObtenerPorNombreUsuario(string nombreUsuario)
    {
        const string sql = """
            SELECT u.Id_Usuario, u.Nombre_Usuario, u.Password,
                   u.Id_Rol, u.Nombre, u.Apellido, r.Descripcion AS DescripcionRol
            FROM Usuario u
            INNER JOIN Rol r ON u.Id_Rol = r.Id_Rol
            WHERE u.Nombre_Usuario = @NombreUsuario
            """;

        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@NombreUsuario", nombreUsuario);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;

        return MapUsuario(reader);
    }

    // ── Actualizar password (migración BCrypt) ─────────────────────────────
    public void ActualizarPassword(int idUsuario, string passwordHash)
    {
        const string sql = "UPDATE Usuario SET Password = @Password WHERE Id_Usuario = @Id";

        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Password", passwordHash);
        cmd.Parameters.AddWithValue("@Id", idUsuario);
        cmd.ExecuteNonQuery();
    }

    // ── CRUD ───────────────────────────────────────────────────────────────
    public List<Usuario> ObtenerTodos()
    {
        const string sql = """
            SELECT u.Id_Usuario, u.Nombre_Usuario, u.Password,
                   u.Id_Rol, u.Nombre, u.Apellido, r.Descripcion AS DescripcionRol
            FROM Usuario u
            INNER JOIN Rol r ON u.Id_Rol = r.Id_Rol
            ORDER BY u.Apellido, u.Nombre
            """;

        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        var lista = new List<Usuario>();
        while (reader.Read()) lista.Add(MapUsuario(reader));
        return lista;
    }

    public Usuario? ObtenerPorId(int id)
    {
        const string sql = """
            SELECT u.Id_Usuario, u.Nombre_Usuario, u.Password,
                   u.Id_Rol, u.Nombre, u.Apellido, r.Descripcion AS DescripcionRol
            FROM Usuario u
            INNER JOIN Rol r ON u.Id_Rol = r.Id_Rol
            WHERE u.Id_Usuario = @Id
            """;

        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapUsuario(reader) : null;
    }

    public void Insertar(Usuario u)
    {
        const string sql = """
            INSERT INTO Usuario (Nombre_Usuario, Password, Id_Rol, Nombre, Apellido)
            VALUES (@NombreUsuario, @Password, @IdRol, @Nombre, @Apellido)
            """;

        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@NombreUsuario", u.Nombre_Usuario);
        cmd.Parameters.AddWithValue("@Password", u.Password);
        cmd.Parameters.AddWithValue("@IdRol", u.Id_Rol);
        cmd.Parameters.AddWithValue("@Nombre", u.Nombre);
        cmd.Parameters.AddWithValue("@Apellido", u.Apellido);
        cmd.ExecuteNonQuery();
    }

    public void Actualizar(Usuario u)
    {
        const string sql = """
            UPDATE Usuario
            SET Nombre_Usuario = @NombreUsuario,
                Id_Rol = @IdRol,
                Nombre = @Nombre,
                Apellido = @Apellido
            WHERE Id_Usuario = @Id
            """;

        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@NombreUsuario", u.Nombre_Usuario);
        cmd.Parameters.AddWithValue("@IdRol", u.Id_Rol);
        cmd.Parameters.AddWithValue("@Nombre", u.Nombre);
        cmd.Parameters.AddWithValue("@Apellido", u.Apellido);
        cmd.Parameters.AddWithValue("@Id", u.Id_Usuario);
        cmd.ExecuteNonQuery();
    }

    public bool ExisteNombreUsuario(string nombreUsuario, int? excluirId = null)
    {
        const string sql = """
            SELECT COUNT(1) FROM Usuario
            WHERE Nombre_Usuario = @NombreUsuario
              AND (@ExcluirId IS NULL OR Id_Usuario <> @ExcluirId)
            """;

        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@NombreUsuario", nombreUsuario);
        cmd.Parameters.AddWithValue("@ExcluirId", (object?)excluirId ?? DBNull.Value);
        return (int)cmd.ExecuteScalar()! > 0;
    }

    // ── Mapper ─────────────────────────────────────────────────────────────
    private static Usuario MapUsuario(SqlDataReader r) => new()
    {
        Id_Usuario = r.GetInt32(r.GetOrdinal("Id_Usuario")),
        Nombre_Usuario = r.GetString(r.GetOrdinal("Nombre_Usuario")),
        Password = r.GetString(r.GetOrdinal("Password")),
        Id_Rol = r.GetInt32(r.GetOrdinal("Id_Rol")),
        Nombre = r.GetString(r.GetOrdinal("Nombre")),
        Apellido = r.GetString(r.GetOrdinal("Apellido")),
        DescripcionRol = r.GetString(r.GetOrdinal("DescripcionRol"))
    };
}
