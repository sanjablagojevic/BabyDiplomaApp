namespace BabyApp.Api.Models;

public class FeedingLog
{
    public int Id { get; set; }

    public int BabyId { get; set; }

    public Baby? Baby { get; set; }

    public DateTimeOffset StartUtc { get; set; }

    public DateTimeOffset? EndUtc { get; set; }

    public FeedingLogType Type { get; set; }

    public BreastSide? BreastSide { get; set; }

    public int? AmountMl { get; set; }

    public string? FoodDescription { get; set; }

    public string? Notes { get; set; }
}
