namespace BabyApp.Api.Services;

/// <summary>
/// Heuristički plan dremki i budni prozori po uzrastu (informativno — nije medicinski savjet).
/// Budni prozori: prosječne smjernice po uzrastu (slično popularnim tablicama za roditelje).
/// </summary>
public static class NapPlanService
{
    public sealed record NapPlan(
        int NapCount,
        IReadOnlyList<int> TypicalNapLengthsMinutes,
        int SuggestedWakeWindowMinutes,
        int WakeWindowMinMinutes,
        int WakeWindowMaxMinutes,
        string WakeWindowBandLabel,
        string BedtimeWindowHint,
        string Notes);

    /// <summary>
    /// Budni prozor u minutama po uzrastu (min–max iz tablice).
    /// </summary>
    public static (int Min, int Max, string BandLabel) WakeWindowRange(DateOnly birth, DateOnly today)
    {
        var days = Math.Max(0, today.DayNumber - birth.DayNumber);
        var ageWeeks = days / 7.0;
        var wholeMonths = BabyAge.AgeInWholeMonths(birth, today);

        if (ageWeeks <= 6)
            return (45, 60, "1–6 tjedana");
        if (ageWeeks <= 12)
            return (60, 105, "7–12 tjedana");
        if (wholeMonths < 5)
            return (90, 120, "3–4 mjeseca");
        if (wholeMonths < 8)
            return (120, 210, "5–7 mjeseci");
        if (wholeMonths < 13)
            return (180, 240, "8–12 mjeseci");
        if (wholeMonths < 19)
            return (210, 300, "13–18 mjeseci");
        return (300, 360, "18+ mjeseci");
    }

    public static NapPlan ForBirthDate(DateOnly birth, DateOnly today)
    {
        var (wMin, wMax, label) = WakeWindowRange(birth, today);
        var typical = (wMin + wMax) / 2;
        var days = Math.Max(0, today.DayNumber - birth.DayNumber);
        var ageWeeks = days / 7.0;
        var wholeMonths = BabyAge.AgeInWholeMonths(birth, today);

        if (ageWeeks <= 6)
            return new NapPlan(5, [30, 45, 45, 45, 30], typical, wMin, wMax, label, "19:30–21:00", "Novorođenče: česti kratki snovi i kratki budni prozori.");
        if (ageWeeks <= 12)
            return new NapPlan(4, [45, 60, 60, 45], typical, wMin, wMax, label, "19:00–20:30", "Više kratkih dremki; noćni san se polako produžuje.");
        if (wholeMonths < 5)
            return new NapPlan(3, [60, 90, 45], typical, wMin, wMax, label, "18:30–20:00", "3 dremke u prosjeku; prati znakove umora.");
        if (wholeMonths < 8)
            return new NapPlan(3, [60, 90, 60], typical, wMin, wMax, label, "18:30–19:30", "Često 2–3 dremke; prilagodi ako noćni san traje dobro.");
        if (wholeMonths < 13)
            return new NapPlan(2, [90, 90], typical, wMin, wMax, label, "18:30–19:30", "Obično 2 dremke; jedna ponekad ispane.");
        return new NapPlan(1, [120], typical, wMin, wMax, label, "19:00–20:00", "Većina beba na jednom podnevnom snu ili bez dremke.");
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
