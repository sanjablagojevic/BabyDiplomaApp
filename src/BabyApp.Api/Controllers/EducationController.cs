using BabyApp.Api.Data;
using BabyApp.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BabyApp.Api.Controllers;

[ApiController]
[Route("api/education")]
[Authorize]
public class EducationController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public EducationController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("articles")]
    public async Task<ActionResult<IReadOnlyList<ArticleDto>>> Articles([FromQuery] AgeBand? band, CancellationToken ct)
    {
        var q = _db.EducationalArticles.AsNoTracking().AsQueryable();
        if (band.HasValue)
            q = q.Where(a => a.Band == band.Value);

        var rows = await q.OrderBy(a => a.Band).ThenBy(a => a.SortOrder).ToListAsync(ct);
        return Ok(rows.Select(a => new ArticleDto(a.Id, a.Band.ToString(), a.Title, a.Body)).ToList());
    }

    public record ArticleDto(int Id, string AgeBand, string Title, string Body);
}
