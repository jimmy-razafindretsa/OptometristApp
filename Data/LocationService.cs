using Npgsql;
using OptometristApp.Data.Models;

namespace OptometristApp.Data;

public class LocationService
{
    private readonly NpgsqlDataSource _dataSource;

    public LocationService(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<List<City>> GetCitiesAsync()
    {
        var cities = new List<City>();
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT id, nom, province FROM ville ORDER BY nom", conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            cities.Add(new City
            {
                Id = reader.GetInt32(0),
                Nom = reader.GetString(1),
                Province = reader.GetString(2)
            });
        }
        return cities;
    }

    public async Task<List<Clinique>> GetCliniquesAsync()
    {
        var cliniques = new List<Clinique>();
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT id, nom, id_ville FROM clinique ORDER BY nom", conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            cliniques.Add(new Clinique
            {
                Id = reader.GetInt32(0),
                Nom = reader.GetString(1),
                VilleId = reader.IsDBNull(2) ? null : reader.GetInt32(2)
            });
        }
        return cliniques;
    }
}
