using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoTrongHung2280601119.Migrations
{
    /// <inheritdoc />
    public partial class AddSlugToWarehouse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Warehouses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Warehouses");
        }
    }
}
