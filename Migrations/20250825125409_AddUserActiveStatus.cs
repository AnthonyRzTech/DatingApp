using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebMatcha.Migrations
{
    /// <inheritdoc />
    public partial class AddUserActiveStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "deactivated_at",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "deactivated_at",
                table: "users");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "users");
        }
    }
}
