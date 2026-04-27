using Microsoft.AspNetCore.Mvc;
using StudyENEM.API.Services;

namespace StudyENEM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController(ExamService service) : ControllerBase
{
    [HttpGet("performance/{studentName}")]
    public async Task<IActionResult> GetPerformance(string studentName) =>
        Ok(await service.GetPerformanceSummaryAsync(studentName));
}
