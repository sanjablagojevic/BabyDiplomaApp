using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BabyApp.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedingBreastSide : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BreastSide",
                table: "FeedingLogs",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BreastSide",
                table: "FeedingLogs");
        }
    }
}
