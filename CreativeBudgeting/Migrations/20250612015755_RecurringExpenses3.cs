using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreativeBudgeting.Migrations
{
    /// <inheritdoc />
    public partial class RecurringExpenses3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "recurring_expenses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_recurring_expenses_UserId",
                table: "recurring_expenses",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_recurring_expenses_Users_UserId",
                table: "recurring_expenses",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_recurring_expenses_Users_UserId",
                table: "recurring_expenses");

            migrationBuilder.DropIndex(
                name: "IX_recurring_expenses_UserId",
                table: "recurring_expenses");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "recurring_expenses");
        }
    }
}
