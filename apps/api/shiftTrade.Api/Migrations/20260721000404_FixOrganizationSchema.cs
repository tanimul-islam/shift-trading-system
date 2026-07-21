using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace shiftTrade.api.Migrations
{
    /// <inheritdoc />
    public partial class FixOrganizationSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Naem",
                table: "Organizations",
                newName: "Name");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Organizations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Organizations");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Organizations",
                newName: "Naem");
        }
    }
}
