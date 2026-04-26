using BabyApp.Api.Models;

namespace BabyApp.Api.Services;

public static class BabyAge
{
    public static double AgeInMonths(DateOnly birth, DateOnly today)
    {
        var days = today.DayNumber - birth.DayNumber;
        return days / 30.437; // prosječan mjesec
    }

    public static AgeBand BandFor(double ageMonths) =>
        ageMonths < 3 ? AgeBand.ZeroToThreeMonths :
        ageMonths < 6 ? AgeBand.ThreeToSixMonths :
        AgeBand.SixToTwelveMonths;

    public static bool IsYoungInfant(double ageMonths) => ageMonths < 4.5;

    public static int AgeInWholeMonths(DateOnly birth, DateOnly today)
    {
        var m = (today.Year - birth.Year) * 12 + (today.Month - birth.Month);
        if (today.Day < birth.Day) m--;
        return Math.Max(0, m);
    }
}
