using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreativeBudgeting.Migrations
{
    /// <inheritdoc />
    public partial class updatesToBudget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        ALTER TABLE budget_period
        ALTER COLUMN start_date TYPE timestamp with time zone
        USING start_date::timestamp with time zone;

        ALTER TABLE budget_period
        ALTER COLUMN end_date TYPE timestamp with time zone
        USING end_date::timestamp with time zone;
    ");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "budget_period",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_budget_period_UserId",
                table: "budget_period",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_budget_period_Users_UserId",
                table: "budget_period",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_budget_period_Users_UserId",
                table: "budget_period");

            migrationBuilder.DropIndex(
                name: "IX_budget_period_UserId",
                table: "budget_period");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "budget_period");

            migrationBuilder.AlterColumn<string>(
                name: "start_date",
                table: "budget_period",
                type: "text",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "end_date",
                table: "budget_period",
                type: "text",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");
        }
    }
}
