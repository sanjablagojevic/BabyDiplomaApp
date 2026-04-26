using System.Security.Claims;
using BabyApp.Api.Data;
using BabyApp.Api.Models;
using BabyApp.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BabyApp.Api.Controllers;

[ApiController]
[Route("api/babies/{babyId:int}/reactions")]
[Authorize]
public class BabyReactionsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public BabyReactionsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ReactionDto>>> List(int babyId, [FromQuery] DateTimeOffset? from, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        if (await BabyAccess.ForParentAsync(_db, babyId, userId, ct) is null)
            return NotFound();

        var q = _db.ReactionLogs.AsNoTracking().Where(r => r.BabyId == babyId);
        if (from.HasValue)
            q = q.Where(r => r.OccurredUtc >= from.Value);

        var rows = await q.OrderByDescending(r => r.OccurredUtc).Take(200).ToListAsync(ct);
        return Ok(rows.Select(ReactionDto.FromEntity).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<ReactionDto>> Create(int babyId, [FromBody] ReactionCreate body, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        if (await BabyAccess.ForParentAsync(_db, babyId, userId, ct) is null)
            return NotFound();

        var r = new ReactionLog
        {
            BabyId = babyId,
            OccurredUtc = body.OccurredUtc,
            Kind = body.Kind,
            FoodTrigger = body.FoodTrigger,
            Notes = body.Notes,
        };
        _db.ReactionLogs.Add(r);
        await _db.SaveChangesAsync(ct);
        return Ok(ReactionDto.FromEntity(r));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int babyId, int id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        if (await BabyAccess.ForParentAsync(_db, babyId, userId, ct) is null)
            return NotFound();

        var r = await _db.ReactionLogs.FirstOrDefaultAsync(x => x.Id == id && x.BabyId == babyId, ct);
        if (r is null)
            return NotFound();
        _db.ReactionLogs.Remove(r);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    public record ReactionCreate(DateTimeOffset OccurredUtc, ReactionKind Kind, string? FoodTrigger, string? Notes);

    public record ReactionDto(int Id, DateTimeOffset OccurredUtc, string Kind, string? FoodTrigger, string? Notes)
    {
        public static ReactionDto FromEntity(ReactionLog r) =>
            new(r.Id, r.OccurredUtc, r.Kind.ToString(), r.FoodTrigger, r.Notes);
    }
}
