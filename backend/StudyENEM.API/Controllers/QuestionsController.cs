using Microsoft.AspNetCore.Mvc;
using StudyENEM.API.DTOs;
using StudyENEM.API.Services;

namespace StudyENEM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuestionsController(ExamService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetQuestions([FromQuery] int? year, [FromQuery] string? area, [FromQuery] int? count) =>
        Ok(await service.GetQuestionsAsync(year, area, count));

    [HttpGet("years")]
    public async Task<IActionResult> GetYears() =>
        Ok(await service.GetAvailableYearsAsync());

    [HttpGet("areas")]
    public async Task<IActionResult> GetAreas() =>
        Ok(await service.GetAvailableAreasAsync());
}
