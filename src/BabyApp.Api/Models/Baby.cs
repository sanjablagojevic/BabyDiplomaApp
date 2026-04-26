namespace BabyApp.Api.Models;

public class Baby
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public DateOnly DateOfBirth { get; set; }

    public BabySex Sex { get; set; }

    /// <summary>Primarna rutina prije uvoda u čvrstu hranu (cca do 4–4,5 mj).</summary>
    public InfantMilkRoutine? InfantMilkRoutine { get; set; }

    /// <summary>Ciljani broj obroka čvrstom hranom (0 = još nije počelo).</summary>
    public int SolidMealsPerDayGoal { get; set; }

    public string ParentId { get; set; } = null!;

    public ApplicationUser? Parent { get; set; }

    public ICollection<DailyWakeLog> DailyWakeLogs { get; set; } = new List<DailyWakeLog>();
    public ICollection<SleepSession> SleepSessions { get; set; } = new List<SleepSession>();
    public ICollection<FeedingLog> FeedingLogs { get; set; } = new List<FeedingLog>();
    public ICollection<ReactionLog> ReactionLogs { get; set; } = new List<ReactionLog>();
    public ICollection<GrowthMeasurement> GrowthMeasurements { get; set; } = new List<GrowthMeasurement>();
    public ICollection<MilestoneAchievement> MilestoneAchievements { get; set; } = new List<MilestoneAchievement>();
    public ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();
}
