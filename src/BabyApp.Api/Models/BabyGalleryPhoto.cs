namespace BabyApp.Api.Models;

public class BabyGalleryPhoto
{
    public int Id { get; set; }

    public int BabyId { get; set; }

    public Baby? Baby { get; set; }

    /// <summary>Slot po mjesecu: 0 = just born, 1..11 mjeseci.</summary>
    public int MonthSlot { get; set; }

    /// <summary>Relativni path npr. "gallery/uuid.jpg"</summary>
    public required string RelativePath { get; set; }

    public DateTimeOffset UploadedUtc { get; set; }
}
