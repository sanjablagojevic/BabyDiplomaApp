using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BabyApp.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class FullFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InfantMilkRoutine",
                table: "Babies",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Sex",
                table: "Babies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SolidMealsPerDayGoal",
                table: "Babies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DailyWakeLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BabyId = table.Column<int>(type: "int", nullable: false),
                    ForDate = table.Column<DateOnly>(type: "date", nullable: false),
                    MorningWakeTime = table.Column<TimeOnly>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyWakeLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyWakeLogs_Babies_BabyId",
                        column: x => x.BabyId,
                        principalTable: "Babies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EducationalArticles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Band = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationalArticles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeedingLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BabyId = table.Column<int>(type: "int", nullable: false),
                    StartUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EndUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    AmountMl = table.Column<int>(type: "int", nullable: true),
                    FoodDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedingLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeedingLogs_Babies_BabyId",
                        column: x => x.BabyId,
                        principalTable: "Babies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FoodSuggestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IntroFromMonth = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodSuggestions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GrowthMeasurements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BabyId = table.Column<int>(type: "int", nullable: false),
                    MeasuredDate = table.Column<DateOnly>(type: "date", nullable: false),
                    WeightKg = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    HeightCm = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    HeadCircumferenceCm = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrowthMeasurements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrowthMeasurements_Babies_BabyId",
                        column: x => x.BabyId,
                        principalTable: "Babies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MilestoneAchievements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BabyId = table.Column<int>(type: "int", nullable: false),
                    Milestone = table.Column<int>(type: "int", nullable: false),
                    AchievedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MilestoneAchievements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MilestoneAchievements_Babies_BabyId",
                        column: x => x.BabyId,
                        principalTable: "Babies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReactionLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BabyId = table.Column<int>(type: "int", nullable: false),
                    OccurredUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    FoodTrigger = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReactionLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReactionLogs_Babies_BabyId",
                        column: x => x.BabyId,
                        principalTable: "Babies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Recipes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MinAgeMonths = table.Column<int>(type: "int", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PdfFileName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recipes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reminders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BabyId = table.Column<int>(type: "int", nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LocalTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    VaccineName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VaccineDueDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reminders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reminders_Babies_BabyId",
                        column: x => x.BabyId,
                        principalTable: "Babies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SleepAudioResources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SleepAudioResources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SleepSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BabyId = table.Column<int>(type: "int", nullable: false),
                    StartUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EndUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsNap = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SleepSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SleepSessions_Babies_BabyId",
                        column: x => x.BabyId,
                        principalTable: "Babies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyWakeLogs_BabyId_ForDate",
                table: "DailyWakeLogs",
                columns: new[] { "BabyId", "ForDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeedingLogs_BabyId",
                table: "FeedingLogs",
                column: "BabyId");

            migrationBuilder.CreateIndex(
                name: "IX_GrowthMeasurements_BabyId",
                table: "GrowthMeasurements",
                column: "BabyId");

            migrationBuilder.CreateIndex(
                name: "IX_MilestoneAchievements_BabyId_Milestone",
                table: "MilestoneAchievements",
                columns: new[] { "BabyId", "Milestone" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReactionLogs_BabyId",
                table: "ReactionLogs",
                column: "BabyId");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_BabyId",
                table: "Reminders",
                column: "BabyId");

            migrationBuilder.CreateIndex(
                name: "IX_SleepSessions_BabyId",
                table: "SleepSessions",
                column: "BabyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyWakeLogs");

            migrationBuilder.DropTable(
                name: "EducationalArticles");

            migrationBuilder.DropTable(
                name: "FeedingLogs");

            migrationBuilder.DropTable(
                name: "FoodSuggestions");

            migrationBuilder.DropTable(
                name: "GrowthMeasurements");

            migrationBuilder.DropTable(
                name: "MilestoneAchievements");

            migrationBuilder.DropTable(
                name: "ReactionLogs");

            migrationBuilder.DropTable(
                name: "Recipes");

            migrationBuilder.DropTable(
                name: "Reminders");

            migrationBuilder.DropTable(
                name: "SleepAudioResources");

            migrationBuilder.DropTable(
                name: "SleepSessions");

            migrationBuilder.DropColumn(
                name: "InfantMilkRoutine",
                table: "Babies");

            migrationBuilder.DropColumn(
                name: "Sex",
                table: "Babies");

            migrationBuilder.DropColumn(
                name: "SolidMealsPerDayGoal",
                table: "Babies");
        }
    }
}
