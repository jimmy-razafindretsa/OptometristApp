using Npgsql;

namespace OptometristApp.Data;

public class DatabaseConnexionService
{
    private readonly string _connectionString;
    public DatabaseConnexionService(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection");
        
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(_connectionString);
        var dataSource = dataSourceBuilder.Build();

        var conn =  dataSource.OpenConnectionAsync();
    }
}