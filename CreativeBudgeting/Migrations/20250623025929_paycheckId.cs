using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreativeBudgeting.Migrations
{
    /// <inheritdoc />
    public partial class paycheckId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PaycheckId",
                table: "recurring_expenses",
                type: "integer",
                nullable: true
               );

            migrationBuilder.CreateIndex(
                name: "IX_recurring_expenses_PaycheckId",
                table: "recurring_expenses",
                column: "PaycheckId");

            migrationBuilder.AddForeignKey(
                name: "FK_recurring_expenses_Paychecks_PaycheckId",
                table: "recurring_expenses",
                column: "PaycheckId",
                principalTable: "Paychecks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_recurring_expenses_Paychecks_PaycheckId",
                table: "recurring_expenses");

            migrationBuilder.DropIndex(
                name: "IX_recurring_expenses_PaycheckId",
                table: "recurring_expenses");

            migrationBuilder.DropColumn(
                name: "PaycheckId",
                table: "recurring_expenses");
        }
    }
}
