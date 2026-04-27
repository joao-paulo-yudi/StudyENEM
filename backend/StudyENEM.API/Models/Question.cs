namespace StudyENEM.API.Models;

public class Question
{
    public int Id { get; set; }
    public int Year { get; set; }
    public string Area { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Statement { get; set; } = string.Empty;
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;
    public string OptionE { get; set; } = string.Empty;
    public char CorrectOption { get; set; }
    public ICollection<AttemptAnswer> Answers { get; set; } = new List<AttemptAnswer>();
}
