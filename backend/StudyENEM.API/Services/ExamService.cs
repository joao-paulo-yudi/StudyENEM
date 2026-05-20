using Microsoft.EntityFrameworkCore;
using StudyENEM.API.Data;
using StudyENEM.API.DTOs;
using StudyENEM.API.Models;

namespace StudyENEM.API.Services;

public class ExamService(AppDbContext db)
{
    public async Task<List<QuestionDto>> GetQuestionsAsync(int? year, string? area, int? count)
    {
        var query = db.Questions.AsQueryable();
        if (year.HasValue) query = query.Where(q => q.Year == year);
        if (!string.IsNullOrEmpty(area)) query = query.Where(q => q.Area == area);

        var list = await query.Select(q => new QuestionDto(
            q.Id, q.Year, q.Area, q.Subject, q.Topic, q.Difficulty, q.Statement,
            q.OptionA, q.OptionB, q.OptionC, q.OptionD, q.OptionE
        )).ToListAsync();

        if (list.Count == 0) return list;

        var rng = new Random();
        list = list.OrderBy(_ => rng.Next()).ToList();

        if (count.HasValue && count.Value > 0)
        {
            var result = new List<QuestionDto>(count.Value);
            for (int i = 0; i < count.Value; i++)
                result.Add(list[i % list.Count]);
            return result;
        }

        return list;
    }

    public async Task<List<int>> GetAvailableYearsAsync() =>
        await db.Questions.Select(q => q.Year).Distinct().OrderByDescending(y => y).ToListAsync();

    public async Task<List<string>> GetAvailableAreasAsync() =>
        await db.Questions.Select(q => q.Area).Distinct().OrderBy(a => a).ToListAsync();

    public async Task<int> StartAttemptAsync(StartAttemptDto dto)
    {
        var attempt = new Attempt
        {
            StudentName = dto.StudentName,
            StartedAt = DateTime.UtcNow,
            Year = dto.Year,
            Area = dto.Area,
            Mode = string.IsNullOrEmpty(dto.Mode) ? "geral" : dto.Mode,
        };
        db.Attempts.Add(attempt);
        await db.SaveChangesAsync();
        return attempt.Id;
    }

    public async Task<AttemptResultDto> SubmitAttemptAsync(SubmitAttemptDto dto)
    {
        var attempt = await db.Attempts.FindAsync(dto.AttemptId)
            ?? throw new KeyNotFoundException($"Attempt {dto.AttemptId} not found");

        var questionIds = dto.Answers.Select(a => a.QuestionId).Distinct().ToList();
        var questions = await db.Questions
            .Where(q => questionIds.Contains(q.Id))
            .ToDictionaryAsync(q => q.Id);

        var answers = dto.Answers.Select(a =>
        {
            var q = questions[a.QuestionId];
            return new AttemptAnswer
            {
                AttemptId = attempt.Id,
                QuestionId = a.QuestionId,
                SelectedOption = a.SelectedOption,
                IsCorrect = char.ToUpper(a.SelectedOption) == char.ToUpper(q.CorrectOption)
            };
        }).ToList();

        db.AttemptAnswers.AddRange(answers);
        attempt.FinishedAt = DateTime.UtcNow;
        attempt.TimeTakenSeconds = dto.TimeTakenSeconds;
        await db.SaveChangesAsync();

        var details = answers.Select(a => new AnswerResultDto(
            a.QuestionId,
            questions[a.QuestionId].Subject,
            questions[a.QuestionId].Topic,
            questions[a.QuestionId].Area,
            a.SelectedOption,
            questions[a.QuestionId].CorrectOption,
            a.IsCorrect
        )).ToList();

        int correct = answers.Count(a => a.IsCorrect);
        return new AttemptResultDto(
            attempt.Id, attempt.StudentName, attempt.StartedAt, attempt.FinishedAt!.Value,
            answers.Count, correct,
            answers.Count > 0 ? Math.Round((double)correct / answers.Count * 100, 1) : 0,
            details
        );
    }

