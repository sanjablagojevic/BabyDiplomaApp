using BabyApp.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BabyApp.Api.Controllers;

[ApiController]
[Route("api/sleep-audio")]
[Authorize]
public class SleepAudioResourcesController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public SleepAudioResourcesController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AudioDto>>> List(CancellationToken ct)
    {
        var rows = await _db.SleepAudioResources.AsNoTracking().OrderBy(x => x.Kind).ThenBy(x => x.Title).ToListAsync(ct);
        return Ok(rows.Select(a => new AudioDto(a.Id, a.Title, a.Kind.ToString(), a.Url)).ToList());
    }

    public record AudioDto(int Id, string Title, string Kind, string? Url);
}
