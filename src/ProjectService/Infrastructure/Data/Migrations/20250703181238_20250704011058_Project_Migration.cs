using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250704011058_Project_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "public_id",
                table: "warehouse");

            migrationBuilder.DropColumn(
                name: "public_id",
                table: "branch_user");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "public_id",
                table: "warehouse",
                type: "character varying(26)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "public_id",
                table: "branch_user",
                type: "character varying(26)",
                nullable: false,
                defaultValue: "");
        }
    }
}
