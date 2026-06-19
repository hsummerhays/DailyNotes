using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyNotes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMemoryItemConfidenceAndExcerpt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ConfidenceScore",
                table: "memory_items",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "SourceExcerpt",
                table: "memory_items",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfidenceScore",
                table: "memory_items");

            migrationBuilder.DropColumn(
                name: "SourceExcerpt",
                table: "memory_items");
        }
    }
}
