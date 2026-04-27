using System.Security.Claims;
using BabyApp.Api.Data;
using BabyApp.Api.Models;
using BabyApp.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BabyApp.Api.Controllers;

[ApiController]
[Route("api/babies/{babyId:int}/gallery")]
[Authorize]
public class BabyGalleryController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;

    public BabyGalleryController(ApplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [HttpGet("slots")]
    public async Task<ActionResult<IReadOnlyList<GallerySlotDto>>> Slots(int babyId, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        if (await BabyAccess.ForParentAsync(_db, babyId, userId, ct) is null)
            return NotFound();

        var rows = await _db.BabyGalleryPhotos
            .AsNoTracking()
            .Where(x => x.BabyId == babyId)
            .OrderBy(x => x.MonthSlot)
            .ToListAsync(ct);

        return Ok(rows.Select(x => new GallerySlotDto(
            x.Id,
            x.MonthSlot,
            $"{Request.Scheme}://{Request.Host}/{x.RelativePath.Replace("\\", "/")}",
            x.UploadedUtc)).ToList());
    }

    [HttpPost("slots/{monthSlot:int}")]
    [RequestSizeLimit(10_000_000)]
    public async Task<ActionResult<GallerySlotDto>> Upload(int babyId, int monthSlot, IFormFile file, CancellationToken ct)
    {
        if (monthSlot < 0 || monthSlot > 11)
            return BadRequest(new { message = "monthSlot mora biti 0-11." });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        if (await BabyAccess.ForParentAsync(_db, babyId, userId, ct) is null)
            return NotFound();

        if (file is null || file.Length == 0)
            return BadRequest(new { message = "Datoteka je obavezna." });

        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Dozvoljene su samo slike." });

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(ext))
            ext = ".jpg";

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var relative = Path.Combine("gallery", fileName);
        var root = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var targetDir = Path.Combine(root, "gallery");
        Directory.CreateDirectory(targetDir);
        var fullPath = Path.Combine(targetDir, fileName);

        await using (var fs = System.IO.File.Create(fullPath))
            await file.CopyToAsync(fs, ct);

        var existing = await _db.BabyGalleryPhotos.FirstOrDefaultAsync(x => x.BabyId == babyId && x.MonthSlot == monthSlot, ct);
        if (existing is not null)
        {
            TryDeleteExisting(root, existing.RelativePath);
            existing.RelativePath = relative;
            existing.UploadedUtc = DateTimeOffset.UtcNow;
        }
        else
        {
            existing = new BabyGalleryPhoto
            {
                BabyId = babyId,
                MonthSlot = monthSlot,
                RelativePath = relative,
                UploadedUtc = DateTimeOffset.UtcNow,
            };
            _db.BabyGalleryPhotos.Add(existing);
        }

        await _db.SaveChangesAsync(ct);

        return Ok(new GallerySlotDto(
            existing.Id,
            existing.MonthSlot,
            $"{Request.Scheme}://{Request.Host}/{existing.RelativePath.Replace("\\", "/")}",
            existing.UploadedUtc));
    }

    [HttpDelete("slots/{monthSlot:int}")]
    public async Task<IActionResult> Delete(int babyId, int monthSlot, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        if (await BabyAccess.ForParentAsync(_db, babyId, userId, ct) is null)
            return NotFound();

        var row = await _db.BabyGalleryPhotos.FirstOrDefaultAsync(x => x.BabyId == babyId && x.MonthSlot == monthSlot, ct);
        if (row is null)
            return NotFound();

        var root = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        TryDeleteExisting(root, row.RelativePath);

        _db.BabyGalleryPhotos.Remove(row);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    private static void TryDeleteExisting(string webRoot, string relativePath)
    {
        try
        {
            var full = Path.Combine(webRoot, relativePath);
            if (System.IO.File.Exists(full))
                System.IO.File.Delete(full);
        }
        catch
        {
            // best effort
        }
    }

    public record GallerySlotDto(int Id, int MonthSlot, string ImageUrl, DateTimeOffset UploadedUtc);
}
