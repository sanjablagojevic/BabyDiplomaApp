using System.Security.Claims;
using BabyApp.Api.Data;
using BabyApp.Api.Models;
using BabyApp.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BabyApp.Api.Controllers;

[ApiController]
[Route("api/babies/{babyId:int}/milestones")]
[Authorize]
public class BabyMilestonesController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public BabyMilestonesController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("catalog")]
    public async Task<ActionResult<IReadOnlyList<CatalogItemDto>>> Catalog(int babyId, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        if (await BabyAccess.ForParentAsync(_db, babyId, userId, ct) is null)
            return NotFound();

        var items = MilestoneCatalog.All.Select(m => new CatalogItemDto(
            m.Key.ToString(),
            m.Title,
            m.TypicalFromWeeks,
            m.TypicalToWeeks)).ToList();
        return Ok(items);
    }

    [HttpGet("progress")]
    public async Task<ActionResult<ProgressDto>> Progress(int babyId, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var baby = await BabyAccess.ForParentAsync(_db, babyId, userId, ct);
        if (baby is null)
            return NotFound();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var ageWeeks = (int)((today.DayNumber - baby.DateOfBirth.DayNumber) / 7.0);

        var done = await _db.MilestoneAchievements.AsNoTracking()
            .Where(m => m.BabyId == babyId)
            .ToDictionaryAsync(m => m.Milestone, m => m.AchievedOn, ct);

        var items = MilestoneCatalog.All.Select(m =>
        {
            var achieved = done.TryGetValue(m.Key, out var d);
            var typical = ageWeeks >= m.TypicalFromWeeks && ageWeeks <= m.TypicalToWeeks;
            return new ProgressItemDto(
                m.Key.ToString(),
                m.Title,
                m.TypicalFromWeeks,
                m.TypicalToWeeks,
                achieved,
                achieved ? d : null,
                typical);
        }).ToList();

        var pct = items.Count == 0 ? 0 : Math.Round(100.0 * items.Count(i => i.Achieved) / items.Count, 0);
        return Ok(new ProgressDto(ageWeeks, pct, items));
    }

    [HttpPost("achievements")]
    public async Task<ActionResult<AchievementDto>> Add(int babyId, [FromBody] AchievementCreate body, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        if (await BabyAccess.ForParentAsync(_db, babyId, userId, ct) is null)
            return NotFound();

        var existing = await _db.MilestoneAchievements.FirstOrDefaultAsync(
            m => m.BabyId == babyId && m.Milestone == body.Milestone, ct);

        if (existing is null)
        {
            existing = new MilestoneAchievement
            {
                BabyId = babyId,
                Milestone = body.Milestone,
                AchievedOn = body.AchievedOn,
                Notes = body.Notes,
            };
            _db.MilestoneAchievements.Add(existing);
        }
        else
        {
            existing.AchievedOn = body.AchievedOn;
            existing.Notes = body.Notes;
        }

        await _db.SaveChangesAsync(ct);
        return Ok(new AchievementDto(existing.Id, existing.Milestone.ToString(), existing.AchievedOn, existing.Notes));
    }

    public record AchievementCreate(MilestoneKey Milestone, DateOnly AchievedOn, string? Notes);

    public record CatalogItemDto(string Key, string Title, int TypicalFromWeeks, int TypicalToWeeks);

    public record ProgressItemDto(
        string Key,
        string Title,
        int TypicalFromWeeks,
        int TypicalToWeeks,
        bool Achieved,
        DateOnly? AchievedOn,
        bool InTypicalWindowNow);

    public record ProgressDto(int AgeWeeksApprox, double CompletionPercent, IReadOnlyList<ProgressItemDto> Items);

    public record AchievementDto(int Id, string Milestone, DateOnly AchievedOn, string? Notes);
}
