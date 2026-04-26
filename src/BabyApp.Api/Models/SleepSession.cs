namespace BabyApp.Api.Models;

public class SleepSession
{
    public int Id { get; set; }

    public int BabyId { get; set; }

    public Baby? Baby { get; set; }

    public DateTimeOffset StartUtc { get; set; }

    public DateTimeOffset? EndUtc { get; set; }

    public bool IsNap { get; set; }

    public string? Notes { get; set; }
}
