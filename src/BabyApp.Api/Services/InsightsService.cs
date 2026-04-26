using BabyApp.Api.Data;
using BabyApp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BabyApp.Api.Services;

public class InsightsService(ApplicationDbContext db)
{
    public async Task<IReadOnlyList<string>> GetInsightsAsync(int babyId, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var from = today.AddDays(-7);
        var fromDt = new DateTimeOffset(from.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

        var sessions = await db.SleepSessions
            .AsNoTracking()
            .Where(s => s.BabyId == babyId && s.StartUtc >= fromDt)
            .ToListAsync(ct);

        var feedings = await db.FeedingLogs
            .AsNoTracking()
            .Where(f => f.BabyId == babyId && f.StartUtc >= fromDt)
            .ToListAsync(ct);

        var list = new List<string>();

        var napMinutes = sessions
            .Where(s => s.IsNap && s.EndUtc.HasValue)
            .Sum(s => (s.EndUtc!.Value - s.StartUtc).TotalMinutes);
        var nightMinutes = sessions
            .Where(s => !s.IsNap && s.EndUtc.HasValue)
            .Sum(s => (s.EndUtc!.Value - s.StartUtc).TotalMinutes);

        if (sessions.Count == 0)
            list.Add("Nema unosa sna u zadnjih 7 dana — razmisli o bilježenju dremki radi pregleda.");
        else if (napMinutes + nightMinutes < 8 * 60 * 5)
            list.Add("Ukupno spavanje u zadnjih 7 dana djeluje kao da je na nižoj strani — prati bebu i po potrebi se posavjetuj s pedijatrom.");

        var milkCount = feedings.Count(f => f.Type is FeedingLogType.Breast or FeedingLogType.Bottle);
        if (milkCount < 14 && feedings.Count > 0)
            list.Add("Broj unosa dojenja/flaše u zadnjih 7 dana je manji od tipičnih ~2–3 u danu za stariju dojenčad — provjeri rutinu.");

        if (await db.ReactionLogs.AsNoTracking().CountAsync(r => r.BabyId == babyId && r.Kind == ReactionKind.Allergy && r.OccurredUtc >= fromDt, ct) >= 2)
            list.Add("Više zabilježenih reakcija/alergija u kratkom razdoblju — drži detaljan dnevnik i razgovaraj s liječnikom.");

        if (list.Count == 0)
            list.Add("U ovom trenutku nema automatskih upozorenja na temelju zadnjih unosa.");

        return list;
    }
}
