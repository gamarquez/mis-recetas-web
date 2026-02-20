using Microsoft.Data.SqlClient;
using MisRecetas.Web.Models;

namespace MisRecetas.Web.Data;

public class RecetaRepository
{
    private readonly DbConnectionFactory _factory;

    public RecetaRepository(DbConnectionFactory factory)
        => _factory = factory;

    // ── Consultas por estado ───────────────────────────────────────────────

    public List<RecetaDetalle> ObtenerPorEstado(int idEstado)
    {
        const string sql = """
            SELECT r.Id_Receta, r.Nro_Receta, r.Id_Estado, e.Descripcion AS DescripcionEstado,
                   r.Id_Medico, m.Nro_Matricula AS NroMatricula,
                   m.Nombre_Medico AS NombreMedico, m.Apellido_Medico AS ApellidoMedico,
                   r.Id_Paciente, p.Nro_Documento AS NroDocumento,
                   td.Descripcion AS TipoDocumento,
                   p.Nombre_Paciente AS NombrePaciente, p.Apellido_Paciente AS ApellidoPaciente,
                   r.Fecha_Usuario_Registra AS Fecha_Registro,
                   r.Fecha_Usuario_Entrega_A_Medico AS Fecha_Entrega_Medico,
                   r.Fecha_Usuario_Recibe_De_Medico AS Fecha_Recibe_Medico,
                   r.Fecha_Usuario_Entrega_A_Paciente AS Fecha_Entrega_Paciente,
                   r.Fecha_Entrega_A_Archivo AS Fecha_Archivo,
                   r.Fecha_Usuario_Desecha AS Fecha_Desecho,
                   r.Nombre_Retira, r.Parentezco,
                   ISNULL(DATEDIFF(Day, r.Fecha_Usuario_Recibe_De_Medico, GETDATE()), 0) AS DiasEnEstado3
            FROM Receta r
            INNER JOIN Medico m ON r.Id_Medico = m.Id_Medico
            INNER JOIN Paciente p ON r.Id_Paciente = p.Id_Paciente
            INNER JOIN Tipo_Documento td ON p.Id_TipoDocumento = td.Id_TipoDocumento
            INNER JOIN Estado e ON r.Id_Estado = e.Id_Estado
            WHERE r.Id_Estado = @Estado
            ORDER BY r.Nro_Receta
            """;

        return EjecutarListaDetalle(sql, cmd =>
            cmd.Parameters.AddWithValue("@Estado", idEstado));
    }

    public List<RecetaDetalle> ObtenerParaArchivar()
    {
        // Estado 3 con 7+ días desde que regresó del médico
        const string sql = """
            SELECT r.Id_Receta, r.Nro_Receta, r.Id_Estado, e.Descripcion AS DescripcionEstado,
                   r.Id_Medico, m.Nro_Matricula AS NroMatricula,
                   m.Nombre_Medico AS NombreMedico, m.Apellido_Medico AS ApellidoMedico,
                   r.Id_Paciente, p.Nro_Documento AS NroDocumento,
                   td.Descripcion AS TipoDocumento,
                   p.Nombre_Paciente AS NombrePaciente, p.Apellido_Paciente AS ApellidoPaciente,
                   r.Fecha_Usuario_Registra AS Fecha_Registro,
                   r.Fecha_Usuario_Entrega_A_Medico AS Fecha_Entrega_Medico,
                   r.Fecha_Usuario_Recibe_De_Medico AS Fecha_Recibe_Medico,
                   r.Fecha_Usuario_Entrega_A_Paciente AS Fecha_Entrega_Paciente,
                   r.Fecha_Entrega_A_Archivo AS Fecha_Archivo,
                   r.Fecha_Usuario_Desecha AS Fecha_Desecho,
                   r.Nombre_Retira, r.Parentezco,
                   DATEDIFF(Day, r.Fecha_Usuario_Recibe_De_Medico, GETDATE()) AS DiasEnEstado3
            FROM Receta r
            INNER JOIN Medico m ON r.Id_Medico = m.Id_Medico
            INNER JOIN Paciente p ON r.Id_Paciente = p.Id_Paciente
            INNER JOIN Tipo_Documento td ON p.Id_TipoDocumento = td.Id_TipoDocumento
            INNER JOIN Estado e ON r.Id_Estado = e.Id_Estado
            WHERE r.Id_Estado = 3
              AND DATEDIFF(Day, r.Fecha_Usuario_Recibe_De_Medico, GETDATE()) >= 7
            ORDER BY r.Fecha_Usuario_Recibe_De_Medico
            """;

        return EjecutarListaDetalle(sql, _ => { });
    }

