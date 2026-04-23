namespace OptometristApp.Data.Models;

public class Patient
{
    public int Id { get; set; }
    public string Prenom { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    
    public string FullName => $"{Prenom} {Nom}";
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
}
