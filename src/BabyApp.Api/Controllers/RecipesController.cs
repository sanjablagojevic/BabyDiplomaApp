using BabyApp.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BabyApp.Api.Controllers;

[ApiController]
[Route("api/recipes")]
[Authorize]
public class RecipesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;

    public RecipesController(ApplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RecipeDto>>> List([FromQuery] int? maxAgeMonths, CancellationToken ct)
    {
        var q = _db.Recipes.AsNoTracking().AsQueryable();
        if (maxAgeMonths.HasValue)
            q = q.Where(r => r.MinAgeMonths <= maxAgeMonths.Value);
        var rows = await q.OrderBy(r => r.MinAgeMonths).ThenBy(r => r.Title).ToListAsync(ct);
        return Ok(rows.Select(r => new RecipeDto(r.Id, r.Title, r.MinAgeMonths, r.Summary, r.PdfFileName)).ToList());
    }

    /// <summary>PDF iz wwwroot/recipes ako postoji.</summary>
    [HttpGet("{id:int}/file")]
    public async Task<IActionResult> File(int id, CancellationToken ct)
    {
        var r = await _db.Recipes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (r?.PdfFileName is null || string.IsNullOrWhiteSpace(r.PdfFileName))
            return NotFound();

        var path = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "recipes", r.PdfFileName);
        if (!System.IO.File.Exists(path))
            return NotFound();

        var stream = System.IO.File.OpenRead(path);
        return File(stream, "application/pdf", r.PdfFileName);
    }

    public record RecipeDto(int Id, string Title, int MinAgeMonths, string? Summary, string? PdfFileName);
}
