using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace shiftTrade.api.Migrations
{
    /// <inheritdoc />
    public partial class HoursDebt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HoursDebts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShiftId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreditorUserId = table.Column<string>(type: "text", nullable: false),
                    DebitorUserId = table.Column<string>(type: "text", nullable: false),
                    HoursOwed = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreateAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoursDebts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HoursDebts_ShiftId",
                table: "HoursDebts",
                column: "ShiftId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HoursDebts");
        }
    }
}
