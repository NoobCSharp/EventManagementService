using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventManagement.Bookings.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeSeatCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SeatCount",
                table: "Bookings",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SeatCount",
                table: "Bookings");
        }
    }
}
