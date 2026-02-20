using Microsoft.Data.SqlClient;
using MisRecetas.Web.Models;

namespace MisRecetas.Web.Data;

public class PacienteRepository
{
    private readonly DbConnectionFactory _factory;

    public PacienteRepository(DbConnectionFactory factory)
        => _factory = factory;

    public Paciente? BuscarPorDocumento(int idTipoDocumento, string nroDocumento)
    {
        const string sql = """
            SELECT p.Id_Paciente, p.Id_TipoDocumento, p.Nro_Documento,
                   p.Nombre_Paciente, p.Apellido_Paciente, p.Email,
                   td.Descripcion AS DescripcionTipoDocumento
            FROM Paciente p
            INNER JOIN Tipo_Documento td ON p.Id_TipoDocumento = td.Id_TipoDocumento
            WHERE p.Id_TipoDocumento = @Tipo AND p.Nro_Documento = @Doc
            """;

        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Tipo", idTipoDocumento);
        cmd.Parameters.AddWithValue("@Doc", nroDocumento);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapPaciente(reader) : null;
    }

    public Paciente? ObtenerPorId(int id)
    {
        const string sql = """
            SELECT p.Id_Paciente, p.Id_TipoDocumento, p.Nro_Documento,
                   p.Nombre_Paciente, p.Apellido_Paciente, p.Email,
                   td.Descripcion AS DescripcionTipoDocumento
            FROM Paciente p
            INNER JOIN Tipo_Documento td ON p.Id_TipoDocumento = td.Id_TipoDocumento
            WHERE p.Id_Paciente = @Id
            """;

        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapPaciente(reader) : null;
    }

    public List<Paciente> BuscarPorNombreApellido(string nombre, string apellido)
    {
        const string sql = """
            SELECT p.Id_Paciente, p.Id_TipoDocumento, p.Nro_Documento,
                   p.Nombre_Paciente, p.Apellido_Paciente, p.Email,
                   td.Descripcion AS DescripcionTipoDocumento
            FROM Paciente p
            INNER JOIN Tipo_Documento td ON p.Id_TipoDocumento = td.Id_TipoDocumento
            WHERE (@Nombre = '' OR p.Nombre_Paciente LIKE @NombreLike)
              AND (@Apellido = '' OR p.Apellido_Paciente LIKE @ApellidoLike)
            ORDER BY p.Apellido_Paciente, p.Nombre_Paciente
            """;

        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Nombre", nombre);
        cmd.Parameters.AddWithValue("@NombreLike", $"%{nombre}%");
        cmd.Parameters.AddWithValue("@Apellido", apellido);
        cmd.Parameters.AddWithValue("@ApellidoLike", $"%{apellido}%");

        using var reader = cmd.ExecuteReader();
        var lista = new List<Paciente>();
        while (reader.Read()) lista.Add(MapPaciente(reader));
        return lista;
    }

    public int Insertar(Paciente p)
    {
        const string sql = """
            INSERT INTO Paciente (Id_TipoDocumento, Nro_Documento, Nombre_Paciente, Apellido_Paciente, Email)
            OUTPUT INSERTED.Id_Paciente
            VALUES (@Tipo, @Doc, @Nombre, @Apellido, @Email)
            """;

        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Tipo", p.Id_TipoDocumento);
        cmd.Parameters.AddWithValue("@Doc", p.Nro_Documento);
        cmd.Parameters.AddWithValue("@Nombre", p.Nombre_Paciente);
        cmd.Parameters.AddWithValue("@Apellido", p.Apellido_Paciente);
        cmd.Parameters.AddWithValue("@Email", p.Email ?? (object)DBNull.Value);
        return (int)cmd.ExecuteScalar()!;
    }

    public void Actualizar(Paciente p)
    {
        const string sql = """
            UPDATE Paciente
            SET Id_TipoDocumento = @Tipo,
                Nro_Documento = @Doc,
                Nombre_Paciente = @Nombre,
                Apellido_Paciente = @Apellido,
                Email = @Email
            WHERE Id_Paciente = @Id
            """;

        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Tipo", p.Id_TipoDocumento);
        cmd.Parameters.AddWithValue("@Doc", p.Nro_Documento);
        cmd.Parameters.AddWithValue("@Nombre", p.Nombre_Paciente);
        cmd.Parameters.AddWithValue("@Apellido", p.Apellido_Paciente);
        cmd.Parameters.AddWithValue("@Email", p.Email ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@Id", p.Id_Paciente);
        cmd.ExecuteNonQuery();
    }

    private static Paciente MapPaciente(SqlDataReader r) => new()
    {
        Id_Paciente = r.GetInt32(r.GetOrdinal("Id_Paciente")),
        Id_TipoDocumento = r.GetInt32(r.GetOrdinal("Id_TipoDocumento")),
        Nro_Documento = r.GetString(r.GetOrdinal("Nro_Documento")),
        Nombre_Paciente = r.GetString(r.GetOrdinal("Nombre_Paciente")),
        Apellido_Paciente = r.GetString(r.GetOrdinal("Apellido_Paciente")),
        Email = r.IsDBNull(r.GetOrdinal("Email")) ? string.Empty : r.GetString(r.GetOrdinal("Email")),
        DescripcionTipoDocumento = r.GetString(r.GetOrdinal("DescripcionTipoDocumento"))
    };
}
