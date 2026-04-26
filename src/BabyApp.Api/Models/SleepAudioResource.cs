namespace BabyApp.Api.Models;

public class SleepAudioResource
{
    public int Id { get; set; }

    public required string Title { get; set; }

    public AudioResourceKind Kind { get; set; }

    /// <summary>Vanjski link (npr. legalni streaming) ili interni put.</summary>
    public string? Url { get; set; }
}
