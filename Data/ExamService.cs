using Npgsql;
using OptometristApp.Data.Models;

namespace OptometristApp.Data;

public class ExamService
{
    private readonly NpgsqlDataSource _dataSource;

    public ExamService(IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection");
        _dataSource = NpgsqlDataSource.Create(connectionString);
    }

    public async Task<List<Patient>> GetPatientsAsync()
    {
        var patients = new List<Patient>();
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT id, prenom, nom FROM patient ORDER BY nom, prenom", conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            patients.Add(new Patient
            {
                Id = reader.GetInt32(0),
                Prenom = reader.GetString(1),
                Nom = reader.GetString(2)
            });
        }
        return patients;
    }

    public async Task<List<ExamType>> GetExamTypesAsync()
    {
        var examTypes = new List<ExamType>();
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT id, nom FROM liste_examen ORDER BY nom", conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            examTypes.Add(new ExamType
            {
                Id = reader.GetInt32(0),
                Nom = reader.GetString(1)
            });
        }
        return examTypes;
    }

    public async Task AddExamAsync(Exam exam)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(
            "INSERT INTO examen (id_patient, id_liste_examen, date_examen) VALUES (@pId, @eId, @date)", conn);
        cmd.Parameters.AddWithValue("pId", exam.PatientId);
        cmd.Parameters.AddWithValue("eId", exam.ExamTypeId);
        cmd.Parameters.AddWithValue("date", exam.DateExamen);
        await cmd.ExecuteNonQueryAsync();
    }
}
