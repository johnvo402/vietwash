using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250323174405_Project_Migration : Migration
    {
        /// <inheritdoc />
       protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "payment_method",
                table: "fund",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)1);
             migrationBuilder.AddColumn<byte>(
                name: "payment_method",
                table: "order_payment",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "payment_method",
                table: "fund");
            migrationBuilder.DropColumn(
                name: "payment_method",
                table: "order_payment");
        }
    }
}
