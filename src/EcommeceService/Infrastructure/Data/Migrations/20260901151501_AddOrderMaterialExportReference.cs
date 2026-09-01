using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderMaterialExportReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "source_order_id",
                table: "inventory_document",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_inventory_document_source_order_id",
                table: "inventory_document",
                column: "source_order_id",
                unique: true,
                filter: "source_order_id IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "fk_inventory_document_order_source_order_id",
                table: "inventory_document",
                column: "source_order_id",
                principalTable: "order",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_inventory_document_order_source_order_id",
                table: "inventory_document");

            migrationBuilder.DropIndex(
                name: "ix_inventory_document_source_order_id",
                table: "inventory_document");

            migrationBuilder.DropColumn(
                name: "source_order_id",
                table: "inventory_document");
        }
    }
}
