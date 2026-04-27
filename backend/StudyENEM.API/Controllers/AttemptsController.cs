using Microsoft.AspNetCore.Mvc;
using StudyENEM.API.DTOs;
using StudyENEM.API.Services;

namespace StudyENEM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AttemptsController(ExamService service) : ControllerBase
{
    [HttpPost("start")]
    public async Task<IActionResult> Start([FromBody] StartAttemptDto dto)
    {
        var id = await service.StartAttemptAsync(dto);
        return Ok(new { attemptId = id });
    }

    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] SubmitAttemptDto dto)
    {
        var result = await service.SubmitAttemptAsync(dto);
        return Ok(result);
    }

    [HttpGet("{id}/result")]
    public async Task<IActionResult> GetResult(int id)
    {
        var result = await service.GetAttemptResultAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }
}
