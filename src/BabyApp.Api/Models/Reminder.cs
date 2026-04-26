namespace BabyApp.Api.Models;

public class Reminder
{
    public int Id { get; set; }

    public int BabyId { get; set; }

    public Baby? Baby { get; set; }

    public ReminderKind Kind { get; set; }

    public required string Title { get; set; }

    public TimeOnly LocalTime { get; set; }

    public bool IsEnabled { get; set; } = true;

    public string? VaccineName { get; set; }

    public DateOnly? VaccineDueDate { get; set; }
}