    public List<RecetaDetalle> ObtenerPorMedico(int idMedico, int idEstado)
    {
        const string sql = """
            SELECT r.Id_Receta, r.Nro_Receta, r.Id_Estado, e.Descripcion AS DescripcionEstado,
                   r.Id_Medico, m.Nro_Matricula AS NroMatricula,
                   m.Nombre_Medico AS NombreMedico, m.Apellido_Medico AS ApellidoMedico,
                   r.Id_Paciente, p.Nro_Documento AS NroDocumento,
                   td.Descripcion AS TipoDocumento,
                   p.Nombre_Paciente AS NombrePaciente, p.Apellido_Paciente AS ApellidoPaciente,
                   r.Fecha_Usuario_Registra AS Fecha_Registro,
                   r.Fecha_Usuario_Entrega_A_Medico AS Fecha_Entrega_Medico,
                   r.Fecha_Usuario_Recibe_De_Medico AS Fecha_Recibe_Medico,
                   r.Fecha_Usuario_Entrega_A_Paciente AS Fecha_Entrega_Paciente,
                   r.Fecha_Entrega_A_Archivo AS Fecha_Archivo,
                   r.Fecha_Usuario_Desecha AS Fecha_Desecho,
                   r.Nombre_Retira, r.Parentezco,
                   ISNULL(DATEDIFF(Day, r.Fecha_Usuario_Recibe_De_Medico, GETDATE()), 0) AS DiasEnEstado3
            FROM Receta r
            INNER JOIN Medico m ON r.Id_Medico = m.Id_Medico
            INNER JOIN Paciente p ON r.Id_Paciente = p.Id_Paciente
            INNER JOIN Tipo_Documento td ON p.Id_TipoDocumento = td.Id_TipoDocumento
            INNER JOIN Estado e ON r.Id_Estado = e.Id_Estado
            WHERE r.Id_Medico = @Medico AND r.Id_Estado = @Estado
            ORDER BY r.Nro_Receta
            """;

        return EjecutarListaDetalle(sql, cmd =>
        {
            cmd.Parameters.AddWithValue("@Medico", idMedico);
            cmd.Parameters.AddWithValue("@Estado", idEstado);
        });
    }

    // ── Buscar ─────────────────────────────────────────────────────────────
    public List<RecetaDetalle> Buscar(string? nroReceta, string? nombre, string? apellido, string? documento)
    {
        const string sql = """
            SELECT r.Id_Receta, r.Nro_Receta, r.Id_Estado, e.Descripcion AS DescripcionEstado,
                   r.Id_Medico, m.Nro_Matricula AS NroMatricula,
                   m.Nombre_Medico AS NombreMedico, m.Apellido_Medico AS ApellidoMedico,
                   r.Id_Paciente, p.Nro_Documento AS NroDocumento,
                   td.Descripcion AS TipoDocumento,
                   p.Nombre_Paciente AS NombrePaciente, p.Apellido_Paciente AS ApellidoPaciente,
                   r.Fecha_Usuario_Registra AS Fecha_Registro,
                   r.Fecha_Usuario_Entrega_A_Medico AS Fecha_Entrega_Medico,
                   r.Fecha_Usuario_Recibe_De_Medico AS Fecha_Recibe_Medico,
                   r.Fecha_Usuario_Entrega_A_Paciente AS Fecha_Entrega_Paciente,
                   r.Fecha_Entrega_A_Archivo AS Fecha_Archivo,
                   r.Fecha_Usuario_Desecha AS Fecha_Desecho,
                   r.Nombre_Retira, r.Parentezco,
                   ISNULL(DATEDIFF(Day, r.Fecha_Usuario_Recibe_De_Medico, GETDATE()), 0) AS DiasEnEstado3
            FROM Receta r
            INNER JOIN Medico m ON r.Id_Medico = m.Id_Medico
            INNER JOIN Paciente p ON r.Id_Paciente = p.Id_Paciente
            INNER JOIN Tipo_Documento td ON p.Id_TipoDocumento = td.Id_TipoDocumento
            INNER JOIN Estado e ON r.Id_Estado = e.Id_Estado
            WHERE (@NroReceta IS NULL OR r.Nro_Receta = @NroReceta)
              AND (@Nombre IS NULL OR p.Nombre_Paciente LIKE @NombreLike)
              AND (@Apellido IS NULL OR p.Apellido_Paciente LIKE @ApellidoLike)
              AND (@Documento IS NULL OR p.Nro_Documento LIKE @DocumentoLike)
            ORDER BY r.Nro_Receta DESC
            """;

        return EjecutarListaDetalle(sql, cmd =>
        {
            cmd.Parameters.AddWithValue("@NroReceta",
                int.TryParse(nroReceta, out var nro) ? (object)nro : DBNull.Value);
            cmd.Parameters.AddWithValue("@Nombre", string.IsNullOrWhiteSpace(nombre) ? DBNull.Value : nombre);
            cmd.Parameters.AddWithValue("@NombreLike", $"%{nombre}%");
            cmd.Parameters.AddWithValue("@Apellido", string.IsNullOrWhiteSpace(apellido) ? DBNull.Value : apellido);
            cmd.Parameters.AddWithValue("@ApellidoLike", $"%{apellido}%");
            cmd.Parameters.AddWithValue("@Documento", string.IsNullOrWhiteSpace(documento) ? DBNull.Value : documento);
            cmd.Parameters.AddWithValue("@DocumentoLike", $"%{documento}%");
        });
    }

