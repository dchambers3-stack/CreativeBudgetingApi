using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreativeBudgeting.Migrations
{
    /// <inheritdoc />
    public partial class MakePaycheckIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_recurring_expenses_Paychecks_PaycheckId",
                table: "recurring_expenses");

            migrationBuilder.AlterColumn<int>(
                name: "PaycheckId",
                table: "recurring_expenses",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_recurring_expenses_Paychecks_PaycheckId",
                table: "recurring_expenses",
                column: "PaycheckId",
                principalTable: "Paychecks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_recurring_expenses_Paychecks_PaycheckId",
                table: "recurring_expenses");

            migrationBuilder.AlterColumn<int>(
                name: "PaycheckId",
                table: "recurring_expenses",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_recurring_expenses_Paychecks_PaycheckId",
                table: "recurring_expenses",
                column: "PaycheckId",
                principalTable: "Paychecks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
