using Npgsql;
using OptometristApp.Data.Models;

namespace OptometristApp.Data;

public class ExamService
{
    private readonly NpgsqlDataSource _dataSource;

    public ExamService(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<List<Exam>> GetExamsAsync()
    {
        var exams = new List<Exam>();
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(@"
            SELECT e.id, e.id_patient, e.id_liste_examen, e.date_examen, 
                   p.prenom || ' ' || p.nom as patient_nom, 
                   le.nom as examen_nom 
            FROM examen e 
            JOIN patient p ON e.id_patient = p.id 
            JOIN liste_examen le ON e.id_liste_examen = le.id 
            ORDER BY e.date_examen DESC", conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            exams.Add(new Exam
            {
                Id = reader.GetInt32(0),
                PatientId = reader.GetInt32(1),
                ExamTypeId = reader.GetInt32(2),
                DateExamen = reader.GetDateTime(3),
                PatientName = reader.GetString(4),
                ExamTypeName = reader.GetString(5)
            });
        }
        return exams;
    }

    public async Task<Exam?> GetExamByIdAsync(int id)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT * FROM examen WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Exam
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                PatientId = reader.GetInt32(reader.GetOrdinal("id_patient")),
                ExamTypeId = reader.GetInt32(reader.GetOrdinal("id_liste_examen")),
                DateExamen = reader.GetDateTime(reader.GetOrdinal("date_examen"))
            };
        }
        return null;
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

    public async Task UpdateExamAsync(Exam exam)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(
            "UPDATE examen SET id_patient = @pId, id_liste_examen = @eId, date_examen = @date WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("pId", exam.PatientId);
        cmd.Parameters.AddWithValue("eId", exam.ExamTypeId);
        cmd.Parameters.AddWithValue("date", exam.DateExamen);
        cmd.Parameters.AddWithValue("id", exam.Id);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteExamAsync(int id)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("DELETE FROM examen WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        await cmd.ExecuteNonQueryAsync();
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
}
