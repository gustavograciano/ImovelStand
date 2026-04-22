using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ImovelStand.Infrastructure.Persistence;

namespace ImovelStand.Api.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public HealthController(ApplicationDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> Check(CancellationToken ct)
    {
        try
        {
            await _context.Database.ExecuteSqlRawAsync("SELECT 1", ct);
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                version = typeof(HealthController).Assembly.GetName().Version?.ToString()
            });
        }
        catch (Exception ex)
        {
            return StatusCode(503, new { status = "unhealthy", error = ex.Message });
        }
    }
}
