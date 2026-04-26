using System.Security.Claims;
using BabyApp.Api.Data;
using BabyApp.Api.Models;
using BabyApp.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BabyApp.Api.Controllers;

[ApiController]
[Route("api/babies/{babyId:int}/growth")]
[Authorize]
public class BabyGrowthController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public BabyGrowthController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("measurements")]
    public async Task<ActionResult<IReadOnlyList<GrowthDto>>> List(int babyId, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        if (await BabyAccess.ForParentAsync(_db, babyId, userId, ct) is null)
            return NotFound();

        var rows = await _db.GrowthMeasurements.AsNoTracking()
            .Where(g => g.BabyId == babyId)
            .OrderBy(g => g.MeasuredDate)
            .ToListAsync(ct);

        return Ok(rows.Select(GrowthDto.FromEntity).ToList());
    }

    [HttpPost("measurements")]
    public async Task<ActionResult<GrowthDto>> Add(int babyId, [FromBody] GrowthCreate body, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var baby = await BabyAccess.ForParentAsync(_db, babyId, userId, ct);
        if (baby is null)
            return NotFound();

        var g = new GrowthMeasurement
        {
            BabyId = babyId,
            MeasuredDate = body.MeasuredDate,
            WeightKg = body.WeightKg,
            HeightCm = body.HeightCm,
            HeadCircumferenceCm = body.HeadCircumferenceCm,
            Notes = body.Notes,
        };
        _db.GrowthMeasurements.Add(g);
        await _db.SaveChangesAsync(ct);
        return Ok(GrowthDto.FromEntity(g));
    }

    [HttpGet("reference-at-date")]
    public async Task<ActionResult<ReferenceBundleDto>> Reference(int babyId, [FromQuery] DateOnly onDate, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var baby = await BabyAccess.ForParentAsync(_db, babyId, userId, ct);
        if (baby is null)
            return NotFound();

        var whole = BabyAge.AgeInWholeMonths(baby.DateOfBirth, onDate);
        var sex = baby.Sex == BabySex.Unknown ? BabySex.Female : baby.Sex;

        var w = GrowthReferenceService.WeightBand(whole, sex);
        var h = GrowthReferenceService.HeightBand(whole, sex);
        var hc = GrowthReferenceService.HeadBand(whole, sex);

        return Ok(new ReferenceBundleDto(
            whole,
            new RefDto(w.Min, w.Max, w.Note),
            new RefDto(h.Min, h.Max, h.Note),
            new RefDto(hc.Min, hc.Max, hc.Note)));
    }

    public record GrowthCreate(
        DateOnly MeasuredDate,
        decimal? WeightKg,
        decimal? HeightCm,
        decimal? HeadCircumferenceCm,
        string? Notes);

    public record GrowthDto(int Id, DateOnly MeasuredDate, decimal? WeightKg, decimal? HeightCm, decimal? HeadCircumferenceCm, string? Notes)
    {
        public static GrowthDto FromEntity(GrowthMeasurement g) =>
            new(g.Id, g.MeasuredDate, g.WeightKg, g.HeightCm, g.HeadCircumferenceCm, g.Notes);
    }

    public record RefDto(decimal? Min, decimal? Max, string Note);

    public record ReferenceBundleDto(int AgeWholeMonthsOnDate, RefDto WeightKg, RefDto HeightCm, RefDto HeadCm);
}
