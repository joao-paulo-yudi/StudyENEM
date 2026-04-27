namespace StudyENEM.API.DTOs;

public record QuestionDto(
    int Id,
    int Year,
    string Area,
    string Subject,
    string Statement,
    string OptionA,
    string OptionB,
    string OptionC,
    string OptionD,
    string OptionE
);

public record SubmitAnswerDto(int QuestionId, char SelectedOption);

public record StartAttemptDto(string StudentName, int? Year, string? Area);

public record SubmitAttemptDto(int AttemptId, List<SubmitAnswerDto> Answers);

public record AttemptResultDto(
    int AttemptId,
    string StudentName,
    DateTime StartedAt,
    DateTime FinishedAt,
    int TotalQuestions,
    int CorrectAnswers,
    double Score,
    List<AnswerResultDto> AnswerDetails
);

public record AnswerResultDto(
    int QuestionId,
    string Subject,
    string Area,
    char SelectedOption,
    char CorrectOption,
    bool IsCorrect
);

public record PerformanceSummaryDto(
    string StudentName,
    int TotalAttempts,
    List<AreaPerformanceDto> ByArea,
    List<SubjectPerformanceDto> BySubject,
    List<AttemptSummaryDto> RecentAttempts
);

public record AreaPerformanceDto(string Area, int Total, int Correct, double Percentage);
public record SubjectPerformanceDto(string Subject, string Area, int Total, int Correct, double Percentage);
public record AttemptSummaryDto(int AttemptId, DateTime Date, int Total, int Correct, double Score, string? Area);
