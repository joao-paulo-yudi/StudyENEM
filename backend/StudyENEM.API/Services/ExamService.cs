using Microsoft.EntityFrameworkCore;
using StudyENEM.API.Data;
using StudyENEM.API.DTOs;
using StudyENEM.API.Models;

namespace StudyENEM.API.Services;

public class ExamService(AppDbContext db)
{
    public async Task<List<QuestionDto>> GetQuestionsAsync(int? year, string? area)
    {
        var query = db.Questions.AsQueryable();
        if (year.HasValue) query = query.Where(q => q.Year == year);
        if (!string.IsNullOrEmpty(area)) query = query.Where(q => q.Area == area);

        return await query.Select(q => new QuestionDto(
            q.Id, q.Year, q.Area, q.Subject, q.Statement,
            q.OptionA, q.OptionB, q.OptionC, q.OptionD, q.OptionE
        )).ToListAsync();
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
            Area = dto.Area
        };
        db.Attempts.Add(attempt);
        await db.SaveChangesAsync();
        return attempt.Id;
    }

    public async Task<AttemptResultDto> SubmitAttemptAsync(SubmitAttemptDto dto)
    {
        var attempt = await db.Attempts.FindAsync(dto.AttemptId)
            ?? throw new KeyNotFoundException($"Attempt {dto.AttemptId} not found");

        var questionIds = dto.Answers.Select(a => a.QuestionId).ToList();
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
        await db.SaveChangesAsync();

        var details = answers.Select(a => new AnswerResultDto(
            a.QuestionId,
            questions[a.QuestionId].Subject,
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
            a.QuestionId, a.Question.Subject, a.Question.Area,
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
            a.Area
        )).ToList();

        return new PerformanceSummaryDto(studentName, attempts.Count, byArea, bySubject, recent);
    }
}
