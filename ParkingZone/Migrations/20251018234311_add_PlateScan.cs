using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkingZone.Migrations
{
    /// <inheritdoc />
    public partial class add_PlateScan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlateScans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Plate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Digits = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Confidence = table.Column<float>(type: "real", nullable: false),
                    BoxX = table.Column<int>(type: "int", nullable: true),
                    BoxY = table.Column<int>(type: "int", nullable: true),
                    BoxW = table.Column<int>(type: "int", nullable: true),
                    BoxH = table.Column<int>(type: "int", nullable: true),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlateScans", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlateScans");
        }
    }
}