    public async Task<AttemptResultDto?> GetAttemptResultAsync(int attemptId)
    {
        var attempt = await db.Attempts
            .Include(a => a.Answers).ThenInclude(a => a.Question)
            .FirstOrDefaultAsync(a => a.Id == attemptId);

        if (attempt == null || attempt.FinishedAt == null) return null;

        var details = attempt.Answers.Select(a => new AnswerResultDto(
            a.QuestionId, a.Question.Subject, a.Question.Topic, a.Question.Area,
            a.SelectedOption, a.Question.CorrectOption, a.IsCorrect
        )).ToList();

        int correct = attempt.Answers.Count(a => a.IsCorrect);
        return new AttemptResultDto(
            attempt.Id, attempt.StudentName, attempt.StartedAt, attempt.FinishedAt.Value,
            attempt.Answers.Count, correct,
            attempt.Answers.Count > 0 ? Math.Round((double)correct / attempt.Answers.Count * 100, 1) : 0,
            details
        );
    }

    public async Task<PerformanceSummaryDto> GetPerformanceSummaryAsync(string studentName)
    {
        var attempts = await db.Attempts
            .Include(a => a.Answers).ThenInclude(a => a.Question)
            .Where(a => a.StudentName == studentName && a.FinishedAt != null)
            .OrderByDescending(a => a.FinishedAt)
            .ToListAsync();

        var allAnswers = attempts.SelectMany(a => a.Answers).ToList();
        int totalQuestions = allAnswers.Count;
        int totalCorrect = allAnswers.Count(a => a.IsCorrect);
        int totalTimeSeconds = attempts.Sum(a => a.TimeTakenSeconds ?? 0);

        var byArea = allAnswers
            .GroupBy(a => a.Question.Area)
            .Select(g => new AreaPerformanceDto(
                g.Key, g.Count(), g.Count(a => a.IsCorrect),
                g.Count() > 0 ? Math.Round((double)g.Count(a => a.IsCorrect) / g.Count() * 100, 1) : 0
            )).ToList();

        var bySubject = allAnswers
            .GroupBy(a => new { a.Question.Subject, a.Question.Area })
            .Select(g => new SubjectPerformanceDto(
                g.Key.Subject, g.Key.Area, g.Count(), g.Count(a => a.IsCorrect),
                g.Count() > 0 ? Math.Round((double)g.Count(a => a.IsCorrect) / g.Count() * 100, 1) : 0
            )).OrderBy(s => s.Percentage).ToList();

        var recent = attempts.Take(10).Select(a => new AttemptSummaryDto(
            a.Id, a.FinishedAt!.Value, a.Answers.Count, a.Answers.Count(ans => ans.IsCorrect),
            a.Answers.Count > 0 ? Math.Round((double)a.Answers.Count(ans => ans.IsCorrect) / a.Answers.Count * 100, 1) : 0,
            a.Area, a.Mode, a.TimeTakenSeconds
        )).ToList();

        var studyPlan = allAnswers
            .Where(a => !string.IsNullOrEmpty(a.Question.Topic))
            .GroupBy(a => new { a.Question.Topic, a.Question.Area })
            .Select(g =>
            {
                int total = g.Count();
                int correct = g.Count(a => a.IsCorrect);
                int mastery = total > 0 ? (int)Math.Round((double)correct / total * 100) : 0;
                string priority = mastery < 40 ? "alta" : mastery < 60 ? "média" : "baixa";
                string reason = $"{mastery}% de acertos em {total} questão{(total > 1 ? "ões" : "")} respondida{(total > 1 ? "s" : "")}";
                return new StudyPlanItemDto(g.Key.Topic, g.Key.Area, priority, mastery, total, reason);
            })
            .OrderBy(t => t.Mastery)
            .Take(12)
            .ToList();

        return new PerformanceSummaryDto(
            studentName, attempts.Count, totalQuestions, totalCorrect, totalTimeSeconds,
            byArea, bySubject, recent, studyPlan
        );
    }
}
