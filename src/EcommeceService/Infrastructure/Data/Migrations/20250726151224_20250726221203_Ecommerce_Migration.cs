using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250726221203_Ecommerce_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "tariff_id",
                table: "order",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_order_tariff_id",
                table: "order",
                column: "tariff_id");

            migrationBuilder.AddForeignKey(
                name: "fk_order_tariff_tariff_id",
                table: "order",
                column: "tariff_id",
                principalTable: "tariff",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_order_tariff_tariff_id",
                table: "order");

            migrationBuilder.DropIndex(
                name: "ix_order_tariff_id",
                table: "order");

            migrationBuilder.DropColumn(
                name: "tariff_id",
                table: "order");
        }
    }
}
