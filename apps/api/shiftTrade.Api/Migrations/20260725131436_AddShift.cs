using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace shiftTrade.api.Migrations
{
    /// <inheritdoc />
    public partial class AddShift : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Shifts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PostedByUserId = table.Column<string>(type: "text", nullable: false),
                    ScheduleStartUtx = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ScheduleEndUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Staus = table.Column<string>(type: "text", nullable: false),
                    AcceptedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AcceptedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shifts", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Shifts");
        }
    }
}
