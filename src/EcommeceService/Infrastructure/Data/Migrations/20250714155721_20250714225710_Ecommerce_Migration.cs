using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250714225710_Ecommerce_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "image",
                table: "equipment");

            migrationBuilder.DropColumn(
                name: "barcode",
                table: "branch_product");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "equipment",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "equipment",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "image",
                table: "equipment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "barcode",
                table: "branch_product",
                type: "text",
                nullable: true);
        }
    }
}
