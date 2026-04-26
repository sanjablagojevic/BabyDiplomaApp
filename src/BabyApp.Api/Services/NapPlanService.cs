namespace BabyApp.Api.Services;

/// <summary>
/// Heuristički plan dremki po uzrastu (informativno — nije medicinski algoritam).
/// Inspiracija: opće smjernice sna kod dojenčadi.
/// </summary>
public static class NapPlanService
{
    public sealed record NapPlan(
        int NapCount,
        IReadOnlyList<int> TypicalNapLengthsMinutes,
        int SuggestedWakeWindowMinutes,
        string BedtimeWindowHint,
        string Notes);

    public static NapPlan ForAgeMonths(double ageMonths)
    {
        if (ageMonths < 1)
            return new NapPlan(5, [30, 45, 45, 45, 30], 45, "19:30–21:00", "Novorođenče: česti kratki snovi.");
        if (ageMonths < 3)
            return new NapPlan(4, [45, 60, 60, 45], 75, "19:00–20:30", "3–4 dremke, duži noćni san se formira.");
        if (ageMonths < 6)
            return new NapPlan(3, [60, 90, 45], 120, "18:30–20:00", "3 dremke; prati znakove umora.");
        if (ageMonths < 9)
            return new NapPlan(3, [60, 90, 60], 150, "18:30–19:30", "Još 2–3 dremke ovisno o noćnom snu.");
        if (ageMonths < 12)
            return new NapPlan(2, [90, 90], 180, "18:30–19:30", "Često 2 dremke; jedna može ispasti.");
        return new NapPlan(1, [120], 240, "19:00–20:00", "Većina beba na 1 podnevnom snu.");
    }

    /// <summary>Predložena lokalna vremena dremki od jutarnjeg buđenja.</summary>
    public static IReadOnlyList<TimeOnly> SuggestedNapStartTimes(TimeOnly morningWake, NapPlan plan)
    {
        var list = new List<TimeOnly>();
        var t = morningWake;
        for (var i = 0; i < plan.NapCount; i++)
        {
            t = t.AddMinutes(plan.SuggestedWakeWindowMinutes);
            list.Add(t);
            var napLen = plan.TypicalNapLengthsMinutes[i % plan.TypicalNapLengthsMinutes.Count];
            t = t.AddMinutes(napLen);
        }
        return list;
    }

    public static TimeOnly SuggestBedtimeStart(TimeOnly morningWake, NapPlan plan)
    {
        var lastNapEnd = morningWake;
        var t = morningWake;
        for (var i = 0; i < plan.NapCount; i++)
        {
            t = t.AddMinutes(plan.SuggestedWakeWindowMinutes);
            var napLen = plan.TypicalNapLengthsMinutes[i % plan.TypicalNapLengthsMinutes.Count];
            t = t.AddMinutes(napLen);
            lastNapEnd = t;
        }
        return lastNapEnd.AddMinutes(Math.Min(240, plan.SuggestedWakeWindowMinutes + 60));
    }
}
