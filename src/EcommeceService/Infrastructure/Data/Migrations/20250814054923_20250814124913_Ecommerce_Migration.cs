using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250814124913_Ecommerce_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_unit_relation_branch_product_branch_product_id",
                table: "unit_relation");

            migrationBuilder.AddForeignKey(
                name: "fk_unit_relation_branch_product_branch_product_id",
                table: "unit_relation",
                column: "branch_product_id",
                principalTable: "branch_product",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_unit_relation_branch_product_branch_product_id",
                table: "unit_relation");

            migrationBuilder.AddForeignKey(
                name: "fk_unit_relation_branch_product_branch_product_id",
                table: "unit_relation",
                column: "branch_product_id",
                principalTable: "branch_product",
                principalColumn: "id");
        }
    }
}
