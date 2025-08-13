using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20250812222048_Ecommerce_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_service_resource_branch_product_branch_product_id",
                table: "service_resource"
            );

            migrationBuilder.DropIndex(
                name: "ix_service_resource_branch_product_id",
                table: "service_resource"
            );

            migrationBuilder.DropColumn(name: "branch_product_id", table: "service_resource");

            migrationBuilder.CreateIndex(
                name: "ix_service_resource_product_id",
                table: "service_resource",
                column: "product_id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_service_resource_branch_product_product_id",
                table: "service_resource",
                column: "product_id",
                principalTable: "branch_product",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_service_resource_branch_product_product_id",
                table: "service_resource"
            );

            migrationBuilder.DropIndex(
                name: "ix_service_resource_product_id",
                table: "service_resource"
            );

            migrationBuilder.AddColumn<long>(
                name: "branch_product_id",
                table: "service_resource",
                type: "bigint",
                nullable: false,
                defaultValue: 0L
            );

            migrationBuilder.CreateIndex(
                name: "ix_service_resource_branch_product_id",
                table: "service_resource",
                column: "branch_product_id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_service_resource_branch_product_branch_product_id",
                table: "service_resource",
                column: "branch_product_id",
                principalTable: "branch_product",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );
        }
    }
}
