using Npgsql;
using OptometristApp.Data.Models;

namespace OptometristApp.Data;

public class PatientService
{
    private readonly NpgsqlDataSource _dataSource;

    public PatientService(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<List<Patient>> GetPatientsAsync()
    {
        var patients = new List<Patient>();
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(@"
            SELECT id, prenom, nom, sexe, date_naissance, langue, courriel, adresse_rue, 
                   adresse_appart, code_postal, ramq_date_exp, dossier_no, profession, 
                   date_creation, ne_pas_rappeler, est_decede, id_docteur, id_ville 
            FROM patient 
            ORDER BY nom, prenom", conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            patients.Add(MapPatient(reader));
        }
        return patients;
    }

    public async Task<Patient?> GetPatientByIdAsync(int id)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT * FROM patient WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapPatient(reader);
        }
        return null;
    }

    public async Task AddPatientAsync(Patient patient)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(
            @"INSERT INTO patient (id, prenom, nom, sexe, date_naissance, langue, courriel, adresse_rue, adresse_appart, code_postal, ramq_date_exp, dossier_no, profession, date_creation, ne_pas_rappeler, est_decede, id_docteur, id_ville) 
              VALUES (@id, @prenom, @nom, @sexe, @dob, @langue, @email, @rue, @appart, @cp, @ramq, @dossier, @prof, @creation, @rappel, @decede, @docId, @villeId)", conn);
        
        AddPatientParameters(cmd, patient);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdatePatientAsync(Patient patient)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(
            @"UPDATE patient SET 
                prenom = @prenom, nom = @nom, sexe = @sexe, date_naissance = @dob, 
                langue = @langue, courriel = @email, adresse_rue = @rue, 
                adresse_appart = @appart, code_postal = @cp, ramq_date_exp = @ramq, 
                dossier_no = @dossier, profession = @prof, date_creation = @creation, 
                ne_pas_rappeler = @rappel, est_decede = @decede, id_docteur = @docId, 
                id_ville = @villeId 
              WHERE id = @id", conn);
        
        AddPatientParameters(cmd, patient);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeletePatientAsync(int id)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("DELETE FROM patient WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    private static Patient MapPatient(NpgsqlDataReader reader)
    {
        return new Patient
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            Prenom = reader.GetString(reader.GetOrdinal("prenom")),
            Nom = reader.GetString(reader.GetOrdinal("nom")),
            Sexe = reader.IsDBNull(reader.GetOrdinal("sexe")) ? null : reader.GetString(reader.GetOrdinal("sexe")),
            DateNaissance = reader.IsDBNull(reader.GetOrdinal("date_naissance")) ? null : reader.GetDateTime(reader.GetOrdinal("date_naissance")),
            Langue = reader.GetString(reader.GetOrdinal("langue")),
            Courriel = reader.IsDBNull(reader.GetOrdinal("courriel")) ? null : reader.GetString(reader.GetOrdinal("courriel")),
            AdresseRue = reader.IsDBNull(reader.GetOrdinal("adresse_rue")) ? null : reader.GetString(reader.GetOrdinal("adresse_rue")),
            AdresseAppart = reader.IsDBNull(reader.GetOrdinal("adresse_appart")) ? null : reader.GetString(reader.GetOrdinal("adresse_appart")),
            CodePostal = reader.IsDBNull(reader.GetOrdinal("code_postal")) ? null : reader.GetString(reader.GetOrdinal("code_postal")),
            RamqDateExp = reader.IsDBNull(reader.GetOrdinal("ramq_date_exp")) ? null : reader.GetDateTime(reader.GetOrdinal("ramq_date_exp")),
            DossierNo = reader.IsDBNull(reader.GetOrdinal("dossier_no")) ? null : reader.GetInt32(reader.GetOrdinal("dossier_no")),
            Profession = reader.IsDBNull(reader.GetOrdinal("profession")) ? null : reader.GetString(reader.GetOrdinal("profession")),
            DateCreation = reader.GetDateTime(reader.GetOrdinal("date_creation")),
            NePasRappeler = reader.GetBoolean(reader.GetOrdinal("ne_pas_rappeler")),
            EstDecede = reader.GetBoolean(reader.GetOrdinal("est_decede")),
            DocteurId = reader.IsDBNull(reader.GetOrdinal("id_docteur")) ? null : reader.GetInt32(reader.GetOrdinal("id_docteur")),
            VilleId = reader.IsDBNull(reader.GetOrdinal("id_ville")) ? null : reader.GetInt32(reader.GetOrdinal("id_ville"))
        };
    }

    private static void AddPatientParameters(NpgsqlCommand cmd, Patient patient)
    {
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
    }
}
