namespace BabyApp.Api.Models;

public class Recipe
{
    public int Id { get; set; }

    public required string Title { get; set; }

    public int MinAgeMonths { get; set; }

    public string? Summary { get; set; }

    /// <summary>Relativno unutar wwwroot/recipes npr. "bebe-smoothie.pdf".</summary>
    public string? PdfFileName { get; set; }
}
