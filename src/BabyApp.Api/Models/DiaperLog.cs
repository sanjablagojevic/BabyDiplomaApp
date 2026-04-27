namespace BabyApp.Api.Models;

public class DiaperLog
{
    public int Id { get; set; }

    public int BabyId { get; set; }

    public Baby? Baby { get; set; }

    public DateTimeOffset OccurredUtc { get; set; }

    public DiaperType Type { get; set; }

    public string? Notes { get; set; }
}
