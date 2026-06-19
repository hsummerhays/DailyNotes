using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyNotes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMemoryItemFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Postgres has no implicit/assignment cast from text to integer; ALTER COLUMN TYPE
            // requires an explicit USING clause or it fails with "cannot be cast automatically".
            migrationBuilder.Sql(
                "ALTER TABLE memory_items ALTER COLUMN \"SourceEntityId\" TYPE integer USING \"SourceEntityId\"::integer;");

            migrationBuilder.AddColumn<int>(
                name: "AccessCount",
                table: "memory_items",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastConfirmedAt",
                table: "memory_items",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MemoryStatus",
                table: "memory_items",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessCount",
                table: "memory_items");

            migrationBuilder.DropColumn(
                name: "LastConfirmedAt",
                table: "memory_items");

            migrationBuilder.DropColumn(
                name: "MemoryStatus",
                table: "memory_items");

            migrationBuilder.Sql(
                "ALTER TABLE memory_items ALTER COLUMN \"SourceEntityId\" TYPE text USING \"SourceEntityId\"::text;");
        }
    }
}
