using BabyApp.Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BabyApp.Api.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Baby> Babies => Set<Baby>();
    public DbSet<DailyWakeLog> DailyWakeLogs => Set<DailyWakeLog>();
    public DbSet<SleepSession> SleepSessions => Set<SleepSession>();
    public DbSet<SleepAudioResource> SleepAudioResources => Set<SleepAudioResource>();
    public DbSet<FeedingLog> FeedingLogs => Set<FeedingLog>();
    public DbSet<FoodSuggestion> FoodSuggestions => Set<FoodSuggestion>();
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<ReactionLog> ReactionLogs => Set<ReactionLog>();
    public DbSet<GrowthMeasurement> GrowthMeasurements => Set<GrowthMeasurement>();
    public DbSet<MilestoneAchievement> MilestoneAchievements => Set<MilestoneAchievement>();
    public DbSet<Reminder> Reminders => Set<Reminder>();
    public DbSet<EducationalArticle> EducationalArticles => Set<EducationalArticle>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Baby>(entity =>
        {
            entity.HasOne(b => b.Parent)
                .WithMany()
                .HasForeignKey(b => b.ParentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<DailyWakeLog>(entity =>
        {
            entity.HasIndex(x => new { x.BabyId, x.ForDate }).IsUnique();
            entity.HasOne(x => x.Baby)
                .WithMany(b => b.DailyWakeLogs)
                .HasForeignKey(x => x.BabyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<SleepSession>(entity =>
        {
            entity.HasOne(x => x.Baby)
                .WithMany(b => b.SleepSessions)
                .HasForeignKey(x => x.BabyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<FeedingLog>(entity =>
        {
            entity.HasOne(x => x.Baby)
                .WithMany(b => b.FeedingLogs)
                .HasForeignKey(x => x.BabyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ReactionLog>(entity =>
        {
            entity.HasOne(x => x.Baby)
                .WithMany(b => b.ReactionLogs)
                .HasForeignKey(x => x.BabyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<GrowthMeasurement>(entity =>
        {
            entity.HasOne(x => x.Baby)
                .WithMany(b => b.GrowthMeasurements)
                .HasForeignKey(x => x.BabyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<MilestoneAchievement>(entity =>
        {
            entity.HasIndex(x => new { x.BabyId, x.Milestone }).IsUnique();
            entity.HasOne(x => x.Baby)
                .WithMany(b => b.MilestoneAchievements)
                .HasForeignKey(x => x.BabyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Reminder>(entity =>
        {
            entity.HasOne(x => x.Baby)
                .WithMany(b => b.Reminders)
                .HasForeignKey(x => x.BabyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<GrowthMeasurement>(entity =>
        {
            entity.Property(g => g.WeightKg).HasPrecision(6, 3);
            entity.Property(g => g.HeightCm).HasPrecision(6, 2);
            entity.Property(g => g.HeadCircumferenceCm).HasPrecision(6, 2);
        });
    }
}
