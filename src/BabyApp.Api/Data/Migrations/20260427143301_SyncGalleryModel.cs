using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BabyApp.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class SyncGalleryModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BabyGalleryPhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BabyId = table.Column<int>(type: "int", nullable: false),
                    MonthSlot = table.Column<int>(type: "int", nullable: false),
                    RelativePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadedUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BabyGalleryPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BabyGalleryPhotos_Babies_BabyId",
                        column: x => x.BabyId,
                        principalTable: "Babies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BabyGalleryPhotos_BabyId_MonthSlot",
                table: "BabyGalleryPhotos",
                columns: new[] { "BabyId", "MonthSlot" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BabyGalleryPhotos");
        }
    }
}
