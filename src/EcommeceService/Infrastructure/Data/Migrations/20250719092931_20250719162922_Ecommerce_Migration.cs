using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250719162922_Ecommerce_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "voucher_code",
                table: "order",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_voucher_code",
                table: "voucher",
                column: "code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_voucher_code",
                table: "voucher");

            migrationBuilder.DropColumn(
                name: "voucher_code",
                table: "order");
        }
    }
}
