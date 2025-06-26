using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CreativeBudgeting.Migrations
{
    /// <inheritdoc />
    public partial class Recurring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecurringFrequencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
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
                columns: new[] { "Id", "Name", "SortOrder", "Value" },
                values: new object[,]
                {
                    { 1, "Daily", 1, "daily" },
                    { 2, "Weekly", 2, "weekly" },
                    { 3, "Bi-Weekly", 3, "biweekly" },
                    { 4, "Monthly", 4, "monthly" },
                    { 5, "Yearly", 5, "yearly" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecurringFrequencies");
        }
    }
}
