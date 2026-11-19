using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CIHAProjectTracker.Migrations
{
    /// <inheritdoc />
    public partial class ManualModeIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsManualProgress",
                table: "Tasks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ManualPercent",
                table: "Tasks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsManualProgress",
                table: "Items",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ManualPercent",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsManualProgress",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "ManualPercent",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "IsManualProgress",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ManualPercent",
                table: "Items");
        }
    }
}
