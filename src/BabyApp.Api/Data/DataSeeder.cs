using BabyApp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BabyApp.Api.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db, CancellationToken ct = default)
    {
        if (!await db.SleepAudioResources.AnyAsync(ct))
        {
            db.SleepAudioResources.AddRange(
                new SleepAudioResource { Title = "Bijela buka (rain)", Kind = AudioResourceKind.WhiteNoise, Url = "https://example.com/legal-white-noise" },
                new SleepAudioResource { Title = "Uspavanka — nježni tonovi", Kind = AudioResourceKind.Lullaby, Url = "https://example.com/legal-lullaby" });
        }

        if (!await db.FoodSuggestions.AnyAsync(ct))
        {
            db.FoodSuggestions.AddRange(
                new FoodSuggestion { Name = "Pire od tikvice", IntroFromMonth = 4, Notes = "Dobra prva povrtnica." },
                new FoodSuggestion { Name = "Pire od kruške", IntroFromMonth = 4, Notes = "Blago slatko voće." },
                new FoodSuggestion { Name = "Pire od mrkve", IntroFromMonth = 5, Notes = "Lagano slatko." },
                new FoodSuggestion { Name = "Pire od kumpira", IntroFromMonth = 5, Notes = "Kombiniraj s povrćem." },
                new FoodSuggestion { Name = "Rižina kašica", IntroFromMonth = 6, Notes = "Često baza za kombinacije." },
                new FoodSuggestion { Name = "Zobena kašica", IntroFromMonth = 6, Notes = "Oblikovana tekstura." });
        }

        if (!await db.Recipes.AnyAsync(ct))
        {
            db.Recipes.AddRange(
                new Recipe { Title = "Zelena kašica (tikvica + krumpir)", MinAgeMonths = 5, Summary = "Kuhano, pasirano, zamrznuto u kockicama.", PdfFileName = null },
                new Recipe { Title = "Voćni mix (banana + kruška)", MinAgeMonths = 6, Summary = "Svježe ili kuhano, sitno pasirano.", PdfFileName = null });
        }

        if (!await db.EducationalArticles.AnyAsync(ct))
        {
            db.EducationalArticles.AddRange(
                new EducationalArticle { Band = AgeBand.ZeroToThreeMonths, SortOrder = 1, Title = "Kontakt koža na kožu", Body = "Česti kontakt pomaže regulaciji sna i hranjenja u prvim tjednima." },
                new EducationalArticle { Band = AgeBand.ZeroToThreeMonths, SortOrder = 2, Title = "Signalno hranjenje", Body = "Prati znakove gladi umjesto strogog rasporeda na početku." },
                new EducationalArticle { Band = AgeBand.ThreeToSixMonths, SortOrder = 1, Title = "Uvodenje čvrste hrane", Body = "Jedan obrok dnevno, kasnije postupno povećanje." },
                new EducationalArticle { Band = AgeBand.ThreeToSixMonths, SortOrder = 2, Title = "Razvoj motorike", Body = "Sigurno podsticanje tummy time-a u budnom stanju." },
                new EducationalArticle { Band = AgeBand.SixToTwelveMonths, SortOrder = 1, Title = "Raznolikost tekstura", Body = "Postupno grubiji obroci uz nadzor." },
                new EducationalArticle { Band = AgeBand.SixToTwelveMonths, SortOrder = 2, Title = "Samohranjenje", Body = "Omogući istraživanje hrane u sigurnom okruženju." });
        }

        await db.SaveChangesAsync(ct);
    }
}
