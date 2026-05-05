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

    public async Task<PatientDetailDto?> GetPatientDetailAsync(int id)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        
        await using var cmdPatient = new NpgsqlCommand(@"
            SELECT p.*, 
                   d.id as doc_id, d.licence, d.nom_complet as doc_nom, d.id_clinique as doc_clinique_id,
                   c.id as clin_id, c.nom as clin_nom, c.id_ville as clin_ville_id,
                   v.id as ville_id, v.nom as ville_nom, v.province as ville_prov
            FROM patient p
            LEFT JOIN docteur d ON p.id_docteur = d.id
            LEFT JOIN clinique c ON d.id_clinique = c.id
            LEFT JOIN ville v ON p.id_ville = v.id
            WHERE p.id = @id", conn);
        cmdPatient.Parameters.AddWithValue("id", id);
        
        PatientDetailDto? detail = null;
        await using (var reader = await cmdPatient.ExecuteReaderAsync())
        {
            if (await reader.ReadAsync())
            {
                detail = new PatientDetailDto
                {
                    Patient = MapPatient(reader),
                    Doctor = reader.IsDBNull(reader.GetOrdinal("doc_id")) ? null : new Doctor
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("doc_id")),
                        Licence = reader.GetString(reader.GetOrdinal("licence")),
                        NomComplet = reader.GetString(reader.GetOrdinal("doc_nom")),
                        CliniqueId = reader.IsDBNull(reader.GetOrdinal("doc_clinique_id")) ? null : reader.GetInt32(reader.GetOrdinal("doc_clinique_id"))
                    },
                    Clinique = reader.IsDBNull(reader.GetOrdinal("clin_id")) ? null : new Clinique
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("clin_id")),
                        Nom = reader.GetString(reader.GetOrdinal("clin_nom")),
                        VilleId = reader.IsDBNull(reader.GetOrdinal("clin_ville_id")) ? null : reader.GetInt32(reader.GetOrdinal("clin_ville_id"))
                    },
                    City = reader.IsDBNull(reader.GetOrdinal("ville_id")) ? null : new City
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("ville_id")),
                        Nom = reader.GetString(reader.GetOrdinal("ville_nom")),
                        Province = reader.GetString(reader.GetOrdinal("ville_prov"))
                    }
                };
            }
        }

        if (detail == null) return null;

        await using var cmdPhones = new NpgsqlCommand("SELECT id, id_patient, numero, type_tel FROM telephone WHERE id_patient = @id", conn);
        cmdPhones.Parameters.AddWithValue("id", id);
        await using (var reader = await cmdPhones.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                detail.Phones.Add(new Phone
                {
                    Id = reader.GetInt32(0),
                    PatientId = reader.GetInt32(1),
                    Numero = reader.GetString(2),
                    TypeTel = reader.IsDBNull(3) ? null : reader.GetString(3)
                });
            }
        }

        await using var cmdExams = new NpgsqlCommand(@"
            SELECT e.id, e.id_patient, e.id_liste_examen, e.date_examen, le.nom as exam_nom 
            FROM examen e 
            JOIN liste_examen le ON e.id_liste_examen = le.id 
            WHERE e.id_patient = @id 
            ORDER BY e.date_examen DESC", conn);
        cmdExams.Parameters.AddWithValue("id", id);
        await using (var reader = await cmdExams.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                detail.Exams.Add(new Exam
                {
                    Id = reader.GetInt32(0),
                    PatientId = reader.GetInt32(1),
                    ExamTypeId = reader.GetInt32(2),
                    DateExamen = reader.GetDateTime(3),
                    ExamTypeName = reader.GetString(4)
                });
            }
        }

        return detail;
    }

    public async Task AddPatientAsync(Patient patient)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(
            @"INSERT INTO patient (prenom, nom, sexe, date_naissance, langue, courriel, adresse_rue, adresse_appart, code_postal, ramq_date_exp, dossier_no, profession, date_creation, ne_pas_rappeler, est_decede, id_docteur, id_ville) 
              VALUES (@prenom, @nom, @sexe, @dob, @langue, @email, @rue, @appart, @cp, @ramq, @dossier, @prof, @creation, @rappel, @decede, @docId, @villeId)", conn);
        
        AddPatientParameters(cmd, patient, false);
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
        
        AddPatientParameters(cmd, patient, true);
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

    private static void AddPatientParameters(NpgsqlCommand cmd, Patient patient, bool includeId = true)
    {
        if (includeId)
        {
            cmd.Parameters.AddWithValue("id", patient.Id);
        }
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
