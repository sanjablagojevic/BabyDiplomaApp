using System.Security.Claims;
using BabyApp.Api.Data;
using BabyApp.Api.Models;
using BabyApp.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BabyApp.Api.Controllers;

[ApiController]
[Route("api/babies/{babyId:int}/feeding")]
[Authorize]
public class BabyFeedingController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public BabyFeedingController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("logs")]
    public async Task<ActionResult<IReadOnlyList<FeedingDto>>> Logs(int babyId, [FromQuery] DateTimeOffset? from, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        if (await BabyAccess.ForParentAsync(_db, babyId, userId, ct) is null)
            return NotFound();

        var q = _db.FeedingLogs.AsNoTracking().Where(f => f.BabyId == babyId);
        if (from.HasValue)
            q = q.Where(f => f.StartUtc >= from.Value);

        var rows = await q.OrderByDescending(f => f.StartUtc).Take(300).ToListAsync(ct);
        return Ok(rows.Select(FeedingDto.FromEntity).ToList());
    }

    [HttpPost("logs")]
    public async Task<ActionResult<FeedingDto>> AddLog(int babyId, [FromBody] FeedingCreate body, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        if (await BabyAccess.ForParentAsync(_db, babyId, userId, ct) is null)
            return NotFound();

        var f = new FeedingLog
        {
            BabyId = babyId,
            StartUtc = body.StartUtc,
            EndUtc = body.EndUtc,
            Type = body.Type,
            BreastSide = body.BreastSide,
            AmountMl = body.AmountMl,
            FoodDescription = body.FoodDescription,
            Notes = body.Notes,
        };
        _db.FeedingLogs.Add(f);
        await _db.SaveChangesAsync(ct);
        return Ok(FeedingDto.FromEntity(f));
    }

    /// <summary>Predložena vremena dojenja/flaše svakih ~3h za mlađu dojenčad.</summary>
    [HttpGet("milk-schedule-hints")]
    public async Task<ActionResult<MilkScheduleDto>> MilkHints(int babyId, [FromQuery] TimeOnly? dayStart, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var baby = await BabyAccess.ForParentAsync(_db, babyId, userId, ct);
        if (baby is null)
            return NotFound();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var ageMonths = BabyAge.AgeInMonths(baby.DateOfBirth, today);
        if (!BabyAge.IsYoungInfant(ageMonths))
            return Ok(new MilkScheduleDto(false, baby.InfantMilkRoutine?.ToString(), Array.Empty<TimeOnly>(), "Za stariju dob koristi evidenciju obroka i čvrstu hranu."));

        if (baby.InfantMilkRoutine is null)
            return Ok(new MilkScheduleDto(true, null, Array.Empty<TimeOnly>(), "Postavi u profilu: flaša ili dojenje za personalizirane savjete."));

        var start = dayStart ?? new TimeOnly(7, 0);
        var times = new List<TimeOnly>();
        for (var i = 0; i < 8; i++)
            times.Add(start.AddHours(i * 3));

        var mode = baby.InfantMilkRoutine == InfantMilkRoutine.Breast ? "dojenje" : "flaša";
        return Ok(new MilkScheduleDto(true, mode, times, $"Heuristika: oko svakih 3 sata ({mode}). Prilagodi prema znakovima gladi."));
    }

    [HttpGet("solid-guidance")]
    public async Task<ActionResult<SolidGuidanceDto>> SolidGuidance(int babyId, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var baby = await BabyAccess.ForParentAsync(_db, babyId, userId, ct);
        if (baby is null)
            return NotFound();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var whole = BabyAge.AgeInWholeMonths(baby.DateOfBirth, today);
        var goal = baby.SolidMealsPerDayGoal;

        var foods = await _db.FoodSuggestions.AsNoTracking()
            .Where(f => f.IntroFromMonth <= whole)
            .OrderBy(f => f.Name)
            .Take(30)
            .Select(f => new FoodSuggestionDto(f.Name, f.IntroFromMonth, f.Notes))
            .ToListAsync(ct);

        var phaseHint = goal switch
        {
            0 => whole >= 4
                ? "Možeš započeti s 1 jutarnjim obrokom (pire povrća/voća)."
                : "Do 4–4,5 mj fokus na mlijeko; čvrsta hrana kasnije.",
            1 => "Faza 1 obrok: uvedi jednu namirnicu nekoliko dana, zatim kombiniraj.",
            2 => "Faza 2 obroka: npr. jutro + poslijepodne.",
            3 => "Faza 3 obroka: doručak, ručak, večera.",
            _ => "Postupno povećavaj teksturu i raznolikost uz nadzor.",
        };

        return Ok(new SolidGuidanceDto(whole, goal, phaseHint, foods));
    }

    public record FeedingCreate(
        DateTimeOffset StartUtc,
        DateTimeOffset? EndUtc,
        FeedingLogType Type,
        BreastSide? BreastSide,
        int? AmountMl,
        string? FoodDescription,
        string? Notes);

    public record FeedingDto(
        int Id,
        DateTimeOffset StartUtc,
        DateTimeOffset? EndUtc,
        string Type,
        string? BreastSide,
        int? AmountMl,
        string? FoodDescription,
        string? Notes)
    {
        public static FeedingDto FromEntity(FeedingLog f) =>
            new(
                f.Id,
                f.StartUtc,
                f.EndUtc,
                f.Type.ToString(),
                f.BreastSide?.ToString(),
                f.AmountMl,
                f.FoodDescription,
                f.Notes);
    }

    public record MilkScheduleDto(bool Applies, string? Routine, IReadOnlyList<TimeOnly> SuggestedTimesLocal, string Message);

    public record FoodSuggestionDto(string Name, int IntroFromMonth, string? Notes);

    public record SolidGuidanceDto(int AgeWholeMonths, int SolidMealsPerDayGoal, string PhaseHint, IReadOnlyList<FoodSuggestionDto> Suggestions);
}
