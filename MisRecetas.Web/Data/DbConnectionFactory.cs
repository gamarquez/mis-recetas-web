using Microsoft.Data.SqlClient;

namespace MisRecetas.Web.Data;

/// <summary>
/// Factoría de conexiones ADO.NET. Registrada como Scoped en DI.
/// </summary>
public class DbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(IConfiguration config)
        => _connectionString = config.GetConnectionString("ControlRecetasDB")!;

    public SqlConnection CreateConnection() => new SqlConnection(_connectionString);
}
