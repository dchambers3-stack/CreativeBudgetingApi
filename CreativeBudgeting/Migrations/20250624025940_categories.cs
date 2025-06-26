using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreativeBudgeting.Migrations
{
    /// <inheritdoc />
    public partial class categories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "recurring_expenses",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubcategoryId",
                table: "recurring_expenses",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_recurring_expenses_CategoryId",
                table: "recurring_expenses",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_recurring_expenses_SubcategoryId",
                table: "recurring_expenses",
                column: "SubcategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_recurring_expenses_Categories_CategoryId",
                table: "recurring_expenses",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_recurring_expenses_Subcategories_SubcategoryId",
                table: "recurring_expenses",
                column: "SubcategoryId",
                principalTable: "Subcategories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_recurring_expenses_Categories_CategoryId",
                table: "recurring_expenses");

            migrationBuilder.DropForeignKey(
                name: "FK_recurring_expenses_Subcategories_SubcategoryId",
                table: "recurring_expenses");

            migrationBuilder.DropIndex(
                name: "IX_recurring_expenses_CategoryId",
                table: "recurring_expenses");

            migrationBuilder.DropIndex(
                name: "IX_recurring_expenses_SubcategoryId",
                table: "recurring_expenses");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "recurring_expenses");

            migrationBuilder.DropColumn(
                name: "SubcategoryId",
                table: "recurring_expenses");
        }
    }
}
