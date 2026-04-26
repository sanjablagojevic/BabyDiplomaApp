using System.Security.Claims;
using BabyApp.Api.Data;
using BabyApp.Api.Models;
using BabyApp.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BabyApp.Api.Controllers;

[ApiController]
[Route("api/babies/{babyId:int}/reminders")]
[Authorize]
public class BabyRemindersController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public BabyRemindersController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ReminderDto>>> List(int babyId, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        if (await BabyAccess.ForParentAsync(_db, babyId, userId, ct) is null)
            return NotFound();

        var rows = await _db.Reminders.AsNoTracking()
            .Where(r => r.BabyId == babyId)
            .OrderBy(r => r.LocalTime)
            .ToListAsync(ct);

        return Ok(rows.Select(ReminderDto.FromEntity).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<ReminderDto>> Create(int babyId, [FromBody] ReminderCreate body, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        if (await BabyAccess.ForParentAsync(_db, babyId, userId, ct) is null)
            return NotFound();

        var r = new Reminder
        {
            BabyId = babyId,
            Kind = body.Kind,
            Title = body.Title.Trim(),
            LocalTime = body.LocalTime,
            IsEnabled = body.IsEnabled,
            VaccineName = body.VaccineName,
            VaccineDueDate = body.VaccineDueDate,
        };
        _db.Reminders.Add(r);
        await _db.SaveChangesAsync(ct);
        return Ok(ReminderDto.FromEntity(r));
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult<ReminderDto>> Patch(int babyId, int id, [FromBody] ReminderPatch body, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        if (await BabyAccess.ForParentAsync(_db, babyId, userId, ct) is null)
            return NotFound();

        var r = await _db.Reminders.FirstOrDefaultAsync(x => x.Id == id && x.BabyId == babyId, ct);
        if (r is null)
            return NotFound();

        if (!string.IsNullOrWhiteSpace(body.Title))
            r.Title = body.Title.Trim();
        if (body.LocalTime.HasValue)
            r.LocalTime = body.LocalTime.Value;
        if (body.IsEnabled.HasValue)
            r.IsEnabled = body.IsEnabled.Value;
        if (body.VaccineName is not null)
            r.VaccineName = body.VaccineName;
        if (body.VaccineDueDate.HasValue)
            r.VaccineDueDate = body.VaccineDueDate;

        await _db.SaveChangesAsync(ct);
        return Ok(ReminderDto.FromEntity(r));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int babyId, int id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        if (await BabyAccess.ForParentAsync(_db, babyId, userId, ct) is null)
            return NotFound();

        var r = await _db.Reminders.FirstOrDefaultAsync(x => x.Id == id && x.BabyId == babyId, ct);
        if (r is null)
            return NotFound();
        _db.Reminders.Remove(r);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    public record ReminderCreate(
        ReminderKind Kind,
        string Title,
        TimeOnly LocalTime,
        bool IsEnabled,
        string? VaccineName,
        DateOnly? VaccineDueDate);

    public record ReminderPatch(string? Title, TimeOnly? LocalTime, bool? IsEnabled, string? VaccineName, DateOnly? VaccineDueDate);

    public record ReminderDto(
        int Id,
        string Kind,
        string Title,
        TimeOnly LocalTime,
        bool IsEnabled,
        string? VaccineName,
        DateOnly? VaccineDueDate)
    {
        public static ReminderDto FromEntity(Reminder r) =>
            new(r.Id, r.Kind.ToString(), r.Title, r.LocalTime, r.IsEnabled, r.VaccineName, r.VaccineDueDate);
    }
}
