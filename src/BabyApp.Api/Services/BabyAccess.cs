using BabyApp.Api.Data;
using BabyApp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BabyApp.Api.Services;

public static class BabyAccess
{
    public static async Task<Baby?> ForParentAsync(ApplicationDbContext db, int babyId, string parentUserId, CancellationToken ct) =>
        await db.Babies.AsNoTracking().FirstOrDefaultAsync(b => b.Id == babyId && b.ParentId == parentUserId, ct);

    public static async Task<Baby?> ForParentTrackedAsync(ApplicationDbContext db, int babyId, string parentUserId, CancellationToken ct) =>
        await db.Babies.FirstOrDefaultAsync(b => b.Id == babyId && b.ParentId == parentUserId, ct);
}
