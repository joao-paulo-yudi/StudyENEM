namespace StudyENEM.API.Models;

public class Attempt
{
    public int Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public int? Year { get; set; }
    public string? Area { get; set; }
    public string Mode { get; set; } = "geral";
    public int? TimeTakenSeconds { get; set; }
    public ICollection<AttemptAnswer> Answers { get; set; } = new List<AttemptAnswer>();
}
