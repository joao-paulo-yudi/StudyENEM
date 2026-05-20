namespace StudyENEM.API.DTOs;

public record QuestionDto(
    int Id,
    int Year,
    string Area,
    string Subject,
    string Topic,
    string Difficulty,
    string Statement,
    string OptionA,
    string OptionB,
    string OptionC,
    string OptionD,
    string OptionE
);

public record SubmitAnswerDto(int QuestionId, char SelectedOption);

public record StartAttemptDto(string StudentName, string Mode, int? Count, int? Year, string? Area);

public record SubmitAttemptDto(int AttemptId, int? TimeTakenSeconds, List<SubmitAnswerDto> Answers);

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
    string Topic,
    string Area,
    char SelectedOption,
    char CorrectOption,
    bool IsCorrect
);

public record PerformanceSummaryDto(
    string StudentName,
    int TotalAttempts,
    int TotalQuestions,
    int TotalCorrect,
    int TotalTimeSeconds,
    List<AreaPerformanceDto> ByArea,
    List<SubjectPerformanceDto> BySubject,
    List<AttemptSummaryDto> RecentAttempts,
    List<StudyPlanItemDto> StudyPlan
);

public record AreaPerformanceDto(string Area, int Total, int Correct, double Percentage);
public record SubjectPerformanceDto(string Subject, string Area, int Total, int Correct, double Percentage);
public record AttemptSummaryDto(int AttemptId, DateTime Date, int Total, int Correct, double Score, string? Area, string? Mode, int? TimeTakenSeconds);
public record StudyPlanItemDto(string Topic, string Area, string Priority, int Mastery, int Attempts, string Reason);
