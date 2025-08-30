using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreativeBudgeting.Migrations
{
    /// <inheritdoc />
    public partial class period : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BudgetPeriodId",
                table: "Paychecks",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Paychecks_BudgetPeriodId",
                table: "Paychecks",
                column: "BudgetPeriodId");

            migrationBuilder.AddForeignKey(
                name: "FK_Paychecks_budget_period_BudgetPeriodId",
                table: "Paychecks",
                column: "BudgetPeriodId",
                principalTable: "budget_period",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Paychecks_budget_period_BudgetPeriodId",
                table: "Paychecks");

            migrationBuilder.DropIndex(
                name: "IX_Paychecks_BudgetPeriodId",
                table: "Paychecks");

            migrationBuilder.DropColumn(
                name: "BudgetPeriodId",
                table: "Paychecks");
        }
    }
}
