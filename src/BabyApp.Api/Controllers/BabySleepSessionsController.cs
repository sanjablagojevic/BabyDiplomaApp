using System.Security.Claims;
using BabyApp.Api.Data;
using BabyApp.Api.Models;
using BabyApp.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BabyApp.Api.Controllers;

[ApiController]
[Route("api/babies/{babyId:int}/sleep-sessions")]
[Authorize]
public class BabySleepSessionsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public BabySleepSessionsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SleepSessionDto>>> List(int babyId, [FromQuery] DateTimeOffset? from, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        if (await BabyAccess.ForParentAsync(_db, babyId, userId, ct) is null)
            return NotFound();

        var q = _db.SleepSessions.AsNoTracking().Where(s => s.BabyId == babyId);
        if (from.HasValue)
            q = q.Where(s => s.StartUtc >= from.Value);

        var rows = await q.OrderByDescending(s => s.StartUtc).Take(200).ToListAsync(ct);
        return Ok(rows.Select(SleepSessionDto.FromEntity).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<SleepSessionDto>> Create(int babyId, [FromBody] SleepSessionCreate body, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        if (await BabyAccess.ForParentAsync(_db, babyId, userId, ct) is null)
            return NotFound();

        var s = new SleepSession
        {
            BabyId = babyId,
            StartUtc = body.StartUtc,
            EndUtc = body.EndUtc,
            IsNap = body.IsNap,
            Notes = body.Notes,
        };
        _db.SleepSessions.Add(s);
        await _db.SaveChangesAsync(ct);
        return Ok(SleepSessionDto.FromEntity(s));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int babyId, int id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        if (await BabyAccess.ForParentAsync(_db, babyId, userId, ct) is null)
            return NotFound();

        var s = await _db.SleepSessions.FirstOrDefaultAsync(x => x.Id == id && x.BabyId == babyId, ct);
        if (s is null)
            return NotFound();
        _db.SleepSessions.Remove(s);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    public record SleepSessionCreate(DateTimeOffset StartUtc, DateTimeOffset? EndUtc, bool IsNap, string? Notes);

    public record SleepSessionDto(int Id, DateTimeOffset StartUtc, DateTimeOffset? EndUtc, bool IsNap, string? Notes)
    {
        public static SleepSessionDto FromEntity(SleepSession s) =>
            new(s.Id, s.StartUtc, s.EndUtc, s.IsNap, s.Notes);
    }
}
