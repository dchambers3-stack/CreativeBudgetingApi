using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreativeBudgeting.Migrations
{
    /// <inheritdoc />
    public partial class RecreateRecurringFrequencies : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // RecurringFrequencies table
            migrationBuilder.CreateTable(
                name: "RecurringFrequencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecurringFrequencies", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "RecurringFrequencies",
                columns: new[] { "Id", "Name", "Value", "SortOrder" },
                values: new object[,]
                {
                    { 1, "Daily", "daily", 1 },
                    { 2, "Weekly", "weekly", 2 },
                    { 3, "Bi-Weekly", "biweekly", 3 },
                    { 4, "Monthly", "monthly", 4 },
                    { 5, "Yearly", "yearly", 5 }
                });

            // RecurringExpenses table
            migrationBuilder.CreateTable(
                name: "recurring_expenses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    recurring_expense_name = table.Column<string>(type: "text", nullable: false),
                    recurring_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    frequency_id = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recurring_expenses", x => x.id);
                    table.ForeignKey(
                        name: "FK_recurring_expenses_RecurringFrequencies_frequency_id",
                        column: x => x.frequency_id,
                        principalTable: "RecurringFrequencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_recurring_expenses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_recurring_expenses_frequency_id",
                table: "recurring_expenses",
                column: "frequency_id");

            migrationBuilder.CreateIndex(
                name: "IX_recurring_expenses_UserId",
                table: "recurring_expenses",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recurring_expenses");

            migrationBuilder.DropTable(
                name: "RecurringFrequencies");
        }
    }
}