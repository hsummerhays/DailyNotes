using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyNotes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizAttemptTenantIdAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "quiz_attempts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_work_tasks_TenantId_UserId",
                table: "work_tasks",
                columns: new[] { "TenantId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_work_notes_TenantId_UserId",
                table: "work_notes",
                columns: new[] { "TenantId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_work_days_TenantId_UserId",
                table: "work_days",
                columns: new[] { "TenantId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_topic_notes_TenantId_UserId",
                table: "topic_notes",
                columns: new[] { "TenantId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_quiz_attempts_TenantId_UserId",
                table: "quiz_attempts",
                columns: new[] { "TenantId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_pay_periods_TenantId_UserId",
                table: "pay_periods",
                columns: new[] { "TenantId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_assignments_TenantId_UserId",
                table: "assignments",
                columns: new[] { "TenantId", "UserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_work_tasks_TenantId_UserId",
                table: "work_tasks");

            migrationBuilder.DropIndex(
                name: "IX_work_notes_TenantId_UserId",
                table: "work_notes");

            migrationBuilder.DropIndex(
                name: "IX_work_days_TenantId_UserId",
                table: "work_days");

            migrationBuilder.DropIndex(
                name: "IX_topic_notes_TenantId_UserId",
                table: "topic_notes");

            migrationBuilder.DropIndex(
                name: "IX_quiz_attempts_TenantId_UserId",
                table: "quiz_attempts");

            migrationBuilder.DropIndex(
                name: "IX_pay_periods_TenantId_UserId",
                table: "pay_periods");

            migrationBuilder.DropIndex(
                name: "IX_assignments_TenantId_UserId",
                table: "assignments");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "quiz_attempts");
        }
    }
}
