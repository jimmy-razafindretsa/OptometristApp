using Npgsql;

namespace OptometristApp.Data;

public class DatabaseConnexionService
{
    private readonly NpgsqlDataSource _dataSource;

    public DatabaseConnexionService(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    // This is our test method
    public async Task<bool> IsConnectionHealthy()
    {
        try 
        {
            // 'await using' ensures the connection is closed automatically when done
            await using var conn = await _dataSource.OpenConnectionAsync();
            
            // If we get here, it worked!
            return true; 
        }
        catch (Exception ex)
        {
            // If the DB is down or credentials are wrong, it fails here
            Console.WriteLine($"Database connection failed: {ex.Message}");
            return false;
        }
    }
}