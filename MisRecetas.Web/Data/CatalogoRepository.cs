using Microsoft.Data.SqlClient;
using MisRecetas.Web.Models;

namespace MisRecetas.Web.Data;

/// <summary>
/// Repositorio para tablas de catálogo: Rol, Estado, TipoDocumento.
/// También llama a los SPs existentes para combo boxes.
/// </summary>
public class CatalogoRepository
{
    private readonly DbConnectionFactory _factory;

    public CatalogoRepository(DbConnectionFactory factory)
        => _factory = factory;

    // ── Médicos combo (SP existente) ───────────────────────────────────────
    public List<Medico> ObtenerComboMedicos()
    {
        var lista = new List<Medico>();
        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new SqlCommand("SP_CargarComboBoxMedicos", conn)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            lista.Add(new Medico
            {
                Id_Medico = reader.GetInt32(reader.GetOrdinal("Id_Medico")),
                Nombre_Medico = reader.GetString(reader.GetOrdinal("Nombre_Medico")),
                Apellido_Medico = reader.GetString(reader.GetOrdinal("Apellido_Medico")),
                Nro_Matricula = reader.IsDBNull(reader.GetOrdinal("Nro_Matricula"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("Nro_Matricula"))
            });
        }
        return lista;
    }

    // ── Tipo Documento combo (SP existente) ────────────────────────────────
    public List<TipoDocumento> ObtenerComboTipoDocumento()
    {
        var lista = new List<TipoDocumento>();
        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new SqlCommand("SP_CargarComboBoxTipoDocumento", conn)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            lista.Add(new TipoDocumento
            {
                Id_TipoDocumento = reader.GetInt32(reader.GetOrdinal("Id_TipoDocumento")),
                Descripcion = reader.GetString(reader.GetOrdinal("Descripcion"))
            });
        }
        return lista;
    }

    // ── Roles ──────────────────────────────────────────────────────────────
    public List<Rol> ObtenerRoles()
    {
        const string sql = "SELECT Id_Rol, Descripcion FROM Rol ORDER BY Id_Rol";
        var lista = new List<Rol>();
        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            lista.Add(new Rol
            {
                Id_Rol = reader.GetInt32(0),
                Descripcion = reader.GetString(1)
            });
        }
        return lista;
    }

    // ── Estados ────────────────────────────────────────────────────────────
    public List<Estado> ObtenerEstados()
    {
        const string sql = "SELECT Id_Estado, Descripcion FROM Estado ORDER BY Id_Estado";
        var lista = new List<Estado>();
        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            lista.Add(new Estado
            {
                Id_Estado = reader.GetInt32(0),
                Descripcion = reader.GetString(1)
            });
        }
        return lista;
    }
}
