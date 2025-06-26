﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreativeBudgeting.Migrations
{
    /// <inheritdoc />
    public partial class IsPaid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "Expenses",
                type: "boolean",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "Expenses");
        }
    }
}
