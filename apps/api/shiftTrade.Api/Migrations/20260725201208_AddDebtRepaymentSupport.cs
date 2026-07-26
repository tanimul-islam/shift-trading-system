using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace shiftTrade.api.Migrations
{
    /// <inheritdoc />
    public partial class AddDebtRepaymentSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           migrationBuilder.AddColumn<decimal>(
            name: "RemainingHours",
            table: "HoursDebts",
            type: "numeric(5,2)",
            precision: 5,
            scale: 2,
            nullable: false,
            defaultValue: 0m);

        migrationBuilder.Sql("""
            UPDATE "HoursDebts"
            SET "RemainingHours" = "HoursOwed"
            WHERE "Status" = 'Active';
            """);

            migrationBuilder.CreateTable(
                name: "DebtSettlements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceDebtId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetedDebtId = table.Column<Guid>(type: "uuid", nullable: false),
                    HoursApplied = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DebtSettlements", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DebtSettlements");

            migrationBuilder.DropColumn(
                name: "RemainingHours",
                table: "HoursDebts");
        }
    }
}
