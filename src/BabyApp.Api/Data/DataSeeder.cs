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

        var seededRecipes = new[]
        {
            new Recipe { Title = "Tikvica (početni pire)", MinAgeMonths = 6, Summary = "Prvi dani uvođenja: jednostavna baza od jedne namirnice.", PdfFileName = null },
            new Recipe { Title = "Batat + tikvica", MinAgeMonths = 6, Summary = "Blaga kombinacija za rane dane uvođenja.", PdfFileName = null },
            new Recipe { Title = "Jabuka + batat + tikvica", MinAgeMonths = 6, Summary = "Voće i povrće u jednom laganom obroku.", PdfFileName = null },
            new Recipe { Title = "Palenta + jabuka + tikvica", MinAgeMonths = 6, Summary = "Kombinacija žitarice i voća/povrća.", PdfFileName = null },
            new Recipe { Title = "Suva šljiva + palenta + jabuka", MinAgeMonths = 6, Summary = "Česta kombinacija za probavu i vlakna.", PdfFileName = null },
            new Recipe { Title = "Blitva + tikvica + batat", MinAgeMonths = 6, Summary = "Povrtni obrok uz dodatak edmu po potrebi.", PdfFileName = null },
            new Recipe { Title = "Proso + jabuka + kruška", MinAgeMonths = 6, Summary = "Žitarica i voće za doručak.", PdfFileName = null },
            new Recipe { Title = "Krompir + tikvica + edmu", MinAgeMonths = 6, Summary = "Topli slani obrok za kasnije dane prve faze.", PdfFileName = null },
            new Recipe { Title = "Piletina + krompir + tikvica + edmu", MinAgeMonths = 6, Summary = "Prva proteinska kombinacija iz plana.", PdfFileName = null },
            new Recipe { Title = "Speltin griz + jabuka + banana", MinAgeMonths = 6, Summary = "Kašica sa žitaricom i voćem.", PdfFileName = null },
            new Recipe { Title = "Zobene pahuljice + banana + jabuka", MinAgeMonths = 6, Summary = "Jednostavna zobena kašica.", PdfFileName = null },
            new Recipe { Title = "Grašak + krompir + tikvica + edmu", MinAgeMonths = 6, Summary = "Povrtni ručak sredinom prve faze.", PdfFileName = null },
            new Recipe { Title = "Cejlonski cimet + banana + jabuka + zobene", MinAgeMonths = 6, Summary = "Voćno-zobena varijanta sa cimetom.", PdfFileName = null },
            new Recipe { Title = "Proso + banana", MinAgeMonths = 8, Summary = "Početak 8+ faze sa dva obroka dnevno.", PdfFileName = null },
            new Recipe { Title = "Brokula + tikvica + krompir + edmu", MinAgeMonths = 8, Summary = "Povrtni ručak iz 8+ plana.", PdfFileName = null },
            new Recipe { Title = "Teletina + crveni luk + krompir + grašak + edmu", MinAgeMonths = 8, Summary = "Ručak sa teletinom i povrćem.", PdfFileName = null },
            new Recipe { Title = "Piletina + tikvica + brokula + krompir + edmu", MinAgeMonths = 8, Summary = "Kompletan proteinsko-povrtni obrok.", PdfFileName = null },
            new Recipe { Title = "Mango + banana + zobene", MinAgeMonths = 8, Summary = "Voćno-zobena kombinacija.", PdfFileName = null },
            new Recipe { Title = "Karfiol + batat + piletina + mrkva + edmu", MinAgeMonths = 8, Summary = "Topli ručak za 8-9 mjeseci.", PdfFileName = null },
            new Recipe { Title = "Maline + proso + jogurt", MinAgeMonths = 8, Summary = "Voćna kašica sa fermentisanim mliječnim proizvodom.", PdfFileName = null },
            new Recipe { Title = "Kupine + zobene pahuljice + mango", MinAgeMonths = 9, Summary = "Voćna kombinacija za doručak.", PdfFileName = null },
            new Recipe { Title = "Jaje + crveni luk + blitva", MinAgeMonths = 9, Summary = "Slani obrok sa jajetom i zelenim povrćem.", PdfFileName = null },
            new Recipe { Title = "Borovnice + palenta + jabuka + c.cimet", MinAgeMonths = 9, Summary = "Palenta obogaćena voćem.", PdfFileName = null },
            new Recipe { Title = "Oslić + brokula + batat + tikva + edmu", MinAgeMonths = 10, Summary = "Riblji ručak iz 10+ faze.", PdfFileName = null },
            new Recipe { Title = "Potaž (batat + karfiol + jogurt + edmu)", MinAgeMonths = 10, Summary = "Večernji potaž iz plana 10+.", PdfFileName = null },
            new Recipe { Title = "Kajgana na suvoj tavi + jogurt", MinAgeMonths = 10, Summary = "Brz proteinski obrok.", PdfFileName = null },
            new Recipe { Title = "Supa s knedlama", MinAgeMonths = 10, Summary = "Označeno u planu kao recept sa *.", PdfFileName = null },
            new Recipe { Title = "Riža + oslić + blitva + b.luk + edmu", MinAgeMonths = 10, Summary = "Riblji ručak sa rižom.", PdfFileName = null },
            new Recipe { Title = "Baby tortilje", MinAgeMonths = 10, Summary = "Označeno u planu kao recept sa *.", PdfFileName = null },
            new Recipe { Title = "Pasulj + junetina + mrkva + b.luk + krompir + edmu", MinAgeMonths = 11, Summary = "Mahunarke i meso u jednom obroku.", PdfFileName = null },
            new Recipe { Title = "Potaž (blitva + tikva + tikvica + sjeme tikve + edmu)", MinAgeMonths = 11, Summary = "Zeleni potaž za večeru.", PdfFileName = null },
            new Recipe { Title = "Riblji štapići", MinAgeMonths = 11, Summary = "Označeno u planu kao recept sa *.", PdfFileName = null },
            new Recipe { Title = "Mafini sa jabukom i kruškom", MinAgeMonths = 11, Summary = "Mekani snack obrok iz kasnije faze.", PdfFileName = null },
            new Recipe { Title = "Piletina + paradajz + paprika + komorač + batat + edmu", MinAgeMonths = 11, Summary = "Složeniji povrtno-mesni ručak.", PdfFileName = null },
        };

        var existingRecipeTitles = await db.Recipes.AsNoTracking().Select(r => r.Title).ToListAsync(ct);
        var existingSet = new HashSet<string>(existingRecipeTitles, StringComparer.OrdinalIgnoreCase);
        var missingRecipes = seededRecipes.Where(r => !existingSet.Contains(r.Title)).ToList();
        if (missingRecipes.Count > 0)
            db.Recipes.AddRange(missingRecipes);

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
