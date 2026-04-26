namespace BabyApp.Api.Models;

public class GrowthMeasurement
{
    public int Id { get; set; }

    public int BabyId { get; set; }

    public Baby? Baby { get; set; }

    public DateOnly MeasuredDate { get; set; }

    public decimal? WeightKg { get; set; }

    public decimal? HeightCm { get; set; }

    public decimal? HeadCircumferenceCm { get; set; }

    public string? Notes { get; set; }
}
