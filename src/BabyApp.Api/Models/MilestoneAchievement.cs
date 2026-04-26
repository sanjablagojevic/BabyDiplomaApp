namespace BabyApp.Api.Models;

public class MilestoneAchievement
{
    public int Id { get; set; }

    public int BabyId { get; set; }

    public Baby? Baby { get; set; }

    public MilestoneKey Milestone { get; set; }

    public DateOnly AchievedOn { get; set; }

    public string? Notes { get; set; }
}
