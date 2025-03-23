using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250323171532_Project_Migration : Migration
    {
        /// <inheritdoc />
                protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "payment_method",
                table: "fund");
            migrationBuilder.DropColumn(
                name: "payment_method",
                table: "order_payment");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "payment_method",
                table: "fund",
                type: "text",
                nullable: false,
                defaultValue: "cash");
             migrationBuilder.AddColumn<string>(
                name: "payment_method",
                table: "order_payment",
                type: "text",
                nullable: false,
                defaultValue: "cash");
        }
    }
}
