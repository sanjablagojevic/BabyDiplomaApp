namespace BabyApp.Api.Services;

/// <summary>Pojednostavljeni referentni rasponi (medijan ± široki prag) za validaciju unosa — informativno.</summary>
public static class GrowthReferenceService
{
    public sealed record Range(decimal? Min, decimal? Max, string Note);

    public static Range WeightBand(int wholeMonths, Models.BabySex sex)
    {
        if (wholeMonths < 0 || wholeMonths > 24)
            return new Range(null, null, "Izvan tablice (0–24 mj).");
        var (median, spread) = sex == Models.BabySex.Female ? FemaleWeight[wholeMonths] : MaleWeight[wholeMonths];
        return new Range(Math.Round(median - spread, 2), Math.Round(median + spread, 2), "Približan raspon oko medijana (WHO-inspiracija, pojednostavljeno).");
    }

    public static Range HeightBand(int wholeMonths, Models.BabySex sex)
    {
        if (wholeMonths < 0 || wholeMonths > 24)
            return new Range(null, null, "Izvan tablice (0–24 mj).");
        var (median, spread) = sex == Models.BabySex.Female ? FemaleHeight[wholeMonths] : MaleHeight[wholeMonths];
        return new Range(Math.Round(median - spread, 1), Math.Round(median + spread, 1), "Približan raspon visine.");
    }

    public static Range HeadBand(int wholeMonths, Models.BabySex sex)
    {
        if (wholeMonths < 0 || wholeMonths > 24)
            return new Range(null, null, "Izvan tablice (0–24 mj).");
        var (median, spread) = sex == Models.BabySex.Female ? FemaleHead[wholeMonths] : MaleHead[wholeMonths];
        return new Range(Math.Round(median - spread, 1), Math.Round(median + spread, 1), "Približan raspon obima glave.");
    }

    // Medijan kg, ± kg — grubo za demonstraciju diplomske aplikacije
    private static readonly (decimal m, decimal s)[] MaleWeight = Build(3.3m,
        new[] { 0.45m, 0.55m, 0.65m, 0.7m, 0.75m, 0.8m, 0.82m, 0.84m, 0.86m, 0.88m, 0.9m, 0.22m });

    private static readonly (decimal m, decimal s)[] FemaleWeight = Build(3.2m,
        new[] { 0.42m, 0.5m, 0.58m, 0.62m, 0.66m, 0.7m, 0.72m, 0.74m, 0.76m, 0.78m, 0.8m, 0.2m });

    private static readonly (decimal m, decimal s)[] MaleHeight = BuildLength(49.9m, 2.8m);
    private static readonly (decimal m, decimal s)[] FemaleHeight = BuildLength(49.1m, 2.7m);
    private static readonly (decimal m, decimal s)[] MaleHead = BuildLength(34.5m, 1.4m);
    private static readonly (decimal m, decimal s)[] FemaleHead = BuildLength(33.9m, 1.35m);

    private static (decimal m, decimal s)[] Build(decimal birth, decimal[] monthlyGain)
    {
        var arr = new (decimal m, decimal s)[25];
        var w = birth;
        for (var i = 0; i <= 24; i++)
        {
            var spread = 0.9m + i * 0.08m;
            arr[i] = (Math.Round(w, 2), spread);
            if (i < monthlyGain.Length)
                w += monthlyGain[i];
            else
                w += monthlyGain[^1];
        }
        return arr;
    }

    private static (decimal m, decimal s)[] BuildLength(decimal birth, decimal monthly)
    {
        var arr = new (decimal m, decimal s)[25];
        var h = birth;
        for (var i = 0; i <= 24; i++)
        {
            var spread = 2.2m + i * 0.15m;
            arr[i] = (Math.Round(h, 1), spread);
            h += monthly;
        }
        return arr;
    }
}
