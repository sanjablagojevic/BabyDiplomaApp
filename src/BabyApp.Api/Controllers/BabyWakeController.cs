using System.Security.Claims;
using BabyApp.Api.Data;
using BabyApp.Api.Models;
using BabyApp.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BabyApp.Api.Controllers;

[ApiController]
[Route("api/babies/{babyId:int}/wake")]
[Authorize]
public class BabyWakeController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public BabyWakeController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<ActionResult<WakeLogResponse>> Log(int babyId, [FromBody] WakeLogRequest body, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (await BabyAccess.ForParentAsync(_db, babyId, userId, ct) is null)
            return NotFound();

        var existing = await _db.DailyWakeLogs.FirstOrDefaultAsync(
            w => w.BabyId == babyId && w.ForDate == body.ForDate, ct);

        if (existing is null)
        {
            existing = new DailyWakeLog { BabyId = babyId, ForDate = body.ForDate, MorningWakeTime = body.MorningWakeTime };
            _db.DailyWakeLogs.Add(existing);
        }
        else
            existing.MorningWakeTime = body.MorningWakeTime;

        await _db.SaveChangesAsync(ct);
        return Ok(new WakeLogResponse(existing.Id, existing.ForDate, existing.MorningWakeTime));
    }

    [HttpGet("nap-plan")]
    public async Task<ActionResult<NapPlanResponse>> NapPlan(int babyId, [FromQuery] DateOnly? date, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var baby = await BabyAccess.ForParentAsync(_db, babyId, userId, ct);
        if (baby is null)
            return NotFound();

        var forDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var wake = await _db.DailyWakeLogs.AsNoTracking()
            .FirstOrDefaultAsync(w => w.BabyId == babyId && w.ForDate == forDate, ct);

        if (wake is null)
            return BadRequest(new { message = "Nema unosa jutarnjeg buđenja za taj dan. Najprije spremi vrijeme buđenja." });

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var plan = NapPlanService.ForBirthDate(baby.DateOfBirth, today);
        var starts = NapPlanService.SuggestedNapStartTimes(wake.MorningWakeTime, plan);
        var bedtime = NapPlanService.SuggestBedtimeStart(wake.MorningWakeTime, plan);

        var next = starts.Select((t, i) => new NapSlotDto(i + 1, t, plan.TypicalNapLengthsMinutes[i % plan.TypicalNapLengthsMinutes.Count])).ToList();

        var sessions = await _db.SleepSessions.AsNoTracking()
            .Where(s => s.BabyId == babyId && s.StartUtc >= DateTimeOffset.UtcNow.AddDays(-14))
            .OrderByDescending(s => s.StartUtc)
            .Take(40)
            .ToListAsync(ct);

        DateTimeOffset? lastSleepEndUtc = null;
        var isBabyAsleepNow = false;
        if (sessions.Count > 0)
        {
            var latest = sessions[0];
            if (latest.EndUtc is null)
            {
                isBabyAsleepNow = true;
                lastSleepEndUtc = sessions.Skip(1).FirstOrDefault(s => s.EndUtc.HasValue)?.EndUtc;
            }
            else
                lastSleepEndUtc = latest.EndUtc;
        }

        return Ok(new NapPlanResponse(
            forDate,
            wake.MorningWakeTime,
            plan.NapCount,
            plan.TypicalNapLengthsMinutes,
            plan.SuggestedWakeWindowMinutes,
            plan.WakeWindowMinMinutes,
            plan.WakeWindowMaxMinutes,
            plan.WakeWindowBandLabel,
            plan.BedtimeWindowHint,
            bedtime,
            plan.Notes,
            next,
            lastSleepEndUtc,
            isBabyAsleepNow));
    }

    public record WakeLogRequest(DateOnly ForDate, TimeOnly MorningWakeTime);

    public record WakeLogResponse(int Id, DateOnly ForDate, TimeOnly MorningWakeTime);

    public record NapSlotDto(int Order, TimeOnly SuggestedStartLocal, int TypicalDurationMinutes);

    public record NapPlanResponse(
        DateOnly Date,
        TimeOnly MorningWake,
        int NapCount,
        IReadOnlyList<int> TypicalNapLengthsMinutes,
        int SuggestedWakeWindowMinutes,
        int WakeWindowMinMinutes,
        int WakeWindowMaxMinutes,
        string WakeWindowBandLabel,
        string BedtimeWindowHint,
        TimeOnly SuggestedBedtimeStartLocal,
        string Notes,
        IReadOnlyList<NapSlotDto> Naps,
        DateTimeOffset? LastSleepEndUtc,
        bool IsBabyAsleepNow);
}