    // ── Registro ───────────────────────────────────────────────────────────
    public void Registrar(int nroReceta, int idMedico, int idPaciente, int idUsuario)
    {
        const string sql = """
            INSERT INTO Receta (Nro_Receta, Id_Medico, Id_Paciente, Id_Estado,
                                Id_Usuario_Registra, Fecha_Usuario_Registra)
            VALUES (@Nro, @Medico, @Paciente, 1, @Usuario, GETDATE())
            """;

        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Nro", nroReceta);
        cmd.Parameters.AddWithValue("@Medico", idMedico);
        cmd.Parameters.AddWithValue("@Paciente", idPaciente);
        cmd.Parameters.AddWithValue("@Usuario", idUsuario);
        cmd.ExecuteNonQuery();
    }

    public int ObtenerUltimoNroReceta()
    {
        const string sql = "SELECT ISNULL(MAX(Nro_Receta), 0) FROM Receta";
        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        return (int)cmd.ExecuteScalar()!;
    }

    // ── Cambios de estado ──────────────────────────────────────────────────

    public void EntregarAMedico(int nroReceta, int idUsuario)
    {
        const string sql = """
            UPDATE Receta
            SET Id_Estado = 2,
                Id_Usuario_Entrega_A_Medico = @Usuario,
                Fecha_Usuario_Entrega_A_Medico = GETDATE()
            WHERE Nro_Receta = @Nro AND Id_Estado = 1
            """;
        EjecutarActualizacion(sql, nroReceta, idUsuario);
    }

    public void RecibirDeMedico(int nroReceta, int idUsuario)
    {
        const string sql = """
            UPDATE Receta
            SET Id_Estado = 3,
                Id_Usuario_Recibe_De_Medico = @Usuario,
                Fecha_Usuario_Recibe_De_Medico = GETDATE()
            WHERE Nro_Receta = @Nro AND Id_Estado = 2
            """;
        EjecutarActualizacion(sql, nroReceta, idUsuario);
    }

    public void EntregarAPaciente(int nroReceta, int idUsuario, string nombreRetira, string parentezco)
    {
        const string sql = """
            UPDATE Receta
            SET Id_Estado = 4,
                Id_Usuario_Entrega_A_Paciente = @Usuario,
                Fecha_Usuario_Entrega_A_Paciente = GETDATE(),
                Nombre_Retira = @NombreRetira,
                Parentezco = @Parentezco
            WHERE Nro_Receta = @Nro AND Id_Estado = 3
            """;

        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Nro", nroReceta);
        cmd.Parameters.AddWithValue("@Usuario", idUsuario);
        cmd.Parameters.AddWithValue("@NombreRetira", nombreRetira);
        cmd.Parameters.AddWithValue("@Parentezco", parentezco);
        cmd.ExecuteNonQuery();
    }

    public void Archivar(int nroReceta, int idUsuario)
    {
        const string sql = """
            UPDATE Receta
            SET Id_Estado = 5,
                Id_Usuario_Entrega_A_Archivo = @Usuario,
                Fecha_Entrega_A_Archivo = GETDATE()
            WHERE Nro_Receta = @Nro AND Id_Estado = 3
            """;
        EjecutarActualizacion(sql, nroReceta, idUsuario);
    }

    public void Desechar(int nroReceta, int idUsuario)
    {
        const string sql = """
            UPDATE Receta
            SET Id_Estado = 6,
                Id_Usuario_Desecha = @Usuario,
                Fecha_Usuario_Desecha = GETDATE()
            WHERE Nro_Receta = @Nro AND Id_Estado = 5
            """;
        EjecutarActualizacion(sql, nroReceta, idUsuario);
    }

