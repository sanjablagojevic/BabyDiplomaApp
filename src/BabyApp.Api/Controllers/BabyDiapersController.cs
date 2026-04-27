using System.Security.Claims;
using BabyApp.Api.Data;
using BabyApp.Api.Models;
using BabyApp.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BabyApp.Api.Controllers;

[ApiController]
[Route("api/babies/{babyId:int}/diapers")]
[Authorize]
public class BabyDiapersController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public BabyDiapersController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("logs")]
    public async Task<ActionResult<IReadOnlyList<DiaperDto>>> Logs(int babyId, [FromQuery] DateTimeOffset? from, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        if (await BabyAccess.ForParentAsync(_db, babyId, userId, ct) is null)
            return NotFound();

        var q = _db.DiaperLogs.AsNoTracking().Where(x => x.BabyId == babyId);
        if (from.HasValue)
            q = q.Where(x => x.OccurredUtc >= from.Value);

        var rows = await q.OrderByDescending(x => x.OccurredUtc).Take(400).ToListAsync(ct);
        return Ok(rows.Select(DiaperDto.FromEntity).ToList());
    }

    [HttpPost("logs")]
    public async Task<ActionResult<DiaperDto>> Add(int babyId, [FromBody] DiaperCreate body, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        if (await BabyAccess.ForParentAsync(_db, babyId, userId, ct) is null)
            return NotFound();

        var d = new DiaperLog
        {
            BabyId = babyId,
            OccurredUtc = body.OccurredUtc,
            Type = body.Type,
            Notes = body.Notes,
        };
        _db.DiaperLogs.Add(d);
        await _db.SaveChangesAsync(ct);
        return Ok(DiaperDto.FromEntity(d));
    }

    [HttpDelete("logs/{id:int}")]
    public async Task<IActionResult> Delete(int babyId, int id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        if (await BabyAccess.ForParentAsync(_db, babyId, userId, ct) is null)
            return NotFound();

        var row = await _db.DiaperLogs.FirstOrDefaultAsync(x => x.BabyId == babyId && x.Id == id, ct);
        if (row is null)
            return NotFound();

        _db.DiaperLogs.Remove(row);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    public record DiaperCreate(DateTimeOffset OccurredUtc, DiaperType Type, string? Notes);

    public record DiaperDto(int Id, DateTimeOffset OccurredUtc, string Type, string? Notes)
    {
        public static DiaperDto FromEntity(DiaperLog x) => new(x.Id, x.OccurredUtc, x.Type.ToString(), x.Notes);
    }
}
