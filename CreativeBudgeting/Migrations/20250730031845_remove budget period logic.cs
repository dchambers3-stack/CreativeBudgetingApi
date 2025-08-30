using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CreativeBudgeting.Migrations
{
    /// <inheritdoc />
    public partial class removebudgetperiodlogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_budget_period_BudgetPeriodId",
                table: "Expenses");

            migrationBuilder.DropForeignKey(
                name: "FK_Paychecks_budget_period_BudgetPeriodId",
                table: "Paychecks");

            migrationBuilder.DropTable(
                name: "budget_period");

            migrationBuilder.DropIndex(
                name: "IX_Paychecks_BudgetPeriodId",
                table: "Paychecks");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_BudgetPeriodId",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "BudgetPeriodId",
                table: "Paychecks");

            migrationBuilder.DropColumn(
                name: "BudgetPeriodId",
                table: "Expenses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BudgetPeriodId",
                table: "Paychecks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BudgetPeriodId",
                table: "Expenses",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "budget_period",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    NextMonthName = table.Column<string>(type: "text", nullable: true),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_budget_period", x => x.Id);
                    table.ForeignKey(
                        name: "FK_budget_period_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Paychecks_BudgetPeriodId",
                table: "Paychecks",
                column: "BudgetPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_BudgetPeriodId",
                table: "Expenses",
                column: "BudgetPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_budget_period_UserId",
                table: "budget_period",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_budget_period_BudgetPeriodId",
                table: "Expenses",
                column: "BudgetPeriodId",
                principalTable: "budget_period",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Paychecks_budget_period_BudgetPeriodId",
                table: "Paychecks",
                column: "BudgetPeriodId",
                principalTable: "budget_period",
                principalColumn: "Id");
        }
    }
}
