namespace BabyApp.Api.Models;

public class EducationalArticle
{
    public int Id { get; set; }

    public AgeBand Band { get; set; }

    public required string Title { get; set; }

    public required string Body { get; set; }

    public int SortOrder { get; set; }
}
