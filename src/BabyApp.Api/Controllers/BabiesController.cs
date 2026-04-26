using System.Security.Claims;
using BabyApp.Api.Data;
using BabyApp.Api.Models;
using BabyApp.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BabyApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BabiesController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public BabiesController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BabyResponse>>> GetMine(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var list = await _db.Babies
            .AsNoTracking()
            .Where(b => b.ParentId == userId)
            .OrderBy(b => b.Name)
            .Select(b => new BabyResponse(b.Id, b.Name, b.DateOfBirth, b.Sex, b.InfantMilkRoutine, b.SolidMealsPerDayGoal))
            .ToListAsync(ct);

        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BabyResponse>> GetOne(int id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var baby = await _db.Babies.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id && b.ParentId == userId, ct);
        return baby is null ? NotFound() : Ok(BabyResponse.FromEntity(baby));
    }

    [HttpGet("{id:int}/dashboard")]
    public async Task<ActionResult<DashboardDto>> Dashboard(int id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var baby = await _db.Babies.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id && b.ParentId == userId, ct);
        if (baby is null)
            return NotFound();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var ageMonths = BabyAge.AgeInMonths(baby.DateOfBirth, today);
        var wholeMonths = BabyAge.AgeInWholeMonths(baby.DateOfBirth, today);
        var band = BabyAge.BandFor(ageMonths);
        var young = BabyAge.IsYoungInfant(ageMonths);

        var modules = new List<string>
        {
            "sleep_naps", "sleep_audio", "feeding", "reactions", "growth", "milestones", "reminders", "education", "insights",
        };
        if (young)
            modules.Add("feeding_interval_3h");
        if (!young || baby.SolidMealsPerDayGoal > 0)
            modules.Add("solids");

        return Ok(new DashboardDto(
            BabyResponse.FromEntity(baby),
            Math.Round(ageMonths, 2),
            wholeMonths,
            band.ToString(),
            young,
            modules));
    }

    [HttpPost]
    public async Task<ActionResult<BabyResponse>> Create([FromBody] CreateBabyRequest body, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var baby = new Baby
        {
            Name = body.Name.Trim(),
            DateOfBirth = body.DateOfBirth,
            ParentId = userId,
            Sex = body.Sex ?? BabySex.Unknown,
            InfantMilkRoutine = body.InfantMilkRoutine,
            SolidMealsPerDayGoal = body.SolidMealsPerDayGoal ?? 0,
        };

        _db.Babies.Add(baby);
        await _db.SaveChangesAsync(ct);

        return Created($"{Request.Path}/{baby.Id}", BabyResponse.FromEntity(baby));
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult<BabyResponse>> Patch(int id, [FromBody] PatchBabyRequest body, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var baby = await _db.Babies.FirstOrDefaultAsync(b => b.Id == id && b.ParentId == userId, ct);
        if (baby is null)
            return NotFound();

        if (!string.IsNullOrWhiteSpace(body.Name))
            baby.Name = body.Name.Trim();
        if (body.Sex.HasValue)
            baby.Sex = body.Sex.Value;
        if (body.InfantMilkRoutine.HasValue)
            baby.InfantMilkRoutine = body.InfantMilkRoutine;
        if (body.SolidMealsPerDayGoal.HasValue)
            baby.SolidMealsPerDayGoal = Math.Clamp(body.SolidMealsPerDayGoal.Value, 0, 6);

        await _db.SaveChangesAsync(ct);
        return Ok(BabyResponse.FromEntity(baby));
    }

    public record CreateBabyRequest(
        string Name,
        DateOnly DateOfBirth,
        BabySex? Sex,
        InfantMilkRoutine? InfantMilkRoutine,
        int? SolidMealsPerDayGoal);

    public record PatchBabyRequest(
        string? Name,
        BabySex? Sex,
        InfantMilkRoutine? InfantMilkRoutine,
        int? SolidMealsPerDayGoal);

    public record BabyResponse(
        int Id,
        string Name,
        DateOnly DateOfBirth,
        BabySex Sex,
        InfantMilkRoutine? InfantMilkRoutine,
        int SolidMealsPerDayGoal)
    {
        public static BabyResponse FromEntity(Baby b) =>
            new(b.Id, b.Name, b.DateOfBirth, b.Sex, b.InfantMilkRoutine, b.SolidMealsPerDayGoal);
    }

    public record DashboardDto(
        BabyResponse Baby,
        double AgeMonthsApprox,
        int AgeWholeMonths,
        string AgeBand,
        bool IsYoungInfantUnderFourAndHalfMonths,
        IReadOnlyList<string> AvailableModules);
}
