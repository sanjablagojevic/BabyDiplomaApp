namespace BabyApp.Api.Models;

public enum BabySex
{
    Unknown = 0,
    Male = 1,
    Female = 2,
}

public enum InfantMilkRoutine
{
    Bottle = 0,
    Breast = 1,
}

public enum FeedingLogType
{
    Breast = 0,
    Bottle = 1,
    Solid = 2,
}

public enum ReactionKind
{
    Allergy = 0,
    FoodRefusal = 1,
    Constipation = 2,
}

public enum ReminderKind
{
    EveningSleep = 0,
    Meal = 1,
    GrowthEntry = 2,
    Vaccine = 3,
    Custom = 4,
}

public enum AudioResourceKind
{
    Lullaby = 0,
    WhiteNoise = 1,
}

public enum MilestoneKey
{
    HeadLifting = 0,
    Sitting = 1,
    Crawling = 2,
    FirstSteps = 3,
    FirstWords = 4,
}

public enum AgeBand
{
    ZeroToThreeMonths = 0,
    ThreeToSixMonths = 1,
    SixToTwelveMonths = 2,
}
