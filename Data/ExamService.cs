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
    public async Task<List<Doctor>> GetDoctorsAsync()
    {
        var doctors = new List<Doctor>();
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT id, licence, nom_complet FROM docteur ORDER BY nom_complet", conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            doctors.Add(new Doctor
            {
                Id = reader.GetInt32(0),
                Licence = reader.GetString(1),
                NomComplet = reader.GetString(2)
            });
        }
        return doctors;
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

    public async Task AddPatientAsync(Patient patient)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(
            @"INSERT INTO patient (id, prenom, nom, sexe, date_naissance, langue, courriel, adresse_rue, adresse_appart, code_postal, ramq_date_exp, dossier_no, profession, date_creation, ne_pas_rappeler, est_decede, id_docteur, id_ville) 
              VALUES (@id, @prenom, @nom, @sexe, @dob, @langue, @email, @rue, @appart, @cp, @ramq, @dossier, @prof, @creation, @rappel, @decede, @docId, @villeId)", conn);
        
        cmd.Parameters.AddWithValue("id", patient.Id);
        cmd.Parameters.AddWithValue("prenom", patient.Prenom);
        cmd.Parameters.AddWithValue("nom", patient.Nom);
        cmd.Parameters.AddWithValue("sexe", (object?)patient.Sexe ?? DBNull.Value);
        cmd.Parameters.AddWithValue("dob", (object?)patient.DateNaissance ?? DBNull.Value);
        cmd.Parameters.AddWithValue("langue", patient.Langue);
        cmd.Parameters.AddWithValue("email", (object?)patient.Courriel ?? DBNull.Value);
        cmd.Parameters.AddWithValue("rue", (object?)patient.AdresseRue ?? DBNull.Value);
        cmd.Parameters.AddWithValue("appart", (object?)patient.AdresseAppart ?? DBNull.Value);
        cmd.Parameters.AddWithValue("cp", (object?)patient.CodePostal ?? DBNull.Value);
        cmd.Parameters.AddWithValue("ramq", (object?)patient.RamqDateExp ?? DBNull.Value);
        cmd.Parameters.AddWithValue("dossier", (object?)patient.DossierNo ?? DBNull.Value);
        cmd.Parameters.AddWithValue("prof", (object?)patient.Profession ?? DBNull.Value);
        cmd.Parameters.AddWithValue("creation", patient.DateCreation);
        cmd.Parameters.AddWithValue("rappel", patient.NePasRappeler);
        cmd.Parameters.AddWithValue("decede", patient.EstDecede);
        cmd.Parameters.AddWithValue("docId", (object?)patient.DocteurId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("villeId", (object?)patient.VilleId ?? DBNull.Value);

        await cmd.ExecuteNonQueryAsync();
    }
}
