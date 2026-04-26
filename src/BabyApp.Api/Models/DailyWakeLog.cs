namespace BabyApp.Api.Models;

/// <summary>Jutarnje buđenje za određeni dan (za plan dremki).</summary>
public class DailyWakeLog
{
    public int Id { get; set; }

    public int BabyId { get; set; }

    public Baby? Baby { get; set; }

    public DateOnly ForDate { get; set; }

    public TimeOnly MorningWakeTime { get; set; }
}
