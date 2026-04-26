using System.Security.Claims;
using BabyApp.Api.Data;
using BabyApp.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BabyApp.Api.Controllers;

[ApiController]
[Route("api/babies/{babyId:int}/insights")]
[Authorize]
public class BabyInsightsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly InsightsService _insights;

    public BabyInsightsController(ApplicationDbContext db, InsightsService insights)
    {
        _db = db;
        _insights = insights;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<string>>> Get(int babyId, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        if (await BabyAccess.ForParentAsync(_db, babyId, userId, ct) is null)
            return NotFound();

        var rows = await _insights.GetInsightsAsync(babyId, ct);
        return Ok(rows);
    }
}
