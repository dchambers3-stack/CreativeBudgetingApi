using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreativeBudgeting.Migrations
{
    /// <inheritdoc />
    public partial class AddExpenes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "paycheck_amount_1",
                table: "personal_info");

            migrationBuilder.DropColumn(
                name: "paycheck_amount_2",
                table: "personal_info");

            migrationBuilder.AddColumn<int>(
                name: "PaycheckId",
                table: "Expenses",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_PaycheckId",
                table: "Expenses",
                column: "PaycheckId");

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Paychecks_PaycheckId",
                table: "Expenses",
                column: "PaycheckId",
                principalTable: "Paychecks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Paychecks_PaycheckId",
                table: "Expenses");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_PaycheckId",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "PaycheckId",
                table: "Expenses");

            migrationBuilder.AddColumn<double>(
                name: "paycheck_amount_1",
                table: "personal_info",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "paycheck_amount_2",
                table: "personal_info",
                type: "double precision",
                nullable: true);
        }
    }
}
