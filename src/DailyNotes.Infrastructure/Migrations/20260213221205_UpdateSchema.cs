using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyNotes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Manual schema update already applied by user. 
            // This migration is empty to sync EF history.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_work_notes_work_days_NoteDate",
                table: "work_notes");

            migrationBuilder.DropIndex(
                name: "IX_work_notes_NoteDate",
                table: "work_notes");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_work_days_WorkDate",
                table: "work_days");

            migrationBuilder.DropColumn(
                name: "Comments",
                table: "work_tasks");

            migrationBuilder.DropColumn(
                name: "Holidays",
                table: "pay_periods");

            migrationBuilder.DropColumn(
                name: "PtoDaysOfMonth",
                table: "pay_periods");

            migrationBuilder.DropColumn(
                name: "PtoReported",
                table: "pay_periods");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "work_tasks",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DueDate",
                table: "work_tasks",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "NoteDate",
                table: "work_notes",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AddColumn<int>(
                name: "WorkDayId",
                table: "work_notes",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "WorkDate",
                table: "work_days",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeOut3",
                table: "work_days",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeOut2",
                table: "work_days",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeOut1",
                table: "work_days",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeIn3",
                table: "work_days",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeIn2",
                table: "work_days",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeIn1",
                table: "work_days",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_work_notes_WorkDayId",
                table: "work_notes",
                column: "WorkDayId");

            migrationBuilder.AddForeignKey(
                name: "FK_work_notes_work_days_WorkDayId",
                table: "work_notes",
                column: "WorkDayId",
                principalTable: "work_days",
                principalColumn: "Id");
        }
    }
}