    public void EntregarArchivada(int nroReceta, int idUsuario, string nombreRetira, string parentezco)
    {
        const string sql = """
            UPDATE Receta
            SET Id_Estado = 4,
                Id_Usuario_Entrega_A_Paciente = @Usuario,
                Fecha_Usuario_Entrega_A_Paciente = GETDATE(),
                Nombre_Retira = @NombreRetira,
                Parentezco = @Parentezco
            WHERE Nro_Receta = @Nro AND Id_Estado = 5
            """;

        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Nro", nroReceta);
        cmd.Parameters.AddWithValue("@Usuario", idUsuario);
        cmd.Parameters.AddWithValue("@NombreRetira", nombreRetira);
        cmd.Parameters.AddWithValue("@Parentezco", parentezco);
        cmd.ExecuteNonQuery();
    }

    // ── Modificar (solo Admin) ─────────────────────────────────────────────
    public void ModificarMedicoYEstado(int nroReceta, int idMedico, int idEstado)
    {
        const string sql = """
            UPDATE Receta
            SET Id_Medico = @Medico, Id_Estado = @Estado
            WHERE Nro_Receta = @Nro
            """;

        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Medico", idMedico);
        cmd.Parameters.AddWithValue("@Estado", idEstado);
        cmd.Parameters.AddWithValue("@Nro", nroReceta);
        cmd.ExecuteNonQuery();
    }

    // ── Helpers privados ───────────────────────────────────────────────────
    private void EjecutarActualizacion(string sql, int nroReceta, int idUsuario)
    {
        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Nro", nroReceta);
        cmd.Parameters.AddWithValue("@Usuario", idUsuario);
        cmd.ExecuteNonQuery();
    }

    private List<RecetaDetalle> EjecutarListaDetalle(string sql, Action<SqlCommand> parametros)
    {
        using var conn = _factory.CreateConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        parametros(cmd);
        using var reader = cmd.ExecuteReader();

        var lista = new List<RecetaDetalle>();
        while (reader.Read()) lista.Add(MapDetalle(reader));
        return lista;
    }

    private static RecetaDetalle MapDetalle(SqlDataReader r) => new()
    {
        Id_Receta = r.GetInt32(r.GetOrdinal("Id_Receta")),
        Nro_Receta = r.GetInt32(r.GetOrdinal("Nro_Receta")),
        Id_Estado = r.GetInt32(r.GetOrdinal("Id_Estado")),
        DescripcionEstado = r.GetString(r.GetOrdinal("DescripcionEstado")),
        Id_Medico = r.GetInt32(r.GetOrdinal("Id_Medico")),
        NroMatricula = r.GetString(r.GetOrdinal("NroMatricula")),
        NombreMedico = r.GetString(r.GetOrdinal("NombreMedico")),
        ApellidoMedico = r.GetString(r.GetOrdinal("ApellidoMedico")),
        Id_Paciente = r.GetInt32(r.GetOrdinal("Id_Paciente")),
        NroDocumento = r.GetString(r.GetOrdinal("NroDocumento")),
        TipoDocumento = r.GetString(r.GetOrdinal("TipoDocumento")),
        NombrePaciente = r.GetString(r.GetOrdinal("NombrePaciente")),
        ApellidoPaciente = r.GetString(r.GetOrdinal("ApellidoPaciente")),
        Fecha_Registro = r.IsDBNull(r.GetOrdinal("Fecha_Registro")) ? null : r.GetDateTime(r.GetOrdinal("Fecha_Registro")),
        Fecha_Entrega_Medico = r.IsDBNull(r.GetOrdinal("Fecha_Entrega_Medico")) ? null : r.GetDateTime(r.GetOrdinal("Fecha_Entrega_Medico")),
        Fecha_Recibe_Medico = r.IsDBNull(r.GetOrdinal("Fecha_Recibe_Medico")) ? null : r.GetDateTime(r.GetOrdinal("Fecha_Recibe_Medico")),
        Fecha_Entrega_Paciente = r.IsDBNull(r.GetOrdinal("Fecha_Entrega_Paciente")) ? null : r.GetDateTime(r.GetOrdinal("Fecha_Entrega_Paciente")),
        Fecha_Archivo = r.IsDBNull(r.GetOrdinal("Fecha_Archivo")) ? null : r.GetDateTime(r.GetOrdinal("Fecha_Archivo")),
        Fecha_Desecho = r.IsDBNull(r.GetOrdinal("Fecha_Desecho")) ? null : r.GetDateTime(r.GetOrdinal("Fecha_Desecho")),
        Nombre_Retira = r.IsDBNull(r.GetOrdinal("Nombre_Retira")) ? null : r.GetString(r.GetOrdinal("Nombre_Retira")),
        Parentezco = r.IsDBNull(r.GetOrdinal("Parentezco")) ? null : r.GetString(r.GetOrdinal("Parentezco")),
        DiasEnEstado3 = r.GetInt32(r.GetOrdinal("DiasEnEstado3"))
    };
}
