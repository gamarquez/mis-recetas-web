using Microsoft.Data.SqlClient;
using MisRecetas.Web.Models;

namespace MisRecetas.Web.Data;

public class MedicoRepository
{
    private readonly DbConnectionFactory _factory;

    public MedicoRepository(DbConnectionFactory factory)
        => _factory = factory;

    public List<Medico> ObtenerTodos()
    {
        const string sql = """
            SELECT Id_Medico, Nro_Matricula, Nombre_Medico, Apellido_Medico
            FROM Medico ORDER BY Apellido_Medico, Nombre_Medico
            """;

        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        var lista = new List<Medico>();
        while (reader.Read()) lista.Add(MapMedico(reader));
        return lista;
    }

    public Medico? ObtenerPorId(int id)
    {
        const string sql = """
            SELECT Id_Medico, Nro_Matricula, Nombre_Medico, Apellido_Medico
            FROM Medico WHERE Id_Medico = @Id
            """;

        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapMedico(reader) : null;
    }

    public void Insertar(Medico m)
    {
        const string sql = """
            INSERT INTO Medico (Nro_Matricula, Nombre_Medico, Apellido_Medico)
            VALUES (@Matricula, @Nombre, @Apellido)
            """;

        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Matricula", m.Nro_Matricula);
        cmd.Parameters.AddWithValue("@Nombre", m.Nombre_Medico);
        cmd.Parameters.AddWithValue("@Apellido", m.Apellido_Medico);
        cmd.ExecuteNonQuery();
    }

    public void Actualizar(Medico m)
    {
        const string sql = """
            UPDATE Medico
            SET Nro_Matricula = @Matricula,
                Nombre_Medico = @Nombre,
                Apellido_Medico = @Apellido
            WHERE Id_Medico = @Id
            """;

        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Matricula", m.Nro_Matricula);
        cmd.Parameters.AddWithValue("@Nombre", m.Nombre_Medico);
        cmd.Parameters.AddWithValue("@Apellido", m.Apellido_Medico);
        cmd.Parameters.AddWithValue("@Id", m.Id_Medico);
        cmd.ExecuteNonQuery();
    }

    private static Medico MapMedico(SqlDataReader r) => new()
    {
        Id_Medico = r.GetInt32(r.GetOrdinal("Id_Medico")),
        Nro_Matricula = r.GetString(r.GetOrdinal("Nro_Matricula")),
        Nombre_Medico = r.GetString(r.GetOrdinal("Nombre_Medico")),
        Apellido_Medico = r.GetString(r.GetOrdinal("Apellido_Medico"))
    };
}
