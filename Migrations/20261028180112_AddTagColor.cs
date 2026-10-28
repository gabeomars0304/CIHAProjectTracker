using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CIHAProjectTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddTagColor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ColorHex",
                table: "Tags",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 1,
                column: "ColorHex",
                value: null);

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 2,
                column: "ColorHex",
                value: null);

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 3,
                column: "ColorHex",
                value: null);

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 4,
                column: "ColorHex",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColorHex",
                table: "Tags");
        }
    }
}
