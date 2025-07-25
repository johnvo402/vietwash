using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250719135022_Ecommerce_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "supervisor_code",
                table: "equipment_activity");

            migrationBuilder.DropColumn(
                name: "supervisor_name",
                table: "equipment_activity");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "supervisor_code",
                table: "equipment_activity",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "supervisor_name",
                table: "equipment_activity",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
