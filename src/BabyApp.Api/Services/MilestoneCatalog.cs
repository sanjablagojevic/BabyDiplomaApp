using BabyApp.Api.Models;

namespace BabyApp.Api.Services;

public static class MilestoneCatalog
{
    public sealed record Item(MilestoneKey Key, string Title, int TypicalFromWeeks, int TypicalToWeeks);

    public static IReadOnlyList<Item> All { get; } =
    [
        new Item(MilestoneKey.HeadLifting, "Podizanje / držanje glave (tummy time)", 1, 12),
        new Item(MilestoneKey.Sitting, "Samostalno sjedenje", 20, 32),
        new Item(MilestoneKey.Crawling, "Pužanje", 28, 40),
        new Item(MilestoneKey.FirstSteps, "Prvi koraci", 40, 52),
        new Item(MilestoneKey.FirstWords, "Prve riječi", 48, 60),
    ];
}
