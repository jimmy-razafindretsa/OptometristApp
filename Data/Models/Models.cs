namespace OptometristApp.Data.Models;

public class Patient
{
    public int Id { get; set; }
    public string Prenom { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string? Sexe { get; set; }
    public DateTime? DateNaissance { get; set; }
    public string Langue { get; set; } = "Français";
    public string? Courriel { get; set; }
    public string? AdresseRue { get; set; }
    public string? AdresseAppart { get; set; }
    public string? CodePostal { get; set; }
    public DateTime? RamqDateExp { get; set; }
    public int? DossierNo { get; set; }
    public string? Profession { get; set; }
    public DateTime DateCreation { get; set; } = DateTime.Today;
    public bool NePasRappeler { get; set; } = false;
    public bool EstDecede { get; set; } = false;
    public int? DocteurId { get; set; }
    public int? VilleId { get; set; }
    
    public string FullName => $"{Prenom} {Nom}";
}

public class Doctor
{
    public int Id { get; set; }
    public string Licence { get; set; } = string.Empty;
    public string NomComplet { get; set; } = string.Empty;
    public int? CliniqueId { get; set; }
    public string? CliniqueName { get; set; }
}

public class Clinique
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public int? VilleId { get; set; }
}

public class City
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Province { get; set; } = "Québec";
}

public class ExamType
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
}

public class Exam
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int ExamTypeId { get; set; }
    public DateTime DateExamen { get; set; } = DateTime.Today;
    
    public string? PatientName { get; set; }
    public string? ExamTypeName { get; set; }
}
