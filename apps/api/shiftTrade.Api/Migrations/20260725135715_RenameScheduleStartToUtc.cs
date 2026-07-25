using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace shiftTrade.api.Migrations
{
    /// <inheritdoc />
    public partial class RenameScheduleStartToUtc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ScheduleStartUtx",
                table: "Shifts",
                newName: "ScheduleStartUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ScheduleStartUtc",
                table: "Shifts",
                newName: "ScheduleStartUtx");
        }
    }
}
