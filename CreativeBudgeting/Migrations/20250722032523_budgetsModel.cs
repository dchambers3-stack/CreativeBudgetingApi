using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CreativeBudgeting.Migrations
{
    /// <inheritdoc />
    public partial class budgetsModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_recurring_expenses_RecurringExpenseId",
                table: "Expenses");

            migrationBuilder.DropTable(
                name: "recurring_expenses");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_RecurringExpenseId",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "RecurringExpenseId",
                table: "Expenses");

            migrationBuilder.AlterColumn<double>(
                name: "TotalBalance",
                table: "Expenses",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

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
                    Name = table.Column<string>(type: "text", nullable: false),
                    start_date = table.Column<string>(type: "text", nullable: false),
                    end_date = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_budget_period", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_BudgetPeriodId",
                table: "Expenses",
                column: "BudgetPeriodId");

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_budget_period_BudgetPeriodId",
                table: "Expenses",
                column: "BudgetPeriodId",
                principalTable: "budget_period",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_budget_period_BudgetPeriodId",
                table: "Expenses");

            migrationBuilder.DropTable(
                name: "budget_period");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_BudgetPeriodId",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "BudgetPeriodId",
                table: "Expenses");

            migrationBuilder.AlterColumn<double>(
                name: "TotalBalance",
                table: "Expenses",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RecurringExpenseId",
                table: "Expenses",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "recurring_expenses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: true),
                    frequency_id = table.Column<int>(type: "integer", nullable: false),
                    PaycheckId = table.Column<int>(type: "integer", nullable: true),
                    SubcategoryId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    recurring_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    recurring_expense_name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recurring_expenses", x => x.id);
                    table.ForeignKey(
                        name: "FK_recurring_expenses_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_recurring_expenses_Paychecks_PaycheckId",
                        column: x => x.PaycheckId,
                        principalTable: "Paychecks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_recurring_expenses_RecurringFrequencies_frequency_id",
                        column: x => x.frequency_id,
                        principalTable: "RecurringFrequencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_recurring_expenses_Subcategories_SubcategoryId",
                        column: x => x.SubcategoryId,
                        principalTable: "Subcategories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_recurring_expenses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_RecurringExpenseId",
                table: "Expenses",
                column: "RecurringExpenseId");

            migrationBuilder.CreateIndex(
                name: "IX_recurring_expenses_CategoryId",
                table: "recurring_expenses",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_recurring_expenses_frequency_id",
                table: "recurring_expenses",
                column: "frequency_id");

            migrationBuilder.CreateIndex(
                name: "IX_recurring_expenses_PaycheckId",
                table: "recurring_expenses",
                column: "PaycheckId");

            migrationBuilder.CreateIndex(
                name: "IX_recurring_expenses_SubcategoryId",
                table: "recurring_expenses",
                column: "SubcategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_recurring_expenses_UserId",
                table: "recurring_expenses",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_recurring_expenses_RecurringExpenseId",
                table: "Expenses",
                column: "RecurringExpenseId",
                principalTable: "recurring_expenses",
                principalColumn: "id");
        }
    }
}
