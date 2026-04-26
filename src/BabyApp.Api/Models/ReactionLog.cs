namespace BabyApp.Api.Models;

public class ReactionLog
{
    public int Id { get; set; }

    public int BabyId { get; set; }

    public Baby? Baby { get; set; }

    public DateTimeOffset OccurredUtc { get; set; }

    public ReactionKind Kind { get; set; }

    public string? FoodTrigger { get; set; }

    public string? Notes { get; set; }
}
