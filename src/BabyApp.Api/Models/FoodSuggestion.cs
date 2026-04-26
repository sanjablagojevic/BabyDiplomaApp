namespace BabyApp.Api.Models;

public class FoodSuggestion
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public int IntroFromMonth { get; set; }

    public string? Notes { get; set; }
}
