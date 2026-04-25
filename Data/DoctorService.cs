using Npgsql;
using OptometristApp.Data.Models;

namespace OptometristApp.Data;

public class DoctorService
{
    private readonly NpgsqlDataSource _dataSource;

    public DoctorService(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<List<Doctor>> GetDoctorsAsync()
    {
        var doctors = new List<Doctor>();
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(@"
            SELECT d.id, d.licence, d.nom_complet, d.id_clinique, c.nom as clinique_nom 
            FROM docteur d 
            LEFT JOIN clinique c ON d.id_clinique = c.id 
            ORDER BY d.nom_complet", conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            doctors.Add(new Doctor
            {
                Id = reader.GetInt32(0),
                Licence = reader.GetString(1),
                NomComplet = reader.GetString(2),
                CliniqueId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                CliniqueName = reader.IsDBNull(4) ? null : reader.GetString(4)
            });
        }
        return doctors;
    }

    public async Task<Doctor?> GetDoctorByIdAsync(int id)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT * FROM docteur WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Doctor
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Licence = reader.GetString(reader.GetOrdinal("licence")),
                NomComplet = reader.GetString(reader.GetOrdinal("nom_complet")),
                CliniqueId = reader.IsDBNull(reader.GetOrdinal("id_clinique")) ? null : reader.GetInt32(reader.GetOrdinal("id_clinique"))
            };
        }
        return null;
    }

    public async Task AddDoctorAsync(Doctor doctor)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(
            "INSERT INTO docteur (licence, nom_complet, id_clinique) VALUES (@licence, @nom, @cliniqueId)", conn);
        cmd.Parameters.AddWithValue("licence", doctor.Licence);
        cmd.Parameters.AddWithValue("nom", doctor.NomComplet);
        cmd.Parameters.AddWithValue("cliniqueId", (object?)doctor.CliniqueId ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateDoctorAsync(Doctor doctor)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(
            "UPDATE docteur SET licence = @licence, nom_complet = @nom, id_clinique = @cliniqueId WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("licence", doctor.Licence);
        cmd.Parameters.AddWithValue("nom", doctor.NomComplet);
        cmd.Parameters.AddWithValue("cliniqueId", (object?)doctor.CliniqueId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("id", doctor.Id);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteDoctorAsync(int id)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("DELETE FROM docteur WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        await cmd.ExecuteNonQueryAsync();
    }
}
